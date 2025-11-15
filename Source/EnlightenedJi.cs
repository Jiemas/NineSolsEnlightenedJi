using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using NineSolsAPI.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Reflection;

namespace EnlightenedJi;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class EnlightenedJi : BaseUnityPlugin {
    // https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html

    private Harmony harmony = null!;

    private ConfigEntry<float> JiAnimatorSpeed = null!;
    private ConfigEntry<float> JiHPScale = null!;

    private string jiBossPath = "";
    private string jiAttackStatesPath = "";
    private string jiAttackSequences1Path = "";
    private string jiAttackSequences2Path = "";
    private string jiAttackGroupsPath = "";

    private static string[] Attacks = [
        "",
        "[1]Divination Free Zone",
        "[2][Short]Flying Projectiles",
        "[3][Finisher]BlackHoleAttack",
        "[4][Altar]Set Laser Altar Environment",
        "[5][Short]SuckSword 往內",
        "[6][Finisher]Teleport3Sword Smash 下砸",
        "[7][Finisher]SwordBlizzard",
        "",
        "[9][Short]GroundSword",
        "[10][Altar]SmallBlackHole",
        "[11][Short]ShorFlyingSword",
        "[12][Short]QuickHorizontalDoubleSword",
        "[13][Finisher]QuickTeleportSword 危戳",
        "[14][Altar]Laser Altar Circle",
        "[15][Altar]Health Altar",
        "[16]Divination JumpKicked"
    ];

    private static BossGeneralState[] BossGeneralStates = new BossGeneralState[17];

    private static Weight<MonsterState>[] Weights = new Weight<MonsterState>[17];

    private static string[] Sequences1 = { "", "_WithAltar", "_WithSmallBlackHole", "_QuickToBlizzard" };

    private static string[] Sequences2 = { "", "_WithAltar", "_WithSmallBlackHole", "_QuickToBlizzard", 
                                            "_QuickToBlackHole", "_Phase2_OpeningBlackHole" };
    // private static string[] Groups = {
    //     "SmallBlackHole(Attack10)", 
    //     "LongerAttack(Attack2/Attack5/Attack9)", 
    //     "Blizzard(Attack7)", 
    //     "QuickAttack(Attack11/Attack12)",
    //     "Altar_OnlyEasier",
    //     "Finisher_Easier(Attack13)",
    //     "BigBlackHole(Attack3)"
    // };

    // private static Dictionary<string, MonsterStateGroup> MonsterStateGroups = new Dictionary<string, MonsterStateGroup>();

    private bool HPUpdated = false;
    public static bool phase2 = false;

    string temp = "";

    int updateCounter = 0;
    private int randomNum = 0;
    int phasesFromBlackHole = 0;

    System.Random random = new System.Random();

    // #region Attacks BossGeneralState
    BossGeneralState HurtBossGeneralState = null!;
    BossGeneralState BigHurtBossGeneralState = null!;
    // #endregion

    PostureBreakState JiStunState = null!;

    #region Attack Sequences MonsterStateGroupSequence
    MonsterStateGroupSequence AttackSequence1_FirstAttackGroupSequence = null!;
    MonsterStateGroupSequence AttackSequence1_GroupSequence = null!;
    MonsterStateGroupSequence AttackSequence1_AltarGroupSequence = null!;
    MonsterStateGroupSequence AttackSequence1_SmallBlackHoleGroupSequence = null!;
    MonsterStateGroupSequence AttackSequence1_QuickToBlizzardGroupSequence = null!;
    MonsterStateGroupSequence SpecialHealthSequence = null!;
    MonsterStateGroupSequence AttackSequence2_Opening = null!;
    MonsterStateGroupSequence AttackSequence2_QuickToBlizzardGroupSequence = null!;
    MonsterStateGroupSequence AttackSequence2 = null!;
    MonsterStateGroupSequence AttackSequence2_AltarGroupSequence = null!;
    MonsterStateGroupSequence AttackSequence2_SmallBlackHoleGroupSequence = null!;
    MonsterStateGroupSequence AttackSequence2_QuickToBlackHoleGroupSequence = null!;
    #endregion

    #region Attack Groups MonsterStateGroup
    MonsterStateGroup SneakAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup BackAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup LongerOrBlizardAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup SwordOrLaserAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup SwordOrAltarAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup LaserAltarHardAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup DoubleTroubleAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup SneakAttack2StateGroup = new MonsterStateGroup();
    MonsterStateGroup HardAltarOrEasyFinisherAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup HardAltarOrHardFinisherAttackStateGroup = new MonsterStateGroup();
    MonsterStateGroup SmallBlackHoleMonsterStateGroup = null!;
    MonsterStateGroup LongerAttackStateGroup = null!;
    MonsterStateGroup BlizzardAttackStateGroup = null!;
    MonsterStateGroup QuickAttackStateGroup = null!;
    MonsterStateGroup LaserAltarEasyAttackStateGroup = null!;
    MonsterStateGroup FinisherEasierAttackStateGroup = null!;
    MonsterStateGroup BigBlackHoleAttackStateGroup = null!;
    #endregion

    StealthEngaging Engaging = null!;
    BossPhaseChangeState PhaseChangeState = null!;

    AttackSequenceModule attackSequenceModule = null!;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(EnlightenedJi).Assembly);

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        JiAnimatorSpeed = Config.Bind("General", "JiSpeed", 1.2f, "The speed at which Ji's attacks occur");
        JiHPScale = Config.Bind("General", "JiHPScale", 6000f, "The amount of Ji's HP in Phase 1 (Phase 2 HP is double this value)");
    }

    public void Update() {
        if (SceneManager.GetActiveScene().name == "A10_S5_Boss_Jee") {
            var JiMonster = MonsterManager.Instance.ClosetMonster;
            GetAttackGameObjects();
            AlterAttacks();
            JiHPChange();
            JiSpeedChange();

            if (JiMonster && temp != JiMonster.currentMonsterState.ToString()) {
                temp = JiMonster.currentMonsterState.ToString();
                randomNum = random.Next();
                if (JiMonster.currentMonsterState == PhaseChangeState) {
                    phase2 = true;
                } 
                phasesFromBlackHole = JiMonster.currentMonsterState == BossGeneralStates[10] ? 0 : (phasesFromBlackHole + 1);
                ToastManager.Toast(phase2);
            }
        } 
    }

    private List<BossGeneralState> GetIndices(int[] indices)
    {
        List<BossGeneralState> pickedStates = new List<BossGeneralState>();
        foreach (int i in indices)
        {
            pickedStates.Add(BossGeneralStates[i]);
        }
        return pickedStates;
    }

    private void JiSpeedChange() {
        var JiMonster = MonsterManager.Instance.ClosetMonster;
        if (!JiMonster) return;

        if (GetIndices([10, 16]).Contains(JiMonster.currentMonsterState) || 
        JiMonster.currentMonsterState == Engaging || JiMonster.currentMonsterState == HurtBossGeneralState ||
        JiMonster.currentMonsterState == BigHurtBossGeneralState || JiMonster.currentMonsterState == JiStunState) 
        {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 3;
        } else if (JiMonster.currentMonsterState == BossGeneralStates[15]) 
        {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 1;
        } else if (!phase2 && JiSpeedChangePhase1()) {
            return;
        } else if (phase2 && JiSpeedChangePhase2()) {
            return;
        } else if (JiMonster.LastClipName == "PostureBreak") {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 0.2f;
        } else {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value;
        }

    }

    private bool JiSpeedChangePhase1() {
        var JiMonster = MonsterManager.Instance.ClosetMonster;

        // Sneak Attack Speed Up
        if ((JiMonster.LastClipName == "Attack13" || JiMonster.LastClipName == "PostureBreak") && 
            GetIndices([11, 12, 14]).Contains(JiMonster.currentMonsterState))
        {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 2;
        } else if ((JiMonster.currentMonsterState == BossGeneralStates[13] && 
            (JiMonster.LastClipName == "Attack13" || JiMonster.LastClipName == "PostureBreak")) || 
            (randomNum % 2 == 0 && GetIndices([11, 5]).Contains(JiMonster.currentMonsterState)))
        {
            ToastManager.Toast(JiMonster.currentMonsterState);
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 0.5f;
        } else {
            return false;
        }
        return true;
    }

    private bool JiSpeedChangePhase2() {
        var JiMonster = MonsterManager.Instance.ClosetMonster;
        if (JiMonster.currentMonsterState == BossGeneralStates[3]) {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 2;

        // Sneak Attack Speed Up
        } else if ((JiMonster.LastClipName == "Attack13" || JiMonster.LastClipName == "PostureBreak" || 
            JiMonster.LastClipName == "Attack6") && !(GetIndices([6, 13]).Contains(JiMonster.currentMonsterState)))
        {
            if (JiMonster.currentMonsterState == BossGeneralStates[14]) {
                JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 2f;
            } else {
               JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 0.65f;
            }

        // Hard Altar Attack Speed Up
        } else if (JiMonster.currentMonsterState == BossGeneralStates[4] && phasesFromBlackHole > 6) 
        {
            updateCounter++;
            if (updateCounter < 500) {
                JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 1.65f;
            } else {
                updateCounter = 0;
            }
        
        // Blizzard Attack Speed up
        } else if (JiMonster.currentMonsterState == BossGeneralStates[7] &&
            GetCurrentSequence() != AttackSequence2_QuickToBlizzardGroupSequence) 
        {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 0.65f;

        // Opening Sequence Sword Attack Speed Up
        } else if (GetCurrentSequence() == AttackSequence2_Opening &&
            GetIndices([11, 12, 2, 9]).Contains(JiMonster.currentMonsterState))
        {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 0.25f;
        
        // Red/Green Attack Speed Up
        } else if (randomNum % 3 == 0 && GetIndices([6, 13]).Contains(JiMonster.currentMonsterState))
        {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 0.35f;
        } else if (randomNum % 2 == 0) {
            JiMonster.monsterCore.AnimationSpeed = JiAnimatorSpeed.Value + 0.3f;
        } else {
            return false;
        }
        return true;
    }

    private void JiHPChange() {
        var baseHealthRef = AccessTools.FieldRefAccess<MonsterStat, float>("BaseHealthValue");
        var JiMonster = MonsterManager.Instance.ClosetMonster;
        if (!HPUpdated && JiMonster) {
            baseHealthRef(JiMonster.monsterStat) = JiHPScale.Value / 1.35f;
            JiMonster.postureSystem.CurrentHealthValue = JiHPScale.Value;
            HPUpdated = true;
            ToastManager.Toast($"Set Ji Phase 1 HP to {JiMonster.postureSystem.CurrentHealthValue}");
        }
        JiMonster.monsterStat.Phase2HealthRatio = 2;
    }

    private MonsterStateGroupSequence GetCurrentSequence(){
        Type type = attackSequenceModule.GetType();
        FieldInfo fieldInfo = type.GetField("sequence", BindingFlags.Instance | BindingFlags.NonPublic);
        MonsterStateGroupSequence sequenceValue = (MonsterStateGroupSequence)fieldInfo.GetValue(attackSequenceModule);
        return sequenceValue;
    }

    private MonsterStateGroupSequence getGroupSequence1(string name){
        return GameObject.Find($"{jiAttackSequences1Path}{name}").GetComponent<MonsterStateGroupSequence>();
    }

    private MonsterStateGroupSequence getGroupSequence2(string name){
        return GameObject.Find($"{jiAttackSequences2Path}{name}").GetComponent<MonsterStateGroupSequence>();
    }

    private MonsterStateGroup getGroup(string name){
        return GameObject.Find($"{jiAttackGroupsPath}{name}").GetComponent<MonsterStateGroup>();
    }

    private BossGeneralState getBossGeneralState(string name){
        return GameObject.Find($"{jiAttackStatesPath}{name}").GetComponent<BossGeneralState>();
    }

    public void GetAttackGameObjects(){

        jiBossPath = "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/";

        jiAttackStatesPath = jiBossPath + "States/Attacks/";
        jiAttackSequences1Path = jiBossPath + "MonsterCore/AttackSequenceModule/MonsterStateSequence_Phase1/";
        jiAttackSequences2Path = jiBossPath + "MonsterCore/AttackSequenceModule/MonsterStateSequence_Phase2/";
        jiAttackGroupsPath = jiBossPath + "MonsterCore/AttackSequenceModule/MonsterStateGroupDefinition/";

        for (int i = 1; i < Attacks.Length; i++)
        {
            BossGeneralStates[i] = getBossGeneralState(Attacks[i]);
            Weights[i] = CreateWeight(BossGeneralStates[i]);
        }

        // foreach (string group in Groups)
        // {
        //     MonsterStateGroups.Add(string, )
        // }

        AttackSequence1_FirstAttackGroupSequence = getGroupSequence1("MonsterStateGroupSequence1_FirstAttack_WithoutDivination");
        AttackSequence1_GroupSequence = getGroupSequence1("MonsterStateGroupSequence1");
        AttackSequence1_AltarGroupSequence = getGroupSequence1("MonsterStateGroupSequence1_WithAltar");
        AttackSequence1_SmallBlackHoleGroupSequence = getGroupSequence1("MonsterStateGroupSequence1_WithSmallBlackHole");
        AttackSequence1_QuickToBlizzardGroupSequence = getGroupSequence1("MonsterStateGroupSequence1_QuickToBlizzard");
        SpecialHealthSequence = GameObject.Find($"{jiBossPath}MonsterCore/AttackSequenceModule/SpecialHealthSequence(Jee_Divination_Logic)").GetComponent<MonsterStateGroupSequence>();
        
        AttackSequence2_Opening = getGroupSequence2("MonsterStateGroupSequence1_Phase2_OpeningBlackHole");
        AttackSequence2_QuickToBlizzardGroupSequence = getGroupSequence2("MonsterStateGroupSequence1_QuickToBlizzard");
        AttackSequence2 = getGroupSequence2("MonsterStateGroupSequence1");
        AttackSequence2_AltarGroupSequence = getGroupSequence2("MonsterStateGroupSequence1_WithAltar");
        AttackSequence2_SmallBlackHoleGroupSequence = getGroupSequence2("MonsterStateGroupSequence1_WithSmallBlackHole");
        AttackSequence2_QuickToBlackHoleGroupSequence = getGroupSequence2("MonsterStateGroupSequence1_QuickToBlackHole");
    

        SmallBlackHoleMonsterStateGroup = getGroup("MonsterStateGroup_SmallBlackHole(Attack10)");
        LongerAttackStateGroup = AttackSequence1_GroupSequence.AttackSequence[3];
        BlizzardAttackStateGroup = AttackSequence1_QuickToBlizzardGroupSequence.AttackSequence[1];
        QuickAttackStateGroup = AttackSequence1_FirstAttackGroupSequence.AttackSequence[0];
        LaserAltarEasyAttackStateGroup = AttackSequence1_AltarGroupSequence.AttackSequence[1];
        FinisherEasierAttackStateGroup = AttackSequence1_AltarGroupSequence.AttackSequence[4];
        BigBlackHoleAttackStateGroup = AttackSequence2_Opening.AttackSequence[0];

        JiStunState = GameObject.Find($"{jiBossPath}States/PostureBreak/").GetComponent<PostureBreakState>();
        PhaseChangeState = GameObject.Find($"{jiBossPath}States/[BossAngry] BossAngry/").GetComponent<BossPhaseChangeState>();
        
        HurtBossGeneralState = GameObject.Find($"{jiBossPath}States/HurtState/").GetComponent<BossGeneralState>();
        BigHurtBossGeneralState = GameObject.Find($"{jiBossPath}States/Hurt_BigState").GetComponent<BossGeneralState>();
        
        Engaging = GameObject.Find($"{jiBossPath}States/1_Engaging").GetComponent<StealthEngaging>();

        attackSequenceModule = GameObject.Find($"{jiBossPath}MonsterCore/AttackSequenceModule/").GetComponent<AttackSequenceModule>();

    }

    private void AddSmallBlackHoleGroupToSequence(MonsterStateGroupSequence sequence){
        sequence.AttackSequence.Add(SmallBlackHoleMonsterStateGroup);
    }

    private void AddSneakAttackGroupToSequence(MonsterStateGroupSequence sequence){
        sequence.AttackSequence.Add(SneakAttackStateGroup);
    }

    private void InsertBackAttackGroupToSequence(MonsterStateGroupSequence sequence, int index){
        sequence.AttackSequence.Insert(index, BackAttackStateGroup);
    }

    private void OverwriteAttackGroupInSequence(MonsterStateGroupSequence sequence, int index, MonsterStateGroup newGroup){
        sequence.AttackSequence[index] = newGroup;
    }

    private void InsertAttackGroupToSequence(MonsterStateGroupSequence sequence, int index, MonsterStateGroup newGroup){
        sequence.AttackSequence.Insert(index, newGroup);
    }

    private void AddAttackGroupToSequence(MonsterStateGroupSequence sequence, MonsterStateGroup newGroup){
        sequence.AttackSequence.Add(newGroup);
    }

    private Weight<MonsterState> CreateWeight(MonsterState state){
        return new Weight<MonsterState>{
            weight = 1,
            option = state
        };
    }

    private MonsterStateGroup CreateMonsterStateGroup(int[] AttacksList, string objectName)
    {
        List<Weight<MonsterState>> newStateWeightList = new List<Weight<MonsterState>>();
        List<MonsterState> newQueue = new List<MonsterState>();
        GameObject GO = new GameObject(objectName);
        MonsterStateGroup newAttackGroup = new MonsterStateGroup();

        foreach (int attackIndex in AttacksList)
        {
            newStateWeightList.Add(Weights[attackIndex]);
            newQueue.Add(BossGeneralStates[attackIndex]);
        }

        MonsterStateWeightSetting newWeightSetting = new MonsterStateWeightSetting {
            stateWeightList = newStateWeightList,
            queue = newQueue
        };
        newAttackGroup = GO.AddComponent<MonsterStateGroup>();
        newAttackGroup.setting = newWeightSetting;
        return newAttackGroup;
    }

    public void AlterAttacks(){
        if (AttackSequence1_FirstAttackGroupSequence.AttackSequence.Contains(SneakAttackStateGroup)) {
            return;
        }
        phase2 = false;

        SneakAttackStateGroup = CreateMonsterStateGroup([11, 12, 13, 14], "MonsterStateGroup_SneakAttack(Attack 11/12/13/14)");
        BackAttackStateGroup = CreateMonsterStateGroup([5, 9], "MonsterStateGroup_BackAttack(Attack 5/9)");
        LongerOrBlizardAttackStateGroup = CreateMonsterStateGroup([5, 9, 7], "MonsterStateGroup_LongerOrBlizardAttack(Attack 5/9/7)");
        SwordOrLaserAttackStateGroup = CreateMonsterStateGroup([11, 12, 14], "MonsterStateGroup_SwordOrLaserAttack(Attack 11/12/14)");
        SwordOrAltarAttackStateGroup = CreateMonsterStateGroup([10, 12, 14], "MonsterStateGroup_SwordOrAltarAttack(Attack10/12/14)");
        LaserAltarHardAttackStateGroup = CreateMonsterStateGroup([4], "MonsterStateGroup_LaserAltarHardAttack(Attack4)");
        DoubleTroubleAttackStateGroup = CreateMonsterStateGroup([2, 5, 9, 12], "MonsterStateGroup_DoubleTroubleAttack(Attack2/9/12/5)");
        SneakAttack2StateGroup = CreateMonsterStateGroup([2, 4, 5, 9, 11, 12, 14], "MonsterStateGroup_SneakAttack2(Attack2/4/5/9/11/12/14)");
        HardAltarOrEasyFinisherAttackStateGroup = CreateMonsterStateGroup([4, 13], "MonsterStateGroup_HardAltarOrEasyFinisher(Attack4/13)");
        HardAltarOrHardFinisherAttackStateGroup = CreateMonsterStateGroup([4, 6], "MonsterStateGroup_HardAltarOrHardFinisher(Attack4/6)");
   

        AddSneakAttackGroupToSequence(AttackSequence1_FirstAttackGroupSequence);
        AddSneakAttackGroupToSequence(AttackSequence1_GroupSequence);
        AddSneakAttackGroupToSequence(AttackSequence1_AltarGroupSequence);
        AddSneakAttackGroupToSequence(AttackSequence1_SmallBlackHoleGroupSequence);
        AddSneakAttackGroupToSequence(AttackSequence1_QuickToBlizzardGroupSequence);
        AddSneakAttackGroupToSequence(SpecialHealthSequence);

        OverwriteAttackGroupInSequence(AttackSequence1_GroupSequence, 2, SneakAttackStateGroup);
        OverwriteAttackGroupInSequence(AttackSequence1_FirstAttackGroupSequence, 1, SneakAttackStateGroup);



        InsertBackAttackGroupToSequence(AttackSequence1_GroupSequence, 4);
        InsertBackAttackGroupToSequence(AttackSequence1_AltarGroupSequence, 4);



        OverwriteAttackGroupInSequence(AttackSequence1_AltarGroupSequence, 2, LongerOrBlizardAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence1_FirstAttackGroupSequence, 3, LongerOrBlizardAttackStateGroup);
        InsertAttackGroupToSequence(SpecialHealthSequence, 2, LongerOrBlizardAttackStateGroup);

        OverwriteAttackGroupInSequence(AttackSequence1_SmallBlackHoleGroupSequence, 2, SwordOrLaserAttackStateGroup);
        OverwriteAttackGroupInSequence(AttackSequence1_SmallBlackHoleGroupSequence, 3, LongerAttackStateGroup);    
        InsertAttackGroupToSequence(AttackSequence1_QuickToBlizzardGroupSequence, 1, SwordOrAltarAttackStateGroup);


        InsertAttackGroupToSequence(AttackSequence2_Opening, 1, LaserAltarHardAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_Opening, 2, BlizzardAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_Opening, 3, QuickAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_Opening, 4, SmallBlackHoleMonsterStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_Opening, 5, LaserAltarEasyAttackStateGroup);



        InsertAttackGroupToSequence(AttackSequence2_Opening, 6, DoubleTroubleAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_Opening, 7, DoubleTroubleAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_Opening, 8, DoubleTroubleAttackStateGroup);


        AddAttackGroupToSequence(AttackSequence2_Opening, SneakAttack2StateGroup);

        InsertAttackGroupToSequence(AttackSequence2_Opening, 9, HardAltarOrEasyFinisherAttackStateGroup);


        AddAttackGroupToSequence(AttackSequence2, SneakAttack2StateGroup);
        AddAttackGroupToSequence(AttackSequence2_AltarGroupSequence, SneakAttack2StateGroup);
        AddAttackGroupToSequence(AttackSequence2_SmallBlackHoleGroupSequence, SneakAttack2StateGroup);
        AddAttackGroupToSequence(AttackSequence2_QuickToBlizzardGroupSequence, SneakAttack2StateGroup);
        AddAttackGroupToSequence(AttackSequence2_QuickToBlackHoleGroupSequence, SneakAttack2StateGroup);

        InsertAttackGroupToSequence(AttackSequence2, 2, DoubleTroubleAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2, 4, DoubleTroubleAttackStateGroup);
        OverwriteAttackGroupInSequence(AttackSequence2, 5, HardAltarOrHardFinisherAttackStateGroup);

        OverwriteAttackGroupInSequence(AttackSequence2_AltarGroupSequence, 1, LaserAltarHardAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_AltarGroupSequence, 2, BlizzardAttackStateGroup);
        OverwriteAttackGroupInSequence(AttackSequence2_AltarGroupSequence, 5, LaserAltarHardAttackStateGroup);
        
        OverwriteAttackGroupInSequence(AttackSequence2_SmallBlackHoleGroupSequence, 2, LaserAltarEasyAttackStateGroup);
        OverwriteAttackGroupInSequence(AttackSequence2_SmallBlackHoleGroupSequence, 5, HardAltarOrHardFinisherAttackStateGroup);

        InsertAttackGroupToSequence(AttackSequence2_QuickToBlizzardGroupSequence, 3, BlizzardAttackStateGroup);
        OverwriteAttackGroupInSequence(AttackSequence2_QuickToBlizzardGroupSequence, 5, HardAltarOrHardFinisherAttackStateGroup);

        InsertAttackGroupToSequence(AttackSequence2_QuickToBlackHoleGroupSequence, 2, LaserAltarHardAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_QuickToBlackHoleGroupSequence, 3, BigBlackHoleAttackStateGroup);
        InsertAttackGroupToSequence(AttackSequence2_QuickToBlackHoleGroupSequence, 4, LongerAttackStateGroup);
        OverwriteAttackGroupInSequence(AttackSequence2_QuickToBlackHoleGroupSequence, 5, HardAltarOrHardFinisherAttackStateGroup);
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading
        HPUpdated = false;
        phase2 = false;
        harmony.UnpatchSelf();
    }
}