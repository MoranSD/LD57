using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public static class G
    {
        public static AudioSystem Audio;
        public static CameraHandle Camera;
        public static GameUI UI;
        public static HUD HUD;
        public static Main Main;
        public static GameState State;
    }
    [Serializable]
    public class GameState
    {
        public int Level;
        public int Grade;

        public bool SelectingHole;
        public HoleState LeftHoleInfo;
        public HoleState RightHoleInfo;

        public bool Fighting;
        public EntityState EnemyState;

        public EntityState PlayerState;
    }
    public class Main : MonoBehaviour
    {
        [Header("Main")]
        public GameState State;
        public int GradeCost = 5;

        public Interactor Interactor;

        [Header("Hole")]
        public GameObject HoleSelectionSceneParent;
        public HoleView LeftHole;
        public HoleView RightHole;
        [Header("Fight")]
        public GameObject FightingSceneParent;
        public EntityView PlayerFightView;
        public EntityView EnemyView;

        private Dictionary<int, GenerationInfo> generationInfosMap = new();

        private void Awake()
        {
            Interactor = new Interactor();
            Interactor.Init();
            CMS.Init();

            G.State = new GameState();
            State = G.State;
            G.Main = this;

            LeftHole.OnPressed += OnSelectHole;
            RightHole.OnPressed += OnSelectHole;
        }

        private void Start()
        {
            G.State.PlayerState = new EntityState();
            G.State.PlayerState.SetModel(GameResources.CMS.PlayerModel.AsEntity());
            LoadGenerationInfo();
            StartCoroutine(EnterHoleSelectionScene());
        }
        private void OnDestroy()
        {
            LeftHole.OnPressed -= OnSelectHole;
            RightHole.OnPressed -= OnSelectHole;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.W))
            {

            }
#endif
        }

        private IEnumerator UnloadScene()
        {
            G.State.SelectingHole = false;
            G.State.LeftHoleInfo = null;
            G.State.RightHoleInfo = null;
            HoleSelectionSceneParent.SetActive(false);

            G.State.Fighting = false;
            G.State.EnemyState = null;
            FightingSceneParent.SetActive(false);
            yield break;
        }
        private IEnumerator EnterNextHole()
        {
            G.State.Level++;
            G.State.Grade = Mathf.Min(G.State.Level / GradeCost, generationInfosMap.Count - 1);
            yield return EnterHoleSelectionScene();
        }
        private IEnumerator EnterHoleSelectionScene()
        {
            yield return UnloadScene();

            for (int i = 0; i < 2; i++)
            {
                var holeState = new HoleState();

                var holeInfo = GetCurrentGenInfo().HoleInfo.Get<TagHoleGenerationInfo>();
                var r = UnityEngine.Random.Range(0, 101);
                bool shouldSetAbility = r != 0 && r <= holeInfo.HoleAbilityChance;

                if (shouldSetAbility)
                {
                    var abilityIds = holeInfo.AbilityLinks.Select(x => x.Id).ToList();
                    abilityIds.Shuffle();
                    var randomAbilityId = abilityIds[0];
                    holeState.Ability = CMS.Get<CMSEntity>(randomAbilityId);
                }

                if(i == 0)
                {
                    G.State.LeftHoleInfo = holeState;
                    LeftHole.SetState(holeState);
                }
                else
                {
                    G.State.RightHoleInfo = holeState;
                    RightHole.SetState(holeState);
                }
            }

            G.State.SelectingHole = true;
            HoleSelectionSceneParent.SetActive(true);

            yield break;
        }
        private void OnSelectHole(HoleState state)
        {
            if (!G.State.SelectingHole) return;

            StartCoroutine(EnterHoleProcess(state));
        }
        private IEnumerator EnterHoleProcess(HoleState holeState)
        {
            G.State.SelectingHole = false;

            if (holeState.HasAbility)
            {
                var abilityState = new AbilityState();
                abilityState.SetModel(holeState.Ability);

                var player = G.State.PlayerState;
                var currentAbilities = G.State.PlayerState.Abilities;

                if (player.AbilitiesCount < player.MaxAbilitiesCount)//has place for new ability
                {
                    int freeSlotId = -1;
                    for (int i = 0; i < currentAbilities.Length; i++)
                    {
                        if (currentAbilities[i] != null) continue;

                        freeSlotId = i;
                        break;
                    }

                    yield return G.HUD.AbilitySelectionPanel.ShowAbilityMoveToSlot(abilityState, player, freeSlotId);

                    currentAbilities[freeSlotId] = abilityState;
                }
                else
                {
                    yield return G.HUD.AbilitySelectionPanel.SelectAbilitySlot(abilityState, player);

                    var selectedSlotId = G.HUD.AbilitySelectionPanel.SelectedNewAbilitySlotId;
                    currentAbilities[selectedSlotId] = abilityState;
                }
            }

            var holeInfo = GetCurrentGenInfo().HoleInfo.Get<TagHoleGenerationInfo>();
            var r = UnityEngine.Random.Range(0, 101);
            bool isEnemyExists = r != 0 && r <= holeInfo.EnemyExistentChance;

            if (isEnemyExists)
            {
                yield return EnterFightScene();
            }
            else
            {
                yield return EnterNextHole();
            }
        }

        private IEnumerator EnterFightScene()
        {
            yield return UnloadScene();

            var enemyInfo = GetCurrentGenInfo().EnemyInfo.Get<TagEnemyGenerationInfo>();
            var entities = enemyInfo.EntityLinks;
            var targetEntityLink = entities[UnityEngine.Random.Range(0, entities.Count)];

            PlayerFightView.SetState(G.State.PlayerState);

            G.State.EnemyState = new();
            G.State.EnemyState.SetModel(CMS.Get<CMSEntity>(targetEntityLink.Id));
            EnemyView.SetState(G.State.EnemyState);

            G.State.Fighting = true;
            FightingSceneParent.SetActive(true);

            yield return BeginFightCycle();
        }
        private IEnumerator BeginFightCycle()
        {
            /*
             * как собственно и раньше
             * 
             * вызываем ивент начала цикла
             * вызываем ивент конца цикла игрока
             * вызываем ивент конца цикла врага
             * 
             * враг по сути выбирает 1 из всех абилок и использует ее
             * 
             * проверку на то, стоит ли использовать способность врагу
             * можно сделать через "фильтры", которые будут принимать PropertyLink<bool>
             * 
             * тогда можно будет быстро прописывать это после создания способности
             */

            yield break;
        }

        private GenerationInfo GetCurrentGenInfo() => generationInfosMap[G.State.Grade];
        private void LoadGenerationInfo()
        {
            generationInfosMap.Clear();

            int grade = 0;
            while (true)
            {
                string holeInfoPath = $"CMS/IgnoreTag_LevelGeneration/{grade}/HoleInfo";
                string enemyInfoPath = $"CMS/IgnoreTag_LevelGeneration/{grade}/EnemyInfo";

                var generationInfo = new GenerationInfo();
                generationInfo.Grade = grade;

                var holeInfo = Resources.Load<CMSEntityPfb>(holeInfoPath);
                if (holeInfo != null)
                    generationInfo.HoleInfo = holeInfo.AsEntity();

                var enemyInfo = Resources.Load<CMSEntityPfb>(enemyInfoPath);
                if (enemyInfo != null)
                    generationInfo.EnemyInfo = enemyInfo.AsEntity();

                if (holeInfo == null || enemyInfo == null)
                {
                    Debug.Log($"No infos on grade {grade}. Stopping loading");
                    break;
                }

                generationInfosMap.Add(grade, generationInfo);
                grade++;
            }
        }
    }
}