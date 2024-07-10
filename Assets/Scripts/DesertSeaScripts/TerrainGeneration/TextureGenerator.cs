using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {
    public static Texture2D TextureFromColorMap(Color[] colorMap, int mapSize) {
        Texture2D texture = new Texture2D(mapSize, mapSize);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(HeightMap heightMap, int mapSize) {
        Color[] colorMap = new Color[mapSize * mapSize];

        for (int i = 0; i < heightMap.values.Length; i++) {
            colorMap[i] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[i]));
        }

        return TextureFromColorMap(colorMap, mapSize);
    }
}
