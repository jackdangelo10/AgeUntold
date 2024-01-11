using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorUI : MonoBehaviour
{

    public GameObject hexPrefab;
    private GameObject _hexPrefabClone;
    public TMP_Dropdown terrainDropdown;
    public TMP_Dropdown biomeDropdown;
    private readonly TMP_Dropdown.OptionDataList _terrainOptions = new TMP_Dropdown.OptionDataList();
    public Image biomePreview;
    public Image terrainPreview;

    private void OnBiomeDropdownChanged()
    {
        //primary functionality
        _hexPrefabClone.GetComponent<HexType>().SetHexBiome(biomeDropdown.value);

        // Change the terrain dropdown's value to the first option
        terrainDropdown.value = 0; // None

        //reset and change hexPrefab to the selected biome
        foreach (Transform child in _hexPrefabClone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //change the terrain dropdown's options to the selected biome's options
        _terrainOptions.options.Clear();
        _terrainOptions.options.Add(new TMP_Dropdown.OptionData("None"));
        switch (biomeDropdown.value)
        {
            case 2:     //Grasslands
                _terrainOptions.options.Add(new TMP_Dropdown.OptionData("Forest"));
                break;
            case 3:     //Plains
                _terrainOptions.options.Add(new TMP_Dropdown.OptionData("Hills"));
                break;
            case 4:     //Tundra
                _terrainOptions.options.Add(new TMP_Dropdown.OptionData("Forest"));
                break;

        }
        terrainDropdown.options = _terrainOptions.options;
        terrainDropdown.RefreshShownValue();
        
        
        biomePreview.sprite = _hexPrefabClone.GetComponent<SpriteRenderer>().sprite;
        if (_hexPrefabClone.gameObject.transform.childCount > 0)
        {
            Transform terrain = _hexPrefabClone.gameObject.transform.GetChild(0);
            terrainPreview.sprite = terrain.GetComponent<SpriteRenderer>().sprite;
            terrainPreview.color = new Color(terrainPreview.color.r, terrainPreview.color.g, terrainPreview.color.b, 1); //set back to visible
        }
        else
        {
            terrainPreview.color = new Color(terrainPreview.color.r, terrainPreview.color.g, terrainPreview.color.b, 0); //set back to invisible
        }
    }


    private void OnTerrainDropdownChanged()
    {
        Sprite sprite = null;
        switch (terrainDropdown.options[terrainDropdown.value].text)
        {
            case "None":     //None
                break;
            case "Forest":     //Forest
                sprite = HexSpriteManager.Instance.GetTerrainSprite(0);
                break;
            case "Hills":     //Hills
                sprite = HexSpriteManager.Instance.GetTerrainSprite(1);
                break;
        }

        if (sprite != null)
        {
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.parent = _hexPrefabClone.transform;
            terrain.transform.localPosition = new Vector3(0, 0, 0);
            terrain.transform.localScale = new Vector3(1, 1, 1);
            terrain.AddComponent<SpriteRenderer>().sprite = sprite;
        }

        biomePreview.sprite = _hexPrefabClone.GetComponent<SpriteRenderer>().sprite;
        if (_hexPrefabClone.gameObject.transform.childCount > 0)
        {
            Transform terrain = _hexPrefabClone.gameObject.transform.GetChild(0);
            terrainPreview.sprite = terrain.GetComponent<SpriteRenderer>().sprite;
            terrainPreview.color = new Color(terrainPreview.color.r, terrainPreview.color.g, terrainPreview.color.b, 1); //set back to visible
        }
        else
        {
            terrainPreview.color = new Color(terrainPreview.color.r, terrainPreview.color.g, terrainPreview.color.b, 0); //set back to invisible
        }
        //FIX: the children are invisible!
    }

    public GameObject GetHexPrefab()
    {
        return _hexPrefabClone;
    }



    void Start()
    {
        _hexPrefabClone = Instantiate(hexPrefab, transform); 
        _hexPrefabClone.GetComponent<HexType>().SetHexBiome(0); //Default to Deep Ocean
        
        if (terrainPreview != null)
            terrainPreview.color = new Color(terrainPreview.color.r, terrainPreview.color.g, terrainPreview.color.b, 0); //start as invisible
        
        // Add listener for the biome dropdown
        biomeDropdown.onValueChanged.AddListener(delegate {
            OnBiomeDropdownChanged();
        });

        terrainDropdown.onValueChanged.AddListener(delegate {
            OnTerrainDropdownChanged();
        });

        // Set default biome to Deep Ocean
        biomeDropdown.value = 0;
        //refresh
        biomeDropdown.RefreshShownValue();
        biomePreview.sprite = _hexPrefabClone.GetComponent<SpriteRenderer>().sprite;
    }
}
