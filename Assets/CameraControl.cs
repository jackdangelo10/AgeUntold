using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//NOTES: Camera adjustments that need to be made: 
/*
 * 1. Camera needs change centering rules depending on zoom level
 * ie. when zoomed out, camera should center on the middle of the map, when zoomed in, camera be able to center higher and lower on the map
 */


public class CameraControl : MonoBehaviour
{
    private const float HorizontalOffsetFactor = 0.768f;
    private const float VerticalOffsetFactor = .475f;
    public float speed = 13;
    private Camera _cam;
    public float height = 20; //set in MapMaker
    public float width = 20; //set in MapMaker

    bool mouseMove = false;
    Vector3 mousePos = Vector3.zero;
    Vector3 desiredPos = Vector3.zero;
    Vector3 oldMousePos = Vector3.zero;

    public BoxCollider2D barrierLeft;
    public BoxCollider2D barrierRight;
    private float moveDelay = 0;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        transform.position = new Vector3(GetComponent<MapMaker>().mapSize.x / 2 * HorizontalOffsetFactor, GetComponent<MapMaker>().mapSize.y / 2 * VerticalOffsetFactor, -10);
    }

    // Update is called once per frame
    void Update()
    {
        //Get player input data
        float horizontalInput=Input.GetAxisRaw("Horizontal");
        float verticalInput=Input.GetAxisRaw("Vertical");
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float minZoom = 2; // Minimum zoom level
        float maxZoom = ((height) / 2) + 1; // Maximum zoom level
        float damping = 0.2f; // Smaller values result in smoother movement for mouse camera jumps
        
        //check if mouse has moved and get desired position
        if(Input.mousePosition != oldMousePos)
        {
           oldMousePos = Input.mousePosition;
           desiredPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        }
        
        
        
        //if middle or right mouse button is pressed, move camera
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            mouseMove = true;
            mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        }
        //if middle or right mouse button is released, stop moving camera
        if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
        {
            mouseMove = false;
        }
        
        //get camera position
        Vector3 pos = transform.position;

        //if mouse is moving the camera, follow the mouse
        if (mouseMove)
        {
            Vector3 deltaPos = _cam.ScreenToWorldPoint(Input.mousePosition) - mousePos;
            pos -= damping * Time.deltaTime * speed * deltaPos; // Apply damping here
        }
        
        //if mouse is scrolling, zoom in or out, changing speed 
        if (scroll < 0 && _cam.orthographicSize < maxZoom)   // Change this value to limit zoom out
        {
            _cam.orthographicSize -= scroll * 5;
            speed -= scroll * 15;   // Increase this value to slow down more when zooming in
            Vector3 deltaPos = desiredPos - _cam.ScreenToWorldPoint(Input.mousePosition);
            pos += deltaPos;
        }
        else if (scroll > 0 && _cam.orthographicSize > minZoom)
        {
            _cam.orthographicSize -= scroll * 15;
            speed -= scroll * 10; // Decrease this value to speed up more when zooming out
            Vector3 deltaPos = desiredPos - _cam.ScreenToWorldPoint(Input.mousePosition);
            pos += deltaPos;
        }
        
        // Calculate Speed based on camera zoom level
        
        float zoomLevel = _cam.orthographicSize;
        speed = Mathf.Lerp(20f, 10f, (zoomLevel - minZoom) / (maxZoom - minZoom));
        speed = Mathf.Clamp(speed, 5f, 20f);
     
        if(horizontalInput != 0 || verticalInput != 0)
        {
            oldMousePos = new Vector3(-1, -1, -1); //impossible mouse position
            pos.x += horizontalInput * Time.deltaTime * speed;
            pos.y += verticalInput * Time.deltaTime * speed;
        }

        float minY = _cam.orthographicSize - 2;
        float maxY = Mathf.Lerp(height, height / 2, (zoomLevel - minZoom) / (maxZoom - minZoom));
        
        
        if(pos.y < minY)
        {
            pos.y = minY;
        }
        else if(pos.y > maxY)
        {
            pos.y = maxY;
        }
        
        if(transform.position != pos)
        {
            transform.position = pos;
            if (moveDelay <= 0)
            {
                MoveTiles();
            }
            
        }
        
        if(moveDelay > 0)
        {
            moveDelay -= Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        
    }

    
    void MoveTiles()
    {
        Collider2D[] targets = new Collider2D[1000];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Default"));

        int count = barrierLeft.OverlapCollider(filter, targets);
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = targets[i].transform.position;
            pos.x += width;
            targets[i].transform.position = pos;
        }
        
        targets = new Collider2D[1000];
        count = barrierRight.OverlapCollider(filter, targets);
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = targets[i].transform.position;
            pos.x -= width;
            targets[i].transform.position = pos;
        }
        
        moveDelay = 0.1f;
    }
    
}
