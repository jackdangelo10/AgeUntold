using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

public class HexType : MonoBehaviour
{
    System.Random random = new System.Random();
    public int type = 0;
    public int grow = 1;
    public int freq = 1;
    public int width = 120;
    
    private const float HorizontalOffsetFactor = 0.768f;
    private const float VerticalOffsetFactor = .475f;
    
    //Hex Instance Data
    private int _hexBiomeIndex = 0; //assigned numbers for biomes
    private string _hexBiomeName; //name of hex
    private Resource[] _resources;
    private int _settlementLevel; //0 = no settlement, 1 = village, 2 = town, 3 = city
    private bool _isUnit; //is there a unit on this hex?
    
    
    //only to be called internally when Hex Instance Data is finished being altered
    private void SwapHexSprite()
    {
        
        // Directly access the sprite from HexSpriteManager using biome index
        Sprite sprite = HexSpriteManager.Instance.GetBiomeSprite(_hexBiomeIndex);
        if (sprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }
    
    public int GetHexBiome()
    {
        return _hexBiomeIndex;
    }

    public void SetHexBiome(int biome)
    {
        _hexBiomeIndex = biome;
        SwapHexSprite();
        
        switch (biome)
        {
            case 0:
                _hexBiomeName = "Deep_Ocean";
                break;
            case 1:
                _hexBiomeName = "Shallow_Ocean";
                break;
            case 2:
                _hexBiomeName = "Grasslands";
                break;
            case 3:
                _hexBiomeName = "Plains";
                break;
            case 4:
                _hexBiomeName = "Tundra";
                break;
            case 5:
                _hexBiomeName = "Desert";
                break;
            case 6:
                _hexBiomeName = "Wasteland";
                break;
            case 7:
                _hexBiomeName = "Mountain";
                break;
        }
    }

    public void GenerateResources()
    {
        //1 to 3 resources per tile
        _resources = new Resource[random.Next(1, 4)];
        
        switch (_hexBiomeIndex)
        {
            case 0:
                
                break;
        }
    }

    public void Grow()
    {
        Vector2 currentPosition = transform.position;
        List<Vector2> neighbors = ToolBox.GetNeighbors(currentPosition);

        for (int i = 0; i < freq; i++)
        {
            int m = Mathf.RoundToInt(Random.value * 5);
            if (neighbors[m].x < 0)
            {
                Vector2 newCoord = new Vector2(neighbors[m].x + width * HorizontalOffsetFactor, neighbors[m].y);
                neighbors[m] = newCoord;
            }
            else if (neighbors[m].x >= width * HorizontalOffsetFactor)
            {
                Vector2 newCoord = new Vector2(neighbors[m].x - width, neighbors[m].y);
                neighbors[m] = newCoord;
            }
            
            RaycastHit2D hit = Physics2D.Raycast(neighbors[m], neighbors[m], 0, LayerMask.GetMask("Default"));
            if (hit)
            {
                HexType newHex = hit.collider.gameObject.GetComponent<HexType>();
                
                if (newHex.GetHexBiome() == 0)
                {
                    // swap with Plains sprite
                    newHex.SetHexBiome(3);

                    if (grow - 1 > 0)
                    {
                        newHex.grow = grow - 1;
                        newHex.freq = freq;
                        newHex.width = width;
                        newHex.Grow();
                    }
                }
            }
        }
    }
    
    public bool HasRiver()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "RiverObject")
            {
                return true; // Found a child named "RiverObject"
            }
        }
        return false; // No child named "RiverObject" found
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
