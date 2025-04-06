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
        public static CutScene CutScene;
    }
    [Serializable]
    public class GameState
    {
        public int Level;
        public int Grade;

        public bool SelectingHole;
        public HoleState LeftHoleInfo;
        public HoleState RightHoleInfo;

        public List<EventState> ActiveEvents = new();

        public bool Fighting;
        public TurnTeam FightTurnTeam = TurnTeam.NoOne;
        public EntityState EnemyState;

        public EntityState PlayerState;
    }
    public class Main : MonoBehaviour
    {
        public event Action OnChangeScene;

        [Header("Main")]
        public GameState State;
        public int GradeCost = 5;

        public Interactor Interactor;

        [Header("Hole")]
        public GameObject HoleSelectionSceneParent;
        public HoleView LeftHole;
        public HoleView RightHole;
        public EntityView PlayerHoleView;
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
            StartCoroutine(MostFirstCutScene());

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
        public EntityState GetEntity(TurnTeam team)
        {
            if (team == TurnTeam.Player) return G.State.PlayerState;
            else if (team == TurnTeam.Enemy) return G.State.EnemyState;
            else
            {
                Debug.Log("no entity for NoOne team");
                return null;
            }
        }
        public IEnumerator ApplySelfHeal(EntityState target, float heal)
        {
            if (target.IsDead)
            {
                Debug.Log("Cant heal dead target", target.View);
                yield break;
            }

            target.Health = Mathf.Min(target.MaxHealth, target.Health + heal);
            target.View.UpdateHealth();
        }
        public IEnumerator ApplySelfStun(EntityState target, int stunCycles)
        {
            target.StunCycles = stunCycles;
            target.CyclesAfterStun = 0;
            Debug.Log("todo: stun effect");
            yield break;
        }
        public IEnumerator ApplyStun(EntityState stunner, EntityState target, int stunCycles)
        {
            target.StunCycles = stunCycles;
            target.CyclesAfterStun = 0;
            Debug.Log("todo: stun effect");
            yield break;
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
        public IEnumerator ApplyDamage(EntityState attacker, EntityState target, float damage, bool throughArmor = false)
        {
            var damageProperty = new PropertyLink<float>(damage);

            var inters = Interactor.FindAll<IOnApplyDamage>();
            foreach (var inter in inters)
                yield return inter.OnApplyDamage(attacker, target, damageProperty);

            if (target.Armor > 0 && !throughArmor)
            {
                if(target.Armor >= damageProperty.Value)
                {
                    target.Armor -= damageProperty.Value;
                    target.View.UpdateArmor();
                }
                else
                {
                    var remainingDamage = damageProperty.Value - target.Armor;
                    target.Armor = 0;
                    target.Health -= remainingDamage;
                    target.View.UpdateArmor();
                    target.View.UpdateHealth();
                }
            }
            else
            {
                target.Health -= damageProperty.Value;
                target.View.UpdateHealth();
            }

            if (target.Health <= 0)
            {
                var inters2 = Interactor.FindAll<IOnBeforeEntityDie>();
                foreach (var inter in inters2)
                    yield return inter.OnBeforeEntityDie(target);

                if (target.Health <= 0)
                {
                    target.View.DrawDie();
                }
            }

            yield break;
        }

        private IEnumerator MostFirstCutScene()
        {
            G.CutScene.SetBG(1);

            yield return G.CutScene.SmartWait(3);

            yield return G.CutScene.Say("Darkness.");

            yield return G.CutScene.SmartWait(3);

            yield return G.CutScene.Say("My whole body aches...");

            yield return G.CutScene.SmartWait(2);

            yield return G.CutScene.ContinueSay(" How did I get here?");

            yield return G.CutScene.SmartWait(3);

            yield return G.CutScene.Say("These staircases");

            yield return G.CutScene.SmartWait(1.5f);

            yield return G.CutScene.ContinueSay("—no good lies below.");

            yield return G.CutScene.SmartWait(3);

            yield return G.CutScene.Say("Smells like sorcery.");

            yield return G.CutScene.SmartWait(4);

            yield return G.CutScene.Say("Must choose");

            yield return G.CutScene.SmartWait(2);

            yield return G.CutScene.ContinueSay(".");

            yield return G.CutScene.SmartWait(0.5f);

            yield return G.CutScene.ContinueSay(".");

            yield return G.CutScene.SmartWait(0.5f);

            yield return G.CutScene.ContinueSay(".");

            yield return G.CutScene.SmartWait(2);

            yield return G.CutScene.Unsay();

            yield return G.CutScene.SmartWait(2);

            yield return EnterHoleSelectionScene();

            G.CutScene.SetBG(0);
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
            var oldGrade = G.State.Grade;
            G.State.Grade = Mathf.Min(G.State.Level / GradeCost, generationInfosMap.Count - 1);

            if(oldGrade != G.State.Grade)
            {

            }

            yield return EnterHoleSelectionScene();
        }
        private IEnumerator EnterHoleSelectionScene()
        {
            yield return UnloadScene();

            PlayerHoleView.SetState(G.State.PlayerState);

            for (int i = 0; i < 2; i++)
            {
                var holeState = new HoleState();

                var holeInfo = GetCurrentGenInfo().HoleInfo.Get<TagHoleGenerationInfo>();
                var playerActiveAbilityIds = G.State.PlayerState.Abilities.Where(x => x != null).Select(x => x.Model.id).ToList();
                var abilityIds = holeInfo.AbilityLinks.Select(x => x.Id).Where(x => !playerActiveAbilityIds.Contains(x)).ToList();

                var r = UnityEngine.Random.Range(0, 101);
                var r2 = UnityEngine.Random.Range(0, 101);

                bool shouldSetAbility = abilityIds.Count > 0 && r != 0 && r <= holeInfo.HoleAbilityChance;
                bool shouldSetEvent = r2 != 0 && r2 <= holeInfo.HoleEventChance;

                if (!holeInfo.CanHaveBothThings && shouldSetAbility && shouldSetEvent)
                {
                    shouldSetAbility = r > r2;
                    shouldSetEvent = r2 > r;
                }

                if (shouldSetAbility)
                {
                    abilityIds.Shuffle();
                    var randomAbilityId = abilityIds[0];
                    holeState.Ability = CMS.Get<CMSEntity>(randomAbilityId);
                }

                if (shouldSetEvent)
                {
                    var eventIds = holeInfo.EventLinks.Select(x => x.Id).ToList();
                    eventIds.Shuffle();
                    var randomEventId = eventIds[0];
                    holeState.Event = CMS.Get<CMSEntity>(randomEventId);
                }

                if (i == 0)
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
            OnChangeScene?.Invoke();
        }
        private void OnSelectHole(HoleState state)
        {
            if (!G.State.SelectingHole) return;

            StartCoroutine(EnterHoleProcess(state));
        }
        private IEnumerator EnterHoleProcess(HoleState holeState)
        {
            G.State.SelectingHole = false;

            if (holeState.HasEvent)
            {
                var eventState = new EventState();
                eventState.SetModel(holeState.Event);

                G.State.ActiveEvents.Add(eventState);

                var inters = Interactor.FindAll<IOnApplyHoleEvent>();
                foreach (var inter in inters)
                    yield return inter.OnApplyHoleEvent(holeState, eventState);
            }

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

            yield return G.CutScene.DoFade(0, 1);
            yield return new WaitForSeconds(2);

            G.HUD.AbilitySelectionPanel.HideMenu();

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

            yield return G.CutScene.DoFade(1, 0);
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
            OnChangeScene?.Invoke();

            var inters = Interactor.FindAll<IOnFightBegin>();
            foreach (var inter in inters)
                yield return inter.OnFightBegin();

            yield return BeginFightCycle();
        }
        private IEnumerator BeginFightCycle()
        {
            G.State.FightTurnTeam = TurnTeam.NoOne;

            var inters = Interactor.FindAll<IOnTurnBegin>();
            foreach (var inter in inters)
                yield return inter.OnTurnBegin(TurnTeam.Player);

            if (CheckFightEnd(out var endState))
            {
                yield return EndFightLoop(endState);
                yield break;
            }

            if (G.State.PlayerState.IsStunned)
            {
                yield return BeginEnemyFightCycle();
            }
            else
            {
                G.State.FightTurnTeam = TurnTeam.Player;
                G.HUD.EnableFightHud();
            }
        }
        private void OnPressEndTurn()
        {
            if (G.State.FightTurnTeam != TurnTeam.Player) return;
            if (usingAbility) return;

            StartCoroutine(BeginEnemyFightCycle());
        }
        private void OnSelectFightAbility(int id)
        {
            Debug.Log($"Pressed ability {id}"); 
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

            if (CheckFightEnd(out var endState))
            {
                yield return EndFightLoop(endState);
                yield break;
            }

            var inters = Interactor.FindAll<IOnTurnEnd>();
            foreach (var inter in inters)
                yield return inter.OnTurnEnd(TurnTeam.Player);

            if (CheckFightEnd(out endState))
            {
                yield return EndFightLoop(endState);
                yield break;
            }

            G.State.FightTurnTeam = TurnTeam.Enemy;  

            var inters2 = Interactor.FindAll<IOnTurnBegin>();
            foreach (var inter in inters2)
                yield return inter.OnTurnBegin(TurnTeam.Enemy);

            if (CheckFightEnd(out endState))
            {
                yield return EndFightLoop(endState);
                yield break;
            }

            yield return ControlFightEnemy();

            if (CheckFightEnd(out endState))
            {
                yield return EndFightLoop(endState);
                yield break;
            }

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
            if (G.State.EnemyState.IsStunned)
                yield break;
            if (G.State.EnemyState.IsDead)
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
            var inters = Interactor.FindAll<IOnFightEnd>();
            foreach (var inter in inters)
                yield return inter.OnFightEnd();

            yield return G.CutScene.DoFade(0, 1);
            yield return new WaitForSeconds(2);

            if (winStateId == 1)//win
            {
                yield return EnterNextHoleSelectionScene();
                yield return G.CutScene.DoFade(1, 0);
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