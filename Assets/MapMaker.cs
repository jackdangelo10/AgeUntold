using System.Collections.Generic;
using System;
using Unity.VisualScripting;
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
    public int maxRiverSourceCount = 3;
    private List<HexType> landSources = new List<HexType>();
    
    
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
        AddShallowWater(); //optimization where we only check shallow water neighbor hexes for river sources
        AddBiomes();
        AddTerrain(); //also adds rivers
        AddMountains(UnityEngine.Random.Range(1,5) + 4);
        AddRivers();
    }
    
    private void GenerateOceanBackground()
    {
        bool offsetFlag = false;
        float y = 0;
        for (float yCounter = 0; yCounter < mapSize.y; yCounter++)
        {
            y += VerticalOffsetFactor;
            float x;
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
            float randomX = Mathf.Round(UnityEngine.Random.value * (mapSize.x * HorizontalOffsetFactor - 1));

            // Generate random y-coordinate within the central area of the map
            float randomY = Mathf.Round(UnityEngine.Random.value * (mapSize.y * VerticalOffsetFactor - 2 * margin) + margin - 1);
            //Debug.Log("Randomly generated coords: " + randomX + ", " + randomY + "");

            Vector2 pos = new Vector2(randomX, randomY);
            //Debug.Log("Randomly generated coords: " + pos.ToString());

            pos = GetHexPosition(pos);
            //Debug.Log("This is the generated position of the hex: " + pos.ToString());

            RaycastHit2D hit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
            if (hit)
            {
                HexType newHex = hit.collider.gameObject.GetComponent<HexType>();
                
                if (newHex.GetHexBiome() == 0)
                {
                    newHex.SetHexBiome(3); // swap with Plains sprite
                    newHex.grow = Mathf.RoundToInt(grow.x + UnityEngine.Random.value * (grow.y - grow.x));
                    newHex.freq = freq;
                    newHex.width = Mathf.RoundToInt(mapSize.x);
                    newHex.Grow();
                    
                    landSources.Add(newHex);

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

    //FIX: Handle case of hexes on the edge of the map?????
    private void AddShallowWater()
    {
        foreach(Transform child in hexes.transform)
        {
            //Debug.Log("child found");
            HexType childHex = child.GetComponent<HexType>();
            
            //if the hex is not water, check if it is an edge case and fix it
            if(childHex.GetHexBiome() > 1)
            {
                continue;
            }
            
            //else, check if it has a land neighbor
            Vector2 currentPosition = child.position;
            List<Vector2> neighbors = ToolBox.GetNeighbors(currentPosition);

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

    private void AddBiomes()
    {
        
        GameObject hexesParent = GameObject.Find("Hexes");
        
        if (hexesParent != null)
        {
            for (int i = 0; i < hexesParent.transform.childCount; i++)
            {
                GameObject currentHexIso = hexesParent.transform.GetChild(i).gameObject;
                HexType currentHexType = currentHexIso.GetComponent<HexType>();
                
                //if hex is land and in the north or south, high chance of being tundra.
                //if hex is land and in 
                if (currentHexType.GetHexBiome() > 1)
                {
                    Vector2 position = currentHexIso.transform.position;
                    currentHexType.SetHexBiome(BiomeSelector(position.y, mapSize.y * VerticalOffsetFactor));
                }
            }
        }
    }
    
    public int BiomeSelector(float y, float yMax)
    {
        // Define ranges for Grasslands and Tundra
        float grasslandsLower = 0.1f * yMax;
        float grasslandsUpper = 0.9f * yMax;
        float grasslandsMidLower = yMax / 3;
        float grasslandsMidUpper = 2 * yMax / 3;
        float tundraMargin = 0.15f * yMax;
        
        
        // Tundra probability - very high near the poles
        float tundraProb;
        if (y < tundraMargin || y > yMax - tundraMargin)
        {
            float distanceFromPole = Mathf.Min(y, yMax - y);
            tundraProb = Mathf.Clamp01(1 - Mathf.Pow(distanceFromPole / tundraMargin, 2));
        }
        else
        {
            tundraProb = 0;
        }

        // Desert probability - reduced
        float desertScalingFactor = 0.4f;
        float desertProb = Mathf.Max(0, 1 - Mathf.Abs(y - yMax / 2) / (yMax / 4)) * desertScalingFactor;

        // Grasslands probability - small chance in the middle
        float middleGrasslandsProb = 0.1f; // Small probability for grasslands in the middle
        float grasslandsProb;
        if (grasslandsLower <= y && y <= grasslandsMidLower ||
            grasslandsMidUpper <= y && y <= grasslandsUpper)
        {
            grasslandsProb = 0.5f; // Regular probability in specified ranges
        }
        else if (y > grasslandsMidLower && y < grasslandsMidUpper)
        {
            grasslandsProb = middleGrasslandsProb; // Small chance in the middle
        }
        else
        {
            grasslandsProb = 0;
        }

        // Normalize probabilities (including a chance for 'None')
        float totalProb = tundraProb + desertProb + grasslandsProb;
        if (totalProb > 1)
        {
            tundraProb /= totalProb;
            desertProb /= totalProb;
            grasslandsProb /= totalProb;
        }

        // Decide the biome
        float randomChoice = UnityEngine.Random.Range(0f, 1f);
        if (randomChoice < tundraProb)
            return 4; // Tundra sprite index
        else if (randomChoice < tundraProb + desertProb)
            return 5; // Desert sprite index
        else if (randomChoice < tundraProb + desertProb + grasslandsProb)
            return 2; // Grasslands sprite index
        else
            return 3; // Remain plains
    }

    private void AddTerrain()
    {
        GameObject hexesParent = GameObject.Find("Hexes");
        if (hexesParent != null)
        {
            for (int i = 0; i < hexesParent.transform.childCount; i++)
            {
                GameObject currentHexIso = hexesParent.transform.GetChild(i).gameObject;
                HexType currentHexType = currentHexIso.GetComponent<HexType>();
                
                if(currentHexIso.GetComponent<HexType>().GetHexBiome() < 2) //skip water hexes
                {
                    continue;
                }

                // Retrieve position
                Vector2 position = currentHexIso.transform.position;
                
                Sprite sprite = null;
                
                // Logic for determining terrain
                float randomChoice = UnityEngine.Random.Range(0f, 1f);
                
                if(currentHexType.GetHexBiome() == 2) //grasslands
                {
                    if(randomChoice > .7f)
                    {
                        sprite = HexSpriteManager.Instance.GetTerrainSprite(0); //forest
                    }
                }
                else if(currentHexType.GetHexBiome() == 3) //plains
                {
                    if(randomChoice > .8f)
                    {
                        sprite = HexSpriteManager.Instance.GetTerrainSprite(1); //hills
                    }
                }
                else if(currentHexType.GetHexBiome() == 4) //tundra
                {
                    if (randomChoice > .9f)
                    {
                        sprite = HexSpriteManager.Instance.GetTerrainSprite(0); //forest
                    }
                }
                else if(currentHexType.GetHexBiome() == 5) //desert
                {
                    
                }
                else if(currentHexType.GetHexBiome() == 6) //wasteland
                {
                    
                }
                else
                {
                    Debug.Log("Error: No terrain sprite found for biome index: " + currentHexType.GetHexBiome());
                }
        
                
                // create new GameObject child of current hex that is the terrain
                if (sprite != null)
                {
                    // Create new GameObject
                    GameObject terrainObject = new GameObject("TerrainObject");

                    // Add SpriteRenderer and set the sprite
                    SpriteRenderer spriteRenderer = terrainObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = sprite;
                
                    // Set the terrain object as a child of the current hex
                    terrainObject.transform.SetParent(currentHexIso.transform, false);
                
                    // Set position to be the same as the parent hex
                    terrainObject.transform.position = position;

                    // Optionally adjust the sorting layer or order if needed, supposed to be one layer above the hex
                    spriteRenderer.sortingOrder = currentHexIso.GetComponent<SpriteRenderer>().sortingOrder + 1;
                }
            }
        }

    }

    private void AddMountains(int mountainRangeCount)
    {
        int attempts = 0;
        int mountainsPlaced = 0;

        // Define a margin as before
        float margin = mapSize.y * VerticalOffsetFactor * 0.2f;

        while (mountainsPlaced < mountainRangeCount && attempts < mountainRangeCount * 2)
        {
            attempts++;

            // Generate random x and y coordinates within the map bounds, avoiding the margins
            float randomX = Mathf.Round(UnityEngine.Random.value * (mapSize.x * HorizontalOffsetFactor - 1));
            float randomY = Mathf.Round(UnityEngine.Random.value * (mapSize.y * VerticalOffsetFactor - 2 * margin) + margin - 1);

            Vector2 pos = new Vector2(randomX, randomY);
            pos = GetHexPosition(pos);

            // takes a source hex and turns it into a mountain range
            RaycastHit2D hit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
            if (hit)
            {
                GameObject currentHexIso = hit.collider.gameObject;
                HexType hexType = currentHexIso.GetComponent<HexType>();
                
                // Check if the hex is land (biome > 1)
                if (hexType.GetHexBiome() > 1)
                {
                    hexType.SetHexBiome(7); // Set the source hex's biome to mountains
                    //delete other terrain
                    foreach (Transform child in hexType.gameObject.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                    
                    int mountainMax = UnityEngine.Random.Range(1, 9); //max number of mountains in a mountain range
                    int mountainCount = 1;
                    int waterCount = 0;
                    
                    // Get the hex at the random neighbor
                    int randomNeighbor = UnityEngine.Random.Range(0, 6);
                    while(mountainCount < mountainMax)
                    {
                        //collect neighboring hexes
                        List<Vector2> neighbors = ToolBox.GetNeighbors(currentHexIso.transform.position);    
                        //Debug.Log("randomNeighbor: " + randomNeighbor + "");
                        
                        RaycastHit2D neighborHit = Physics2D.Raycast(neighbors[randomNeighbor], neighbors[randomNeighbor], 0, LayerMask.GetMask("Default"));
                        if (neighborHit)
                        {
                            HexType neighborHexType = neighborHit.collider.gameObject.GetComponent<HexType>();
                            // Check if the hex is land (biome > 1)
                            if (neighborHexType.GetHexBiome() > 1)
                            {
                                // Set the biome to hills
                                neighborHexType.SetHexBiome(7); //set the neighbor to mountains
                                //delete other terrain
                                foreach (Transform child in neighborHexType.gameObject.transform)
                                {
                                    GameObject.Destroy(child.gameObject);
                                }
                                
                                currentHexIso = neighborHit.collider.gameObject; //set the current hex to the neighbor
                                randomNeighbor = UnityEngine.Random.Range(0, 6); //set the random neighbor to a new random neighbor
                                mountainCount++;
                            }
                            else
                            {
                                //if the neighbor is water, increment the water count and change which neighbor is checked
                                //circular increment of randomNeighbor
                                randomNeighbor = (randomNeighbor + 1) % neighbors.Count;
                                waterCount++;
                            }
                            
                            //if there are too many water hexes as neighbors, forget the mountain range
                            if(waterCount > 5)
                            {
                                break;
                            }
                        }
                        else
                        {
                            //if the neighbor is off the map, forget the mountain range
                            break;
                        }
                    }

                    mountainsPlaced++;
                }
            }
        }
    }



    private void AddRivers()
    {
        HashSet<Tuple<HexType, int>> riverSources = RandomRiverSources();
        HashSet<Tuple<HexType, int>> landSourceRiverSources = LandSourceRiverSources();
        riverSources.UnionWith(landSourceRiverSources);
        
        foreach(Tuple<HexType, int> riverSource in riverSources)
        {
            int riverLength = UnityEngine.Random.Range(1, 18) + 3;
            GenerateRiver(riverSource, riverLength); 
        }
    }
    
    //FIX: connect final river hex to ocean if applicable
    private void GenerateRiver(Tuple<HexType, int> riverSource, int riverLength)
    {
        
        HexType hexType = riverSource.Item1;
        int flowSourceIndex = riverSource.Item2;
        GameObject currentHexIso = hexType.gameObject;
        Vector2 currentPosition = hexType.transform.position;
        Debug.Log("Current position: " + currentPosition.ToString());
        
        //Base case, turn it into shallow water
        if (riverLength <= 0)
        {
            //delete currentHexIso's children
            foreach (Transform child in currentHexIso.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            //set the hex to shallow water
            hexType.SetHexBiome(1);
            Debug.Log("END OF RIVER");
            return;
        }
        
        
        
        
        
        
        List<Vector2> neighbors = ToolBox.GetNeighbors(currentPosition);
        
        //Debug.Log("Current position: " + currentPosition.ToString() + " " + "Flow source index: " + flowSourceIndex + "");
        
        int chosenBranchIndex = -1;
        int nextNeighbor = (flowSourceIndex + 3) % 6;
        List<int> possibleBranchIndices = new List<int>();
        possibleBranchIndices.Add(nextNeighbor);
        possibleBranchIndices.Add((nextNeighbor + 1) % 6);
        possibleBranchIndices.Add((nextNeighbor - 1) % 6);
        int attempts = 0;        
        while(attempts < 3)
        {
            int randomIndex = UnityEngine.Random.Range(0, possibleBranchIndices.Count);
            RaycastHit2D neighborHit = Physics2D.Raycast(neighbors[randomIndex], Vector2.zero, 0, LayerMask.GetMask("Default"));
            if (neighborHit)
            {
                HexType neighborHexType = neighborHit.collider.gameObject.GetComponent<HexType>();
                // Check if the hex is land (biome > 1) and doesn't already have a river and isn't a mountain
                if (neighborHexType.GetHexBiome() > 1 && !neighborHexType.HasRiver() && neighborHexType.GetHexBiome() != 7)
                {
                    chosenBranchIndex = randomIndex;
                    break; // Exit the loop if a suitable neighbor is found
                }
            }
            
            possibleBranchIndices.RemoveAt(randomIndex);
            attempts++;
        }

        // Check after the loop if a neighbor was chosen
        if (chosenBranchIndex == -1)
        {
            Debug.Log("No suitable neighbor found, ending river generation.");
            foreach (Transform child in currentHexIso.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            //set the hex to shallow water
            hexType.SetHexBiome(1);
            Debug.Log("END OF RIVER");
            return;
        }
        //Debug.Log("Chosen branch index: " + chosenBranchIndex + "");
        
        //add the chosenBranchIndex to the chosenBranchIndices
        List<int> chosenBranchIndices = new List<int>();
        chosenBranchIndices.Add(chosenBranchIndex);
        
        //get the river sprite
        Sprite riverSprite = HexSpriteManager.Instance.GetRiverSprite(flowSourceIndex, chosenBranchIndices);
        
        //create a new GameObject child of current hex that is the river
        if (riverSprite != null)
        {
            //delete other terrain
            foreach (Transform child in hexType.gameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            // Create new GameObject
            GameObject riverObject = new GameObject("RiverObject");

            // Add SpriteRenderer and set the sprite
            SpriteRenderer spriteRenderer = riverObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = riverSprite;
                
            // Set the terrain object as a child of the current hex
            riverObject.transform.SetParent(currentHexIso.transform, false);
                
            // Set position to be the same as the parent hex
            riverObject.transform.position = currentPosition;

            // Optionally adjust the sorting layer or order if needed, supposed to be one layer above the hex
            spriteRenderer.sortingOrder = currentHexIso.GetComponent<SpriteRenderer>().sortingOrder + 1;
        }
        
        
        //get the neighbor hex
        RaycastHit2D chosenNeighborHit = Physics2D.Raycast(neighbors[chosenBranchIndex], neighbors[chosenBranchIndex], 0, LayerMask.GetMask("Default"));
        if (chosenNeighborHit)
        {
            HexType chosenNeighborHexType = chosenNeighborHit.collider.gameObject.GetComponent<HexType>();
            // Check if the hex is land (biome > 1)
            if (chosenNeighborHexType.GetHexBiome() > 1)
            {
                //set the neighbor hex to be the new current hex
                Debug.Log("START RIVER");
                GenerateRiver(new Tuple<HexType, int>(chosenNeighborHexType, (chosenBranchIndex + 3) % 6), riverLength - 1);
            }
        }
    }


    //FIX : debug the flowIndex
    //FIX : use the LandPointPlacement as a guide for where to place river sources
    //NOTE: HashSet is used to avoid duplicates
    private HashSet<Tuple<HexType, int>> RandomRiverSources()
    {
        HashSet<Tuple<HexType, int>> riverSources = new HashSet<Tuple<HexType, int>>();
        
        int sourceTemp = maxRiverSourceCount;
        int attempts = 0;

        // Define a margin to avoid testing for land too close to the top or bottom
        float margin = mapSize.y * VerticalOffsetFactor * 0.15f; // For example, 20% of the map height
        
        int direction = 1;
        while (sourceTemp > 0 && attempts < maxRiverSourceCount * 2)
        {
            attempts += 1;

            // Generate random x-coordinate as before
            float randomX = Mathf.Round(UnityEngine.Random.value * (mapSize.x * HorizontalOffsetFactor - 1));

            // Generate random y-coordinate within the central area of the map
            float randomY = Mathf.Round(UnityEngine.Random.value * (mapSize.y * VerticalOffsetFactor - 2 * margin) + margin - 1);
            //Debug.Log("Randomly generated coords: " + randomX + ", " + randomY + "");

            Vector2 pos = new Vector2(randomX, randomY);
            //Debug.Log("Randomly generated coords: " + pos.ToString());

            pos = GetHexPosition(pos);
            //Debug.Log("This is the generated position of the hex: " + pos.ToString());

            RaycastHit2D hit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
            if (hit)
            {
                HexType newHex = hit.collider.gameObject.GetComponent<HexType>();
                
                //test if its land
                if (newHex.GetHexBiome() > 1)
                {
                    //go right or left until you find a water hex
                    
                    

                    while (pos.x > 0 && pos.x < mapSize.x * HorizontalOffsetFactor)
                    {
                        pos.x += direction * HorizontalOffsetFactor;
                        RaycastHit2D neighborHit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
                        if (neighborHit)
                        {
                            //test if its water
                            HexType neighborHex = neighborHit.collider.gameObject.GetComponent<HexType>();
                            if (neighborHex.GetHexBiome() < 2)
                            {
                                //if it is, assign newHex to be the river source
                                pos.x += -direction * HorizontalOffsetFactor; //move back to the source hex
                                //get the HexType at this position
                                hit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
                                newHex = hit.collider.gameObject.GetComponent<HexType>();
                                
                                List<Vector2> neighbors = ToolBox.GetNeighbors(pos);
                                List<int> possibleBranchIndices = new List<int>();
                                for (int i = 0; i < neighbors.Count; i++)
                                {
                                    RaycastHit2D sourceNeighborHit = Physics2D.Raycast(neighbors[i], neighbors[i], 0, LayerMask.GetMask("Default"));
                                    if (sourceNeighborHit)
                                    {
                                        HexType testHex = sourceNeighborHit.collider.gameObject.GetComponent<HexType>();
                                        //test if the neighbor is water
                                        if(testHex.GetHexBiome() < 2)
                                        {
                                            //if it is, add the index to the possibleBranchIndices
                                            possibleBranchIndices.Add(i);
                                        }
                                    }
                                }
                                
                                //after all possibleBranchIndices are found, choose one at random
                                int flowSourceIndex = possibleBranchIndices[UnityEngine.Random.Range(0, possibleBranchIndices.Count)];
                                //add the river source to the list
                                
                                riverSources.Add(new Tuple<HexType, int>(newHex, flowSourceIndex));
                                
                                direction *= -1;
                                break;
                            }
                        }       
                    }
                    sourceTemp -= 1;
                }
            }
        }

        return riverSources;
    }


    private HashSet<Tuple<HexType, int>> LandSourceRiverSources()
    {
        HashSet<Tuple<HexType, int>> riverSources = new HashSet<Tuple<HexType, int>>();
        int direction = 1;
        foreach (HexType landSource in landSources)
        {
            Vector2 pos = landSource.transform.position;
            //go right or left until you find a water hex

            while (pos.x > 0 && pos.x < mapSize.x * HorizontalOffsetFactor)
            {
                pos.x += direction * HorizontalOffsetFactor;
                RaycastHit2D neighborHit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
                if (neighborHit)
                {
                    //test if neighborHex is water
                    HexType neighborHex = neighborHit.collider.gameObject.GetComponent<HexType>();
                    if (neighborHex.GetHexBiome() < 2)
                    {
                        //if it is, assign newHex to be the river source
                        pos.x += -direction * HorizontalOffsetFactor; //move back to the source hex
                        //get the HexType at this position
                        neighborHit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
                        HexType newRiverSource = neighborHit.collider.gameObject.GetComponent<HexType>();
                        
                        List<Vector2> neighbors = ToolBox.GetNeighbors(pos);
                        List<int> possibleBranchIndices = new List<int>();
                        for (int i = 0; i < neighbors.Count; i++)
                        {
                            RaycastHit2D sourceNeighborHit = Physics2D.Raycast(neighbors[i], neighbors[i], 0, LayerMask.GetMask("Default"));
                            if (sourceNeighborHit)
                            {
                                HexType testHex = sourceNeighborHit.collider.gameObject.GetComponent<HexType>();
                                //test if the neighbor is water
                                if(testHex.GetHexBiome() < 2)
                                {
                                    //if it is, add the index to the possibleBranchIndices
                                    possibleBranchIndices.Add(i);
                                }
                            }
                        }
                        
                        //after all possibleBranchIndices are found, choose one at random
                        int flowSourceIndex = possibleBranchIndices[UnityEngine.Random.Range(0, possibleBranchIndices.Count)];
                        //add the river source to the list
                        
                        riverSources.Add(new Tuple<HexType, int>(newRiverSource, flowSourceIndex));
                        direction *= -1;
                        break;
                    }
                }       
            }
        }

        return riverSources;
    }
    
}
