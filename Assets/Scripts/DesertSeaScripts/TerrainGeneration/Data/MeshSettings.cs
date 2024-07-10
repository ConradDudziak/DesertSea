using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData {
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};

    public float meshScale = 3f;
    public bool useFlatShading;

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatShadedChunkSizes - 1)]
    public int flatShadedChunkSizeIndex;

    public List<MeshCrossSectionSetting> meshCrossSectionSettings;

    // num verts per line of mesh rendered at LOD 0. Includes the 2 extra border vertices that are excluded from final mesh, but are used for normal calculations.
    // Note to conrad: Increase the detail of each chunk by reducing chunk size but keeping mesh count the same. This can combat the bumpy sand dunes.
    public int numVertsPerLine {
        get {
            //return supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] - 1 + 2;
            return supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] + 5;
        }
    }

    public float meshWorldSize { 
        get {
            return (numVertsPerLine - 1 - 2) * meshScale;
        }
    }
}

[System.Serializable]
public class MeshCrossSectionSetting {
    [Range(0, 1)]
    public float minHeight;
    [Range(0, 1)]
    public float maxHeight;
}
