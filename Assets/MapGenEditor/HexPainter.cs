using UnityEngine;
using System.Collections.Generic;

public class HexPainter : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject hexes; // Parent GameObject for the new hexes
    private MapEditorUI _mapEditorUI;
    
    // Start is called before the first frame update
    void Start()
    {
        _mapEditorUI = GetComponent<MapEditorUI>();
    }

    // Update is called once per frame
    //FIX : the goddamn hills are forests
    void Update()
    {
        if (Input.GetMouseButton(0)) // Check for left mouse click
        {
            Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));

            if (hit)
            {
                GameObject hitObject = hit.collider.gameObject;
                HexType hexType = hitObject.GetComponent<HexType>();

                if (hexType != null)
                {
                    // Set Hex Biome to the selected option in MapEditorUI
                    hexType.SetHexBiome(_mapEditorUI.biomeDropdown.value);
                    
                    // Remove existing terrain children
                    foreach (Transform child in hitObject.transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }

                    // Get the hex prefab from MapEditorUI
                    GameObject hexPrefab = _mapEditorUI.GetHexPrefab();

                    // Instantiate and add children (terrain) from the hexPrefab to the hitObject
                    foreach (Transform child in hexPrefab.transform)
                    {
                        GameObject childObject = Instantiate(child.gameObject, hitObject.transform);
                        Transform childTransform = childObject.transform;
                        childObject.transform.localPosition = childTransform.localPosition;
                        childObject.transform.localRotation = childTransform.localRotation;
                    }

                    // if land, swap neighbors that are deep ocean to shallow ocean
                    if (_mapEditorUI.biomeDropdown.value > 1)
                    {
                        List<Vector2> neighbors = HexIterator.GetNeighbors(hitObject.transform.position);
                        foreach (Vector2 neighbor in neighbors)
                        {
                            RaycastHit2D hitTest = Physics2D.Raycast(neighbor, neighbor, 0, LayerMask.GetMask("Default"));
                            if (hitTest)
                            {
                                HexType testHex = hitTest.collider.gameObject.GetComponent<HexType>();
                                //test if the hex is deep ocean
                                if(testHex.GetHexBiome() == 0)
                                {
                                    //if it is, set the neighbor hex to shallow water
                                    if (testHex != null)
                                    {
                                        testHex.SetHexBiome(1);
                                    }
                                }
                            }
                        }
                    }
                        
                }
            }
        }
    }

}