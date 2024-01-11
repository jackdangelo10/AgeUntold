using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapEditorUI : MonoBehaviour
{

    public GameObject hexPrefab;
    private GameObject hexPrefabClone;
    public TMP_Dropdown terrainDropdown;
    public TMP_Dropdown biomeDropdown;
    private readonly TMP_Dropdown.OptionDataList _terrainOptions = new TMP_Dropdown.OptionDataList();
    public Image preview;

    private void OnBiomeDropdownChanged()
    {
        //primary functionality
        hexPrefabClone.GetComponent<HexType>().SetHexBiome(biomeDropdown.value);

        // Change the terrain dropdown's value to the first option
        terrainDropdown.value = 0; // None

        //reset and change hexPrefab to the selected biome
        foreach (Transform child in hexPrefabClone.transform)
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
        preview.sprite = hexPrefabClone.GetComponent<SpriteRenderer>().sprite;
    }


    private void OnTerrainDropdownChanged()
    {
        Sprite sprite = null;
        switch (terrainDropdown.value)
        {
            case 0:     //None
                break;
            case 1:     //Forest
                sprite = HexSpriteManager.Instance.GetTerrainSprite(0);
                break;
            case 2:     //Hills
                sprite = HexSpriteManager.Instance.GetTerrainSprite(1);
                break;
        }

        if (sprite != null)
        {
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.parent = hexPrefabClone.transform;
            terrain.transform.localPosition = new Vector3(0, 0, 0);
            terrain.transform.localScale = new Vector3(1, 1, 1);
            terrain.AddComponent<SpriteRenderer>().sprite = sprite;
            terrain.AddComponent<SpriteRenderer>().sortingOrder = hexPrefabClone.GetComponent<SpriteRenderer>().sortingOrder + 1;
        }

        preview.sprite = hexPrefabClone.GetComponent<SpriteRenderer>().sprite;
        //FIX: the children are invisible!
    }

    public GameObject GetHexPrefab()
    {
        return hexPrefabClone;
    }



    void Start()
    {
        hexPrefabClone = Instantiate(hexPrefab, transform); 
        
        
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
        preview.sprite = hexPrefabClone.GetComponent<SpriteRenderer>().sprite;
    }
}
