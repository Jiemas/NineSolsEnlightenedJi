using UnityEngine;

namespace EnlightenedJi;

public static class RBFLUTGenerator
{
    public static Texture3D GenerateRBF_LUT(
        Vector3[] srcColors,
        Vector3[] dstColors,
        int size = 32,
        float eps = 0.1f
    )
    {
        int count = size * size * size;
        Color[] colors = new Color[count];

        int index = 0;

        for (int b = 0; b < size; b++)
            for (int g = 0; g < size; g++)
                for (int r = 0; r < size; r++)
                {
                    Vector3 rgb = new Vector3(
                        r / (size - 1f),
                        g / (size - 1f),
                        b / (size - 1f)
                    );

                    Vector3 mapped = ApplyRBF(srcColors, dstColors, rgb, eps);

                    colors[index++] = new Color(mapped.x, mapped.y, mapped.z, 1);
                }

        Texture3D lut = new Texture3D(size, size, size, TextureFormat.RGBAHalf, false);
        lut.wrapMode = TextureWrapMode.Clamp;
        lut.SetPixels(colors);
        lut.Apply();

        return lut;
    }

    static Vector3 ApplyRBF(
        Vector3[] src,
        Vector3[] dst,
        Vector3 x,
        float eps
    )
    {
        float wSum = 0;
        Vector3 colorSum = Vector3.zero;

        for (int i = 0; i < src.Length; i++)
        {
            float dist = Vector3.Distance(x, src[i] / 255f);
            float w = Mathf.Exp(-(dist * dist) / eps);

            colorSum += dst[i] / 255f * w;
            wSum += w;
        }

        if (wSum <= 1e-6f)
            return x; // fallback: identity

        return colorSum / wSum;
    }
}


