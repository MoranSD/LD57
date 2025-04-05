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

        public EntityState PlayerState;
    }
    public class Main : MonoBehaviour
    {
        public Interactor Interactor;

        [Header("Hole")]
        public GameObject HoleSelectionParent;
        public HoleView LeftHole;
        public HoleView RightHole;

        private Dictionary<int, GenerationInfo> generationInfosMap = new();

        private void Awake()
        {
            Interactor = new Interactor();
            Interactor.Init();
            CMS.Init();

            G.State = new GameState();
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
            HoleSelectionParent.SetActive(false);
            yield break;
        }
        private IEnumerator EnterHoleSelectionScene()
        {
            yield return UnloadScene();

            for (int i = 0; i < 2; i++)
            {
                var holeState = new HoleState();

                var chancesInfo = generationInfosMap[G.State.Grade].HoleInfo.Get<TagHoleGenerationChances>();
                bool shouldSetAbility = UnityEngine.Random.Range(0, 101) < chancesInfo.HoleAbilityChance;

                if (shouldSetAbility)
                {
                    var abilityIds = generationInfosMap[G.State.Grade].HoleInfo.Get<TagAbilitiesHolder>().All.Select(x => x.Id).ToList();
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
            HoleSelectionParent.SetActive(true);

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

                if (player.AbilitiesCount < player.MaxAbilitiesCount)//has place for new ability
                {
                    int freeSlotId = -1;
                    var currentAbilities = G.State.PlayerState.Abilities;
                    for (int i = 0; i < currentAbilities.Length; i++)
                    {
                        if (currentAbilities[i] != null) continue;

                        freeSlotId = i;
                        break;
                    }

                    yield return G.HUD.AbilitySelectionPanel.ShowAbilityMoveToSlot(abilityState, player, freeSlotId);

                    G.State.PlayerState.Abilities[freeSlotId] = abilityState;
                }
                else
                {

                    yield return G.HUD.AbilitySelectionPanel.SelectAbilitySlot(abilityState, player);

                    var selectedSlotId = G.HUD.AbilitySelectionPanel.SelectedNewAbilitySlotId;
                    G.State.PlayerState.Abilities[selectedSlotId] = abilityState;
                }
            }

            //move to fight or next hole
            yield break;
        }

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

                if (holeInfo == null && enemyInfo == null)
                {
                    Debug.Log($"No infos on grade {grade}. Stopping loading");
                    break;
                }

                generationInfosMap.Add(grade, generationInfo);
                grade++;
            }

            Debug.Log($"Loaded generation grades: {generationInfosMap.Count}");
        }
    }
}