using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    public static float[] GenerateNoiseMap(int mapSize, NoiseSettings settings, Vector2 sampleCenter) {

        System.Random prng = new System.Random(settings.seed);
        float offsetX = prng.Next(-1000, 1000) + settings.offset.x + sampleCenter.x; // Too large of values can mess up the noise function
        float offsetY = prng.Next(-1000, 1000) + settings.offset.y + sampleCenter.y; // Too large of values can mess up the noise function

        ComputeShader voronoiNoise = settings.noiseShader;
        int voronoiNoiseHandle = voronoiNoise.FindKernel("CSMain");

        voronoiNoise.SetFloat("_Resolution", (float)mapSize);                    // -- Some nice defaults
        voronoiNoise.SetFloat("_Frequency", (float)settings.frequency);          // 0.0025f
        voronoiNoise.SetFloat("_AngleOffset1", (float)settings.angleOffset1);    // 2.01f
        voronoiNoise.SetFloat("_CellDensity1", (float)settings.cellDensity1);    // 4.02f
        voronoiNoise.SetFloat("_AngleOffset2", (float)settings.angleOffset2);    // 2.74f
        voronoiNoise.SetFloat("_CellDensity2", (float)settings.cellDensity2);    // 11.53f;
        voronoiNoise.SetFloat("_LerpDelta", (float)settings.lerpDelta);          // 0.65f
        voronoiNoise.SetFloat("_NoiseScale", (float)settings.simpleNoiseScale);  // 15f
        voronoiNoise.SetFloat("_OffsetX", (float)(settings.frequency * offsetX)); // used to do (mapSize - 3) * the rest of the values. -1 + -2 for border. But we dont now because sampleCenter is scaled
        voronoiNoise.SetFloat("_OffsetY", (float)(settings.frequency * -offsetY)); // used to do (mapSize - 3) * the rest of the values. -1 + -2 for border. But we dont now because sampleCenter is scaled

        float[] noiseMap = new float[mapSize * mapSize];
        ComputeBuffer noiseMapBuffer = new ComputeBuffer(mapSize * mapSize, sizeof(float));
        noiseMapBuffer.SetData(noiseMap);
        voronoiNoise.SetBuffer(voronoiNoiseHandle, "_HeightMap", noiseMapBuffer);

        voronoiNoise.Dispatch(voronoiNoiseHandle, (256) / 8, (256) / 8, 1); // We want 32 * 8 = 256 total threads in each dimension ( even if our map isnt 256)

        noiseMapBuffer.GetData(noiseMap);
        noiseMapBuffer.Dispose();

        return noiseMap;
    }
}

[System.Serializable]
public class NoiseSettings {
    public ComputeShader noiseShader;
    [Range(0.001f, 0.0075f)]
    public float frequency;
    [Range(1f, 8f)]
    public float angleOffset1;
    [Range(0f, 6f)]
    public float cellDensity1;
    [Range(1f, 8f)]
    public float angleOffset2;
    [Range(0f, 25f)]
    public float cellDensity2;
    [Range(0f, 1f)]
    public float lerpDelta;
    [Range(1f, 50f)]
    public float simpleNoiseScale;

    public int seed;
    public Vector2 offset;
}
