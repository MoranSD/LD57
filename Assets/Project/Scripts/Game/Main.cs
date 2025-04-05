using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
        public TurnTeam FightTurnTeam = TurnTeam.NoOne;
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

        private bool usingAbility;

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

            G.HUD.EndTurnButton.onClick.AddListener(OnPressEndTurn);
            G.HUD.AbilitiesPanel.OnSelectAbility += OnSelectFightAbility;
        }
        private void OnDestroy()
        {
            LeftHole.OnPressed -= OnSelectHole;
            RightHole.OnPressed -= OnSelectHole;

            G.HUD.EndTurnButton.onClick.RemoveAllListeners();
            G.HUD.AbilitiesPanel.OnSelectAbility -= OnSelectFightAbility;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.W))
            {

            }
#endif
        }

        public EntityState GetOpponent(EntityState entity)
        {
            if (entity.Model.Is<TagPlayer>()) return G.State.EnemyState;
            else return G.State.PlayerState;
        }
        public IEnumerator UseAbility(EntityState owner, AbilityState ability)
        {
            if (usingAbility) yield break;
            if (owner.UsedAbilitiesCount > 0) yield break;

            var filters = Interactor.FindAll<ICanUseAbilityFilter>();
            foreach (var filter in filters)
                if (filter.CanUse(owner, ability) == false)
                    yield break;

            usingAbility = true;

            owner.UsedAbilitiesCount++;
            var inters = Interactor.FindAll<IOnUseAbility>();
            foreach (var inter in inters)
                yield return inter.OnUseAbility(owner, ability);

            usingAbility = false;
        }
        public IEnumerator ApplyDamage(EntityState attacker, EntityState entity, float damage)
        {
            //var damageProperty = new PropertyLink<float>(damage);

            //var inters = Interactor.FindAll<IOnEntityApplyDamage>();
            //foreach (var inter in inters)
            //    yield return inter.OnEntityApplyDamage(attacker, entity, damageProperty);

            //damageProperty.Value

            if (entity.Armor > 0)
            {
                if(entity.Armor >= damage)
                {
                    entity.Armor -= damage;
                    entity.View.UpdateArmor();
                }
                else
                {
                    var remainingDamage = damage - entity.Armor;
                    entity.Armor = 0;
                    entity.Health -= remainingDamage;
                    entity.View.UpdateArmor();
                    entity.View.UpdateHealth();
                }
            }
            else
            {
                entity.Health -= damage;
                entity.View.UpdateHealth();
            }

            if (entity.Health <= 0)
            {
                //var inters2 = Interactor.FindAll<IOnBeforeEntityDie>();
                //foreach (var inter in inters2)
                //    yield return inter.OnBeforeEntityDie(entity);

                if (entity.Health <= 0)
                {
                    entity.View.DrawDie();
                }
            }

            yield break;
        }

        private IEnumerator UnloadScene()
        {
            G.State.SelectingHole = false;
            G.State.LeftHoleInfo = null;
            G.State.RightHoleInfo = null;
            HoleSelectionSceneParent.SetActive(false);

            G.State.Fighting = false;
            G.State.FightTurnTeam = TurnTeam.NoOne;
            G.State.EnemyState = null;
            FightingSceneParent.SetActive(false);
            yield break;
        }
        private IEnumerator EnterNextHoleSelectionScene()
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
                yield return EnterNextHoleSelectionScene();
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

            G.State.FightTurnTeam = TurnTeam.NoOne;
            G.State.Fighting = true;
            FightingSceneParent.SetActive(true);

            yield return BeginFightCycle();
        }
        private IEnumerator BeginFightCycle()
        {
            G.State.FightTurnTeam = TurnTeam.NoOne;

            var inters = Interactor.FindAll<IOnTurnBegin>();
            foreach (var inter in inters)
                yield return inter.OnTurnBegin(TurnTeam.Player);

            G.State.FightTurnTeam = TurnTeam.Player;
            G.HUD.EnableFightHud();
        }
        private void OnPressEndTurn()
        {
            if (G.State.FightTurnTeam != TurnTeam.Player) return;

            StartCoroutine(BeginEnemyFightCycle());
        }
        private void OnSelectFightAbility(int id)
        {
            if (G.State.FightTurnTeam != TurnTeam.Player) return;
            if (usingAbility) return;
            if (id < 0 || id >= G.State.PlayerState.Abilities.Length) return;
            if (G.State.PlayerState.Abilities[id] == null) return;

            StartCoroutine(UseAbility(G.State.PlayerState, G.State.PlayerState.Abilities[id]));
        }
        private IEnumerator BeginEnemyFightCycle()
        {
            G.State.FightTurnTeam = TurnTeam.NoOne;
            G.HUD.DisableFightHud();

            var inters = Interactor.FindAll<IOnTurnEnd>();
            foreach (var inter in inters)
                yield return inter.OnTurnEnd(TurnTeam.Player);

            if (CheckFightEnd(out var endState))
            {
                yield return EndFightLoop(endState);
                yield break;
            }

            G.State.FightTurnTeam = TurnTeam.Enemy;

            var inters2 = Interactor.FindAll<IOnTurnBegin>();
            foreach (var inter in inters2)
                yield return inter.OnTurnBegin(TurnTeam.Enemy);

            yield return ControlFightEnemy();

            var inters3 = Interactor.FindAll<IOnTurnEnd>();
            foreach (var inter in inters3)
                yield return inter.OnTurnEnd(TurnTeam.Enemy);

            if (CheckFightEnd(out endState))
            {
                yield return EndFightLoop(endState);
                yield break;
            }

            yield return BeginFightCycle();
        }
        private IEnumerator ControlFightEnemy()
        {
            if(G.State.EnemyState.IsDead)
                yield break;

            var abilities = G.State.EnemyState.Abilities.ToList();
            var checkedAbilities = new List<AbilityState>();

            while (true)
            {
                abilities.Shuffle();
                var targetAbility = abilities[0];

                if (checkedAbilities.Contains(targetAbility))
                    continue;

                var filters = Interactor.FindAll<ICanUseAbilityFilter>();
                bool canUse = true;
                foreach (var filter in filters)
                {
                    if(filter.CanUse(G.State.EnemyState, targetAbility) == false)
                    {
                        checkedAbilities.Add(targetAbility);
                        canUse = false;
                        break;
                    }
                }

                if (canUse)
                {
                    yield return UseAbility(G.State.EnemyState, targetAbility);
                    yield break;
                }
                else if (checkedAbilities.Count == abilities.Count)
                    yield break;
            }
        }
        private bool CheckFightEnd(out int winStateId)
        {
            if (G.State.EnemyState.IsDead)
            {
                winStateId = 1;//win
                return true;
            }
            if (G.State.PlayerState.IsDead)
            {
                winStateId = 0;//lose
                return true;
            }

            winStateId = -1;
            return false;
        }
        private IEnumerator EndFightLoop(int winStateId)
        {
            if (winStateId == 1)//win
            {
                yield return EnterNextHoleSelectionScene();
            }
            else if (winStateId == 0)//lose
            {
                G.State.FightTurnTeam = TurnTeam.NoOne;
                G.State.Fighting = false;
                G.HUD.DisableHud();
                G.UI.ShowLose();
            }
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