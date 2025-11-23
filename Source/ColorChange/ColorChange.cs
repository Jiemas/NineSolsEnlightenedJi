using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using NineSolsAPI;
using NineSolsAPI.Utils;

namespace EnlightenedJi;

//  C:/Users/Jiemas/AppData/LocalLow/RedCandleGames/NineSols
// string spritePath = "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/";

public class ColorChange {
    public static int lutSize = 16;

    private static Vector3[] srcColors =
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(104f, 24f, 23f),
        new Vector3(228f, 190f, 106f),
        new Vector3(247f, 248f, 241f),
        new Vector3(186f, 240f, 227f),
        new Vector3(117f, 220f, 208f),
        new Vector3(192f, 247f, 237f)
    };

    private static Vector3[] dstColors =
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(37f, 44f, 31f),
        new Vector3(233f, 238f, 236f),
        new Vector3(237f, 237f, 213f),
        new Vector3(153f, 102f, 204f),
        new Vector3(153f, 102f, 204f),
        new Vector3(153f, 102f, 204f)
    };

    static public SpriteRenderer getJiSprite()
    {
        string spritePath = "A10S5/Room/Boss And Environment Binder/General Boss Fight FSM Object 姬 Variant/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_Jee/MonsterCore/Animator(Proxy)/Animator/View/";
        return GameObject.Find($"{spritePath}Jee/JeeSprite").GetComponent<SpriteRenderer>();
    }

    static public void RecolorSprite(Material mat) {
        // ToastManager.Toast("RecolorSPrite called!");
        Texture3D lut = RBFLUTGenerator.GenerateRBF_LUT(srcColors, dstColors, lutSize);
        mat.SetTexture("_LUT", lut);
        mat.SetFloat("_LUTSize", lutSize);
    }
}