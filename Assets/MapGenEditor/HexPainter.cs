using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    void Update()
    {
        if (Input.GetMouseButton(0)) // Check for left mouse click
        {
            Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, pos, 0, LayerMask.GetMask("Default"));
            if (hit)
            {
                GameObject hitObject = hit.collider.gameObject;
                GameObject newHex = Instantiate(_mapEditorUI.GetHexPrefab(), hitObject.transform.position, Quaternion.identity, hexes.transform);

                // Instantiate and add children of hexPrefab to the new hex
                foreach (Transform child in _mapEditorUI.GetHexPrefab().transform)
                {
                    Transform childTransform = child.transform;
                    Instantiate(child.gameObject, childTransform.position, childTransform.rotation, newHex.transform);
                }
                
                Destroy(hitObject); // Remove the original hex
            }
        }
    }
}