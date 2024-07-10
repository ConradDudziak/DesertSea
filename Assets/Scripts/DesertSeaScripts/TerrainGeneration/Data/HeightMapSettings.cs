using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData {
    public NoiseSettings noiseSettings;

    public bool useFalloff;
    public float heightMultiplier;
    public AnimationCurve heightCurve;
}
