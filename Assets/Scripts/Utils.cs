using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    static int maxHeight = 150;
    static float smooth = 0.01f;
    static int octaves = 4;
    static float persistence = 0.5f;

    public static int GenerateStoneHeight(float x, float y)
    {
        float height = Map(0, maxHeight-5, 0, 1, fBM(x * smooth * 2, y * smooth * 2, octaves + 1, persistence));
        return (int)height;
    }

    public static int GenerateHeight(float x, float y)
    {
        float height = Map(0, maxHeight, 0, 1, fBM(x * smooth, y * smooth, octaves, persistence));
        return (int) height;
    }

    public static float fBM3D(float x, float y, float z, float sm, int oct)
    {
        float XY = fBM(x * sm * 6, y * sm * 6, oct, 0.5f);
        float YZ = fBM(y * sm * 6, z * sm * 6, oct, 0.5f);
        float XZ = fBM(x * sm * 6, z * sm * 6, oct, 0.5f);

        float YX = fBM(y * sm * 6, x * sm * 6, oct, 0.5f);
        float ZY = fBM(z * sm * 6, y * sm * 6, oct, 0.5f);
        float ZX = fBM(z * sm * 6, x * sm * 6, oct, 0.5f);

        return (XY + YZ + XZ + YX + ZY + ZX) / 6.0f;
    }

    static float Map(float newmin, float newmax, float origmin, float origmax, float value)
    {
        return Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(origmin, origmax, value));
    }
    static float fBM(float x, float z, int oct, float pers)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        float offset = 32000f;
        for(int i = 0; i <oct; i++)
        {
            total += Mathf.PerlinNoise((x + offset) * frequency, (z+offset) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= pers;
            frequency *= 2;
        }
        return total / maxValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
