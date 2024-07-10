using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainDoodad : ScriptableObject {
    public List<GameObject> prefabOptions;

    [Range(0, 15)]
    public float minSitesPerChunk;
    [Range(0, 15)]
    public float maxSitesPerChunk;

    [Range(0, 1)]
    public float minHeightFoundAt;
    [Range(0, 1)]
    public float maxHeightFoundAt;

    [Range(0, 5)]
    public float minBuryDepth;
    [Range(0, 5)]
    public float maxBuryDepth;

    [Range(0, 180)]
    public float maxRotationX = 30f;
    [Range(0, 180)]
    public float maxRotationY = 180f;
    [Range(0, 180)]
    public float maxRotationZ = 30f;

    [Header("Cluster Fields")]
    [Range(0, 100)]
    public float minPerCluster;
    [Range(0, 100)]
    public float maxPerCluster;
    [Range(0, 1)]
    public float chanceToCauseCluster = 0f;
    [Tooltip("This value is later scaled by the clusters distribution curve")]
    [Range(0, 1)]
    public float chanceToJoinCluster = 0f;
    [Range(0, 10)]
    public int minNumOfOtherDoodadsInCluster = 0;
    [Range(0, 10)]
    public int maxNumOfOtherDoodadsInCluster = 1;

    public Vector3 occupyingSize = Vector3.zero;

    public List<TerrainDoodadCluster> clusterOptions;
}
