using System.Collections;
using System.Collections.Generic;
using UnityEditor.Scripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MapMaker : MonoBehaviour
{

    private const float HorizontalOffsetFactor = 0.768f;
    private const float VerticalOffsetFactor = .475f;
    [FormerlySerializedAs("_mapSize")] public Vector2 mapSize = new Vector2(20, 30);
    public GameObject hex; //assigned in inspector as default hex
    public GameObject hexes; //assigned in inspector as parent of all hexes
    public int masses = 15;
    public Button generateButton;
    CameraControl _cameraControl;

    public Vector2 grow = new Vector2(4, 7);
    public int freq = 3;
    private void Generate()
    {
        foreach(Transform child in hexes.transform)
        {
            Destroy(child.gameObject);
        }

        transform.position = new Vector3(GetComponent<MapMaker>().mapSize.x / 2 * HorizontalOffsetFactor, GetComponent<MapMaker>().mapSize.y / 2 * VerticalOffsetFactor, -10);
        _cameraControl.height = mapSize.y * VerticalOffsetFactor;
        _cameraControl.width = mapSize.x * HorizontalOffsetFactor;
        _cameraControl.barrierLeft.offset = new Vector2(-(mapSize.x * HorizontalOffsetFactor) / 2 - 51, 0);
        _cameraControl.barrierRight.offset = new Vector2((mapSize.x * HorizontalOffsetFactor) / 2 + 51, 0);
        
        //delay the generation of the ocean background so that the camera can be centered first
        GenerateOceanBackground();
        GenerateLandPoints();
        AddShallowWater();
    }
    
    private void GenerateOceanBackground()
    {
        bool offsetFlag = false;
        float y = 0;
        for (float yCounter = 0; yCounter < mapSize.y; yCounter++)
        {
            y += VerticalOffsetFactor;
            float x = 0;
            if(!offsetFlag)
            {
                x = 0;
            }
            else
            {
                x = HorizontalOffsetFactor / 2;
            }
            
            for(int xCounter = 0; xCounter < mapSize.x; xCounter++)
            {
                Vector2 position = new Vector2(x, y);
                Instantiate(hex, position, Quaternion.identity, hexes.transform);
                x += HorizontalOffsetFactor;
            }
            offsetFlag = !offsetFlag;
        }
    }

    private void GenerateLandPoints()
    {
        int massTemp = masses;
        int attempts = 0;

        // Define a margin to avoid placing land too close to the top or bottom
        float margin = mapSize.y * VerticalOffsetFactor * 0.2f; // For example, 20% of the map height

        while (massTemp > 0 && attempts < masses * 2)
        {
            attempts += 1;

            // Generate random x-coordinate as before
            float randomX = Mathf.Round(Random.value * (mapSize.x * HorizontalOffsetFactor - 1));

            // Generate random y-coordinate within the central area of the map
            float randomY = Mathf.Round(Random.value * (mapSize.y * VerticalOffsetFactor - 2 * margin) + margin - 1);
            Debug.Log("Randomly generated coords: " + randomX + ", " + randomY + "");

            Vector2 pos = new Vector2(randomX, randomY);
            Debug.Log("Randomly generated coords: " + pos.ToString());

            pos = GetHexPosition(pos);
            Debug.Log("This is the generated position of the hex: " + pos.ToString());

            RaycastHit2D hit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
            if (hit)
            {
                HexType newHex = hit.collider.gameObject.GetComponent<HexType>();

                //FIX: type is deprecated
                if (newHex.GetHexBiome() == 0)
                {
                    newHex.SetHexBiome(2); // swap with Grasslands sprite
                    newHex.grow = Mathf.RoundToInt(grow.x + Random.value * (grow.y - grow.x));
                    newHex.freq = freq;
                    newHex.width = Mathf.RoundToInt(mapSize.x);

                    newHex.Grow();

                    massTemp -= 1;
                }
            }
        }
    }

    
    private Vector2 GetHexPosition(Vector2 pos)
    {
        // Adjust the position based on the offsets
        float adjustedX = pos.x / HorizontalOffsetFactor;
        float adjustedY = pos.y / VerticalOffsetFactor;

        // Determine the column and row in the hex grid
        int col = Mathf.RoundToInt(adjustedX);
        int row = Mathf.RoundToInt(adjustedY);

        // Correct for the staggering of odd or even rows
        if (col % 2 != 0)
        {
            row = Mathf.RoundToInt((pos.y - VerticalOffsetFactor / 2) / VerticalOffsetFactor);
        }

        // Convert back to world coordinates using the center of the hex
        float worldX = col * HorizontalOffsetFactor;
        float worldY = row * VerticalOffsetFactor;

        // Adjust the y-coordinate for odd columns
        if (col % 2 != 0)
        {
            worldY += VerticalOffsetFactor / 2;
        }

        return new Vector2(worldX, worldY);
    }
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        //for purposes of camera wrapping, the width needs to be at least 1.75 times the height
        if(mapSize.x < mapSize.y * 1.75f)
        {
            mapSize.x = mapSize.y * 1.75f;
        }
        
        
        _cameraControl = GetComponent<CameraControl>();
        Generate();
        generateButton.onClick.AddListener(Generate);
        
        
    }

    private void AddShallowWater()
    {
        foreach(Transform child in hexes.transform)
        {
            //Debug.Log("child found");
            HexType childHex = child.GetComponent<HexType>();
            
            //if the hex is not water, skip it
            if(childHex.GetHexBiome() > 1)
            {
                //Debug.Log("Hex is not water");
                continue;
            }
            
            //else, check if it has a land neighbor
            Vector2 currentPosition = child.position;
            List<Vector2> neighbors = new List<Vector2>();
            neighbors.Add(new Vector2(currentPosition.x + HorizontalOffsetFactor, currentPosition.y));
            neighbors.Add(new Vector2(currentPosition.x - HorizontalOffsetFactor, currentPosition.y));
            neighbors.Add(new Vector2(currentPosition.x + HorizontalOffsetFactor / 2, currentPosition.y + VerticalOffsetFactor));
            neighbors.Add(new Vector2(currentPosition.x - HorizontalOffsetFactor / 2, currentPosition.y - VerticalOffsetFactor));
            neighbors.Add(new Vector2(currentPosition.x - HorizontalOffsetFactor / 2, currentPosition.y + VerticalOffsetFactor));
            neighbors.Add(new Vector2(currentPosition.x + HorizontalOffsetFactor / 2, currentPosition.y - VerticalOffsetFactor));

            for (int i = 0; i < neighbors.Count; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(neighbors[i], neighbors[i], 0, LayerMask.GetMask("Default"));
                if (hit)
                {
                    HexType testHex = hit.collider.gameObject.GetComponent<HexType>();
                    //test if the hex is land
                    if(testHex.GetHexBiome() > 1)
                    {
                        //if it is, set the current hex to shallow water
                        //Debug.Log("Neighbor is land");
                        if (childHex != null)
                        {
                            childHex.SetHexBiome(1);
                        }
                        break;
                    }
                }
            }
        }
    }

    
}
