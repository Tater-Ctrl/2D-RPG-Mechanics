using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseMap
{
    public static float fBM(float x, float y, int seed, float scale, int octaves, float persistence) {
        
        System.Random prng = new System.Random (seed);
        float seedX = prng.Next (-100000, 100000) + x;
        float seedY = prng.Next (-100000, 100000) + y;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        float halfWidth = seedX / 2f;
        float halfHeight = seedY / 2f;

        for(int i = 0; i < octaves; i++) {
            total += Mathf.PerlinNoise((halfWidth * frequency) * scale, (halfHeight * frequency) * scale) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total/maxValue;
    }
}