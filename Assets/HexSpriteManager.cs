using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSpriteManager : MonoBehaviour
{
    // Array to hold sprites for each biome
    public Sprite[] biomeSprites;

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
}
