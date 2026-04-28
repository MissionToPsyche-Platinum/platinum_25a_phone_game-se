using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;

    // Start is called before the first frame update
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindFirstObjectByType<InventoryController>();

        LoadGame();
        
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            saveData.playerPosition = player.transform.position;
        }

        if (inventoryController != null)
        {
            saveData.inventorySaveData = inventoryController.GetInventoryItems();
        }
        else
        {
            saveData.inventorySaveData = new List<InventorySaveData>();
        }

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
            }

            if (inventoryController != null)
            {
                inventoryController.SetInventoryItems(saveData.inventorySaveData);
            }
        }
        else
        {
            // No save file yet - create empty slots then save initial state.
            if (inventoryController != null)
                inventoryController.SetInventoryItems(new List<InventorySaveData>());
            SaveGame();
        }
    }
}
