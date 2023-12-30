using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSpriteManager : MonoBehaviour
{
    // Array to hold sprites for each biome
    public Sprite[] biomeSprites;
    public Sprite[] terrainSprites;
    public Sprite[] riverSprites;
    private Dictionary<string, int>  _keyToRiverSpriteIndex = new Dictionary<string, int>()
    {
        { "01", 0 },
        { "02", 1 },
        { "03", 2 },
        { "04", 3 },
        { "05", 4 },
        { "12", 5 },
        { "13", 6 },
        { "14", 7 },
        { "15", 8 },
        { "23", 9 },
        { "24", 10 },
        { "25", 11 },
        { "34", 12 },
        { "35", 13 },
        { "45", 14 },
        { "012", 15 },
        { "013", 16 },
        { "014", 17 },
        { "015", 18 },
        { "023", 19 },
        { "024", 20 },
        { "025", 21 },
        { "034", 22 },
        { "035", 23 },
        { "045", 24 },
        { "123", 25 },
        { "124", 26 },
        { "125", 27 },
        { "134", 28 },
        { "135", 29 },
        { "145", 30 },
        { "234", 31 },
        { "235", 32 },
        { "245", 33 },
        { "345", 34 },
        { "0123", 35 },
        { "0124", 36 },
        { "0125", 37 },
        { "0134", 38 },
        { "0135", 39 },
        { "0145", 40 },
        { "0234", 41 },
        { "0235", 42 },
        { "0245", 43 },
        { "0345", 44 },
        { "1234", 45 },
        { "1235", 46 },
        { "1245", 47 },
        { "1345", 48 },
        { "2345", 49 },
        { "01234", 50 },
        { "01235", 51 },
        { "01245", 52 },
        { "01345", 53 },
        { "02345", 54 },
        { "12345", 55 },
        { "012345", 56 }
    };

    // Static instance for global access
    public static HexSpriteManager Instance { get; private set; }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        
        // Ensuring there's only one instance of HexSpriteManager in the scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to get sprite by biome index
    public Sprite GetBiomeSprite(int biomeIndex)
    {
        // Check if the index is within the range of the array
        if (biomeIndex >= 0 && biomeIndex < biomeSprites.Length)
        {
            return biomeSprites[biomeIndex];
        }
        else
        {
            Debug.LogWarning("No sprite found for biome index: " + biomeIndex);
            return null;
        }
    }
    
    public Sprite GetTerrainSprite(int terrainIndex)
    {
        // Check if the index is within the range of the array
        if (terrainIndex >= 0 && terrainIndex < terrainSprites.Length)
        {
            return terrainSprites[terrainIndex];
        }
        else
        {
            Debug.LogWarning("No sprite found for terrain index: " + terrainIndex);
            return null;
        }
    }

    public Sprite GetRiverSprite(int flowSourceIndex, List<int> chosenBranchIndices)
    {
        chosenBranchIndices.Add(flowSourceIndex);
        chosenBranchIndices.Sort(); //sorts the numbers in ascending order
        string key = "";
        foreach (int i in chosenBranchIndices)
        {
            key += i.ToString();
        }
        Debug.Log("Key: " + key);
        
        if (_keyToRiverSpriteIndex.TryGetValue(key, out int spriteIndex))
        {
            Debug.Log("Sprite index: " + spriteIndex + " for key: " + key + "");
            Debug.Log("riverSprites size: " + riverSprites.Length + "");
            return riverSprites[spriteIndex];
        }

        return null;
    }
}

