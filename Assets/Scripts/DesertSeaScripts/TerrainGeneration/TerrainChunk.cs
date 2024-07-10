using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {

    const float colliderGenerationDistanceThreshold = 5f;
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    GameObject meshObject;
    Vector2 sampleCenter;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapReceived;
    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDst;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    TerrainDoodadSettings terrainDoodadSettings;
    public List<Bounds> terrainDoodadBounds;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, TerrainDoodadSettings terrainDoodadSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material) {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.terrainDoodadSettings = terrainDoodadSettings;
        this.viewer = viewer;

        sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);
        terrainDoodadBounds = new List<Bounds>();

        meshObject = new GameObject("TerrainChunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++) {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex) {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }

            if (i == 0) {
                lodMeshes[i].updateCallback += UpdateTerrainDoodads;
            }
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
    }

    public void Load() {
        // Ideally we would generate our height map on a thread, but we can't use compute shaders off the main thread
        this.heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, heightMapSettings, sampleCenter);
        heightMapReceived = true;

        // Generate the first mesh
        UpdateTerrainChunk();
    }

    Vector2 viewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateTerrainChunk() {
        if (!heightMapReceived) {
            return;
        }

        float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

        bool wasVisible = IsVisible();
        bool visible = viewerDstFromNearestEdge <= maxViewDst;

        if (visible) {
            int lodIndex = 0;

            for (int i = 0; i < detailLevels.Length - 1; i++) {
                if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
                    lodIndex = i + 1;
                } else {
                    break;
                }
            }

            // LOD index has changed, chunk needs to change its mesh
            if (lodIndex != previousLODIndex) {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh) {
                    previousLODIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                } else if (!lodMesh.hasRequestedMesh) {
                    lodMesh.RequestMesh(heightMap, meshSettings);
                }
            }
        }

        if (wasVisible != visible) {
            SetVisible(visible);
            if (onVisibilityChanged != null) {
                onVisibilityChanged(this, visible);
            }
        }
    }

    public void UpdateCollisionMesh() {
        if (hasSetCollider)
            return;

        float sqrDistFromViewerToEdge = bounds.SqrDistance(viewerPosition);

        if (sqrDistFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold) {
            if (!lodMeshes[colliderLODIndex].hasRequestedMesh) {
                lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
            }
        }

        if (sqrDistFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) {
            if (lodMeshes[colliderLODIndex].hasMesh) {
                meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                hasSetCollider = true;
            }
        }
    }

    public void UpdateTerrainDoodads() {
        // We can safely do this because we subscribe this function to the callback of the thread that generates the lodMesh with index 0
        LODMesh lodMesh = lodMeshes[0];

        foreach (TerrainDoodad terrainDoodad in terrainDoodadSettings.terrainDoodads) {
            // How many sites of this doodad will we place? 
            // Total sites can be 0, 1, or many, where a site contains 1 doodad, or a cluster of doodads
            int totalSites = Mathf.RoundToInt(Random.Range(terrainDoodad.minSitesPerChunk, terrainDoodad.maxSitesPerChunk));
            if (totalSites == 0) {
                continue;
            }

            bool clusterOptionsExist = terrainDoodad.clusterOptions != null && terrainDoodad.clusterOptions.Count > 0;
            List<int> validTriangles = lodMesh.meshData.GetTrianglesFromHeightBounds(terrainDoodad.minHeightFoundAt, terrainDoodad.maxHeightFoundAt);

            for (int x = 0; x < totalSites; x++) {
                if (terrainDoodad.chanceToCauseCluster >= Random.value && clusterOptionsExist) {
                    CreateTerrainDoodadCluster(terrainDoodad, validTriangles, lodMesh.meshData);
                } else {
                    CreateTerrainDoodad(terrainDoodad, validTriangles, lodMesh.meshData, Vector3.zero, false);
                }
            }
        }
    }

    public GameObject CreateTerrainDoodad(TerrainDoodad terrainDoodad, List<int> validTriangles, MeshData meshData, Vector3 position, bool belongsToCluster) {
        if (!belongsToCluster) {
            // Choose a random triangle from the valid triangles
            int totalTriangles = validTriangles.Count / 3;
            int randTriangleIndex = Random.Range(0, totalTriangles);
            int triangleIndex = validTriangles[randTriangleIndex];

            // Get a random point on the triangle
            Vector3 randTriangleVertexA = meshData.vertices[meshData.triangles[triangleIndex]];
            Vector3 randTriangleVertexB = meshData.vertices[meshData.triangles[triangleIndex + 1]];
            Vector3 randTriangleVertexC = meshData.vertices[meshData.triangles[triangleIndex + 2]];

            float r1 = Random.Range(0, 1f);
            float r2 = Random.Range(0, 1f);

            position = ((1 - Mathf.Sqrt(r1)) * randTriangleVertexA) + (Mathf.Sqrt(r1) * (1 - r2) * randTriangleVertexB) + (r2 * Mathf.Sqrt(r1) * randTriangleVertexC);
        }

        /*GameObject verTest = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        verTest.transform.position = meshData.vertices[meshData.triangles[triangleIndex]];
        verTest.transform.localScale = Vector3.one * 0.1f;
        verTest.name = triangleIndex + "";*/

        // Displace the position beneath the surface to bury the doodad a bit
        float buryDepth = Random.Range(terrainDoodad.minBuryDepth, terrainDoodad.maxBuryDepth);
        position += Vector3.down * buryDepth;
        
        // Add the doodads bounds to our list of known doodad bounds
        terrainDoodadBounds.Add(new Bounds(position, terrainDoodad.occupyingSize));

        // Add the chunks world location to the position so that the position is correct in world space
        // TerrainDoodads in a cluster already account for worldspace since they use the parent doodad for position
        if (!belongsToCluster) {
            position = position + new Vector3(bounds.center.x, 0, bounds.center.y);
        }

        // Create the rotation that we want to apply to the doodad
        Quaternion rotation = Quaternion.Euler(
                Random.Range(-terrainDoodad.maxRotationX, terrainDoodad.maxRotationX),
                Random.Range(-terrainDoodad.maxRotationY, terrainDoodad.maxRotationY),
                Random.Range(-terrainDoodad.maxRotationZ, terrainDoodad.maxRotationZ));

        // Place the terrain doodad in the world
        GameObject terrainDoodadPrefab = terrainDoodad.prefabOptions[Random.Range(0, terrainDoodad.prefabOptions.Count)];
        return GameObject.Instantiate(terrainDoodadPrefab, position, rotation, meshObject.transform);
    }

    public void CreateTerrainDoodadCluster(TerrainDoodad terrainDoodad, List<int> validTriangleIndices, MeshData meshData) {
        // Place the Doodad responsible for the cluster
        GameObject parentDoodad = CreateTerrainDoodad(terrainDoodad, validTriangleIndices, meshData, Vector3.zero, false);

        // Choose one of the clusters 
        TerrainDoodadCluster terrainDoodadCluster = terrainDoodad.clusterOptions[Random.Range(0, terrainDoodad.clusterOptions.Count)];

        // Choose how many doodads can join the cluster
        int maxOtherDoodadsInCluster = Random.Range(terrainDoodad.minNumOfOtherDoodadsInCluster, terrainDoodad.maxNumOfOtherDoodadsInCluster + 1);

        Dictionary<TerrainDoodad, int> chosenDoodadDict = new Dictionary<TerrainDoodad, int>();

        // Start Creating doodads associated with the chosen terrainDoodadCluster
        // Keep track of the doodads we create with the chosenDoodadDict, this way we know how many of each type we are allowed to add to the cluster
        for (int numOtherDoodadsInCluster = 0; numOtherDoodadsInCluster < maxOtherDoodadsInCluster; numOtherDoodadsInCluster++) {
            // Choose which doodad to add to the cluster based on weight. 
            // We evaluate the weight with the clusters distribution, so that cluster has the end say in the weighted distribution of its members
            float totalWeight = 0f;
            AnimationCurve distribution = terrainDoodadCluster.weightedDistribution;
            foreach (TerrainDoodad tcDoodad in terrainDoodadCluster.terrainDoodads) {
                // Only contribute doodads to our weight if they are still available
                if (chosenDoodadDict.TryGetValue(tcDoodad, out int remaining)) {
                    if (remaining <= 0) {
                        continue;
                    }
                }

                totalWeight += distribution.Evaluate(tcDoodad.chanceToJoinCluster);
            }

            float randomWeight = Random.value * totalWeight;
            TerrainDoodad doodadToAddToCluster = null;

            // Now use the weights to select a random terraindoodad
            foreach (TerrainDoodad tcDoodad in terrainDoodadCluster.terrainDoodads) {
                // Only perform random weighted selection on this doodad if it is still available
                if (chosenDoodadDict.TryGetValue(tcDoodad, out int remaining)) {
                    if (remaining <= 0) {
                        continue;
                    }
                }

                if (randomWeight < terrainDoodadCluster.weightedDistribution.Evaluate(tcDoodad.chanceToJoinCluster)) {
                    doodadToAddToCluster = tcDoodad;
                    break;
                }
                randomWeight -= terrainDoodadCluster.weightedDistribution.Evaluate(tcDoodad.chanceToJoinCluster);
            }

            if (doodadToAddToCluster == null) {
                if (chosenDoodadDict.Keys.Count >= terrainDoodadCluster.terrainDoodads.Count) {
                    // Stop trying to grow the cluster if we can't add any doodads to it. Limit was reached.
                    break;
                }
                // This condition should never be met, but just in case we can log an error and continue
                Debug.LogError("Error - No TerrainDoodad could be found to add to the cluster");
                continue;
            }

            if (!chosenDoodadDict.ContainsKey(doodadToAddToCluster)) {
                // Set the amount of these doodads that this cluster can add
                chosenDoodadDict.Add(doodadToAddToCluster, Mathf.RoundToInt(Random.Range(doodadToAddToCluster.minPerCluster, doodadToAddToCluster.maxPerCluster)));
            }

            if (chosenDoodadDict[doodadToAddToCluster] > 0) {
                // Decrease the amount of these doodads that this cluster can add
                chosenDoodadDict[doodadToAddToCluster] = chosenDoodadDict[doodadToAddToCluster] - 1;

                // Determine a radius for the displacement of the child from the parent
                float radius = (terrainDoodad.occupyingSize.x / 2) + (doodadToAddToCluster.occupyingSize.x / 2);

                // Find a valid displaced position for the child, this could also eventually consider bounding boxes to prevent overlapping doodads
                int vertexIndex = -1;
                int attempts = 0;
                Vector3 childDisplacedPosition = Vector3.zero;
                while (attempts < 3 && vertexIndex == -1) {
                    // Slightly randomize the radius length of the displacement
                    Vector2 randomDisplacement = Random.insideUnitCircle.normalized * radius * Random.Range(1, 1.75f);
                    childDisplacedPosition = parentDoodad.transform.position + new Vector3(randomDisplacement.x, 0, randomDisplacement.y);

                    // Find an approximate height for the child doodad
                    vertexIndex = GetApproximateVertexIndexFromWorldSpace(childDisplacedPosition, meshData);
                    attempts++;
                }
                //Debug.DrawLine(childDisplacedPosition, parentDoodad.transform.position, Color.red, 120f);

                // Create the doodad if we succeeded in finding an appropriate position for it
                if (vertexIndex != -1) {
                    Vector3 childPositionWithAproxHeight = new Vector3(childDisplacedPosition.x, meshData.vertices[vertexIndex].y, childDisplacedPosition.z);

                    GameObject childDoodad = CreateTerrainDoodad(doodadToAddToCluster, validTriangleIndices, meshData, childPositionWithAproxHeight, true);
                    childDoodad.transform.SetParent(parentDoodad.transform);
                }
            } 
        }
    }

    public void SetVisible(bool visible) {
        meshObject.SetActive(visible);
    }

    public bool IsVisible() {
        return meshObject.activeSelf;
    }

    private int GetApproximateVertexIndexFromWorldSpace(Vector3 position, MeshData meshData) {
        Vector2 clusterItemPositionWS = new Vector2(Mathf.Round(position.x), Mathf.Round(position.z));
        Vector2 clusterItemPositionLS = (clusterItemPositionWS - new Vector2(bounds.center.x, bounds.center.y)) / meshSettings.meshScale + (new Vector2(1, -1) * meshSettings.meshWorldSize / 2f / meshSettings.meshScale);
        clusterItemPositionLS = clusterItemPositionLS * new Vector2(1, -1);

        bool outOfBoundsX = clusterItemPositionLS.x < 0 || clusterItemPositionLS.x > meshData.width - 1;
        bool outOfBoundsY = clusterItemPositionLS.y < 0 || clusterItemPositionLS.y > meshData.width - 1;
        if (outOfBoundsX || outOfBoundsY) {
            return -1;
        }

        return ((int)clusterItemPositionLS.y) * meshData.width + ((int)clusterItemPositionLS.x);
    }
}

class LODMesh {
    public Mesh mesh;
    public MeshData meshData;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod) {
        this.lod = lod;
    }

    void OnMeshDataReceived(object meshDataObject) {
        meshData = (MeshData)meshDataObject;
        mesh = meshData.CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap, meshSettings, lod), OnMeshDataReceived);
    }
}
