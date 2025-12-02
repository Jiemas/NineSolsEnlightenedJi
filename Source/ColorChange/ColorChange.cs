using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using NineSolsAPI;
using NineSolsAPI.Utils;

namespace EnlightenedJi;

//  C:/Users/Jiemas/AppData/LocalLow/RedCandleGames/NineSols
// string spritePath = "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/";

// light in staff tip during attack 6 and light in orb during attack 14 charging
// A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Weapon/JeeStaffTip/Effect_BEAM/STPBALL/Light/

// Might be the red laser circles
// CircularDamage(Clone)/Animator/Effect_BEAM/P_ScretTreePowerCIRCLEGlow/

// Red stars are called Tai danger

public class ColorChange {
    public static int lutSize = 16;
    public static Material material = null!;
    private static int stayRange = 1;

    // public static (float x, float y, float z)[] originalColors = 
    // [
    //   (5f, 5f, 5f),  // Black
    //   (247f, 248f, 241f), // Fur White 
    //   (186f, 240f, 227f),   // Eye Baby Blue
    //   (79f, 193f, 129f), // Green Claws/Headband
    //   (104f, 24f, 23f),  // Cape Red 
    //   (228f, 190f, 106f), // Robe Yellow
    //   (212f, 203f, 167f), // Tan Robe Highlight
    // //   (223f, 86f, 96f) // Crimson/Tai Danger Red
    // ];

    // public static ((float x, float y, float z) src, (float x, float y, float z) dst)[] colorPairs = new ((float x, float y, float z) src, (float x, float y, float z) dst)[originalColors.Length];

    // private static Vector3[] srcColors = new Vector3[originalColors.Length * (int) Math.Pow(stayRange * 2 + 1, 3)];

    // private static Vector3[] dstColors = new Vector3[originalColors.Length * (int) Math.Pow(stayRange * 2 + 1, 3)];

    private static string spritePath = "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/";
    private static string[] jiSpritePaths = 
    [
        // "A10S5/Room/Boss And Environment Binder/1_DropPickable 亮亮 FSM Variant/ItemProvider/DropPickable FSM Prototype/FSM Animator/VIEW/DummyJeeAnimator/View/Jee/JeeSprite",
        // "A10S5/Room/Boss And Environment Binder/1_DropPickable 亮亮 FSM Variant/ItemProvider/DropPickable FSM Prototype/FSM Animator/AfterShowView/DummyJeeAnimator/View/Jee/JeeSprite",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Jee/JeeSprite",
        // "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/DummyJeeAnimator/View/Jee/JeeSprite",
        // "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/[CutScene]FirstTimeContact/DummyJeeAnimator/View/Jee/JeeSprite",
        // "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/[CutScene]SecondTimeContact/DummyJeeAnimator/View/Jee/JeeSprite",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/[CutScene]BossAngry_Cutscene/[Timeline]/DummyJeeAnimator/View/Jee/JeeSprite",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/[CutScene]BossAngry_Cutscene/[Timeline]/DummyJeeAnimator/View/Jee/Cloak",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/[CutScene]BossAngry_Cutscene/[Timeline]/DummyJeeAnimator/View/Jee/Arm",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/[CutScene]BossDying/[Timeline]/DummyJeeAnimator/View/Jee/JeeSprite",

        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Jee/Cloak",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Jee/Arm",
        // "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Effect/Attack_TeleportSword/Sprite"
    ];

    private static string[] crimsonSpritePaths =
    [
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Weapon/Jee_Sitck/Effect/EffectSprite",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Effect/Attack_TeleportSword/Sprite",
        // "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Weapon/JeeStaffTip/Effect_BEAM/STPBALL/STPBALL2D",
        // "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Weapon/JeeStaffTip/Effect_BEAM/STPBALL/Light",
        // "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Weapon/JeeStaffTip/Effect_BEAM/Light (1)",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Effect/LaserAltar/P_LaserExplosion/Sprite/",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Effect/LaserAltar/P_LaserExplosion/Sprite/light"
    ];

    private static string[] crimsonParticlePaths =
    [
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Weapon/JeeStaffTip/Effect_BEAM/P_Charging",
        "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/Weapon/JeeStaffTip/Effect_BEAM/P_ScretTreePower B"
    ];

    public static SpriteRenderer[] jiSprites = new SpriteRenderer[jiSpritePaths.Length];
    public static SpriteRenderer[] crimsonSprites = new SpriteRenderer[crimsonSpritePaths.Length];
    public static ParticleSystemRenderer[] crimsonParticles = new ParticleSystemRenderer[crimsonParticlePaths.Length];

    private static Func<string, (float x, float y, float z)> parseTuple =
        s => {
            var p = s.Split(',');
            return new (float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
        };

    static public void InitializePairs(
        ((float x, float y, float z) src, (float x, float y, float z) dst)[] pairs, (float x, float y, float z)[] leftTuple, string[] rightString)
    {
        for (int i = 0; i < leftTuple.Length; i++) 
        {
            pairs[i] = (leftTuple[i], parseTuple(rightString[i]));
        }
    }

    static private float ReturnInRange(float n) {
        return Math.Min(Math.Max(n, 0), 255);
    }

    static private void initializeArr(
        ((float x, float y, float z) src, (float x, float y, float z) dst)[] pairs, Vector3[] srcArr, Vector3[] dstArr)
    {
        int index = 0;
        foreach (((float x, float y, float z) src, (float x, float y, float z) dst) in pairs)
        {
            for (float i = -stayRange; i <= stayRange; i++)
            {
                for (float j = -stayRange; j <= stayRange; j++)
                {
                    for (float k = -stayRange; k <= stayRange; k++)
                    {
                        srcArr[index] = new Vector3(ReturnInRange(src.x + i), ReturnInRange(src.y + j), ReturnInRange(src.z + k));
                        dstArr[index++] = new Vector3(ReturnInRange(dst.x + i), ReturnInRange(dst.y + j), ReturnInRange(dst.z + k));
                    }
                }
            }
        }
    }

    static public void getSprites()
    {
        for (int i = 0; i < jiSprites.Length; i++)
        {
            jiSprites[i] = GameObject.Find(jiSpritePaths[i]).GetComponent<SpriteRenderer>();
        }
        for (int i = 0; i < crimsonSprites.Length; i++) {
            crimsonSprites[i] = GameObject.Find(crimsonSpritePaths[i]).GetComponent<SpriteRenderer>();
        }
        for (int i = 0; i < crimsonParticles.Length; i++) {
            crimsonParticles[i] = GameObject.Find(crimsonParticlePaths[i]).GetComponent<ParticleSystemRenderer>();
        }
    }

    static private void InitializeMat(Material mat, (float x, float y, float z)[] originalColors, string[] newColors) {
        if (originalColors.Length != newColors.Length) {
            ToastManager.Toast("Arrays not same length!");
            return;
        }
        ((float x, float y, float z) src, (float x, float y, float z) dst)[] pairs = 
            new ((float x, float y, float z) src, (float x, float y, float z) dst)[originalColors.Length];
        Vector3[] srcColors = new Vector3[originalColors.Length * (int) Math.Pow(stayRange * 2 + 1, 3)];
        Vector3[] dstColors = new Vector3[originalColors.Length * (int) Math.Pow(stayRange * 2 + 1, 3)];

        InitializePairs(pairs, originalColors, newColors);
        initializeArr(pairs, srcColors, dstColors);
        Texture3D lut = RBFLUTGenerator.GenerateRBF_LUT(srcColors, dstColors, lutSize);
        mat.SetTexture("_LUT", lut);
        mat.SetFloat("_LUTSize", lutSize);
    }

    static public void InitializeJiMat(Material mat, string[] newColors) {
        (float x, float y, float z)[] ogColors = 
            [
                (1f, 1f, 1f),  // Black
                (247f, 248f, 241f), // Fur White 
                (186f, 240f, 227f),   // Eye Baby Blue
                (79f, 193f, 129f), // Green Claws/Headband
                (104f, 24f, 23f),  // Cape Red 
                (228f, 190f, 106f), // Robe Yellow
                (212f, 203f, 167f), // Tan Robe Highlight
            ];
        InitializeMat(mat, ogColors, newColors);
    }

    static public void InitializeCrimsonMat(Material mat, string[] newColors) {
        (float x, float y, float z)[] ogColors = 
            [
                (1f, 1f, 1f),  // Black
                (223f, 86f, 96f) // Crimson/Tai Danger Red
            ];
        InitializeMat(mat, ogColors, newColors);
    }

    static public void updateJiSprite(Material mat)
    {
        foreach (SpriteRenderer jiSprite in jiSprites)
        {
            if (jiSprite.material != mat)
            {  
                jiSprite.material = mat;
            }
            
        }
    }

    static public void updateCrimsonSprites(Material mat) {
        foreach (SpriteRenderer crimsonSprite in crimsonSprites) {
            if (crimsonSprite.material != mat) {
                crimsonSprite.material = mat;
            }
        }
        foreach (ParticleSystemRenderer crimsonParticle in crimsonParticles) {
            if (crimsonParticle.material != mat) {
                crimsonParticle.material = mat;
            }
        }
    }
}