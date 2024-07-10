using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
    public static HeightMap GenerateHeightMap(int mapSize, HeightMapSettings settings, Vector2 sampleCenter) {
        float[] values = Noise.GenerateNoiseMap(mapSize, settings.noiseSettings, sampleCenter);
        //AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        // This normalization adds alot of extra computation time. Ideally we should do this during the compute shader. InterlockedMin/Max functions would work, but would add a bottleneck
        // We also can't build our endless terrain seamlessly without a good approximation of max height.
        // values[i] = (values[i] - min) / (max - min);

        float min = float.MaxValue;
        float max = float.MinValue;

        for (int i = 0; i < mapSize * mapSize; i++) {
            values[i] = settings.heightCurve.Evaluate(values[i]) * settings.heightMultiplier;

            if (values[i] < min) {
                min = values[i];
            }
            if (values[i] > max) {
                max = values[i];
            }
        }

        return new HeightMap(values, min, max);
    }
}

public struct HeightMap {
    public readonly float[] values;
    public float minValue;
    public float maxValue;

    public HeightMap(float[] values, float minValue, float maxValue) {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.values = values;
    }
}
