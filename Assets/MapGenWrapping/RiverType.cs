using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverType : MonoBehaviour
{
    private int _flowSourceIndex = 0;
    private List<int> _sinkIndices = new List<int>();
    
    
    public void SetRiver(int flowSource, List<int> sinks)
    {
        _flowSourceIndex = flowSource;
        _sinkIndices = sinks;
        SwapRiverSprite();
    }
    
    private void SwapRiverSprite()
    {
        
        // Directly access the sprite from HexSpriteManager using biome index
        Sprite sprite = HexSpriteManager.Instance.GetRiverSprite(_flowSourceIndex, _sinkIndices);
        if (sprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }
    
    public int GetFlowSource()
    {
        return _flowSourceIndex;
    }
    
    public List<int> GetSinks()
    {
        return _sinkIndices;
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
