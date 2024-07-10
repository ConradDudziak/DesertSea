using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainDoodadCluster : ScriptableObject {
    public List<TerrainDoodad> terrainDoodads;
    public AnimationCurve weightedDistribution;
}
