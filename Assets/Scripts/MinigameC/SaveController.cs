using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cinemachine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    // Start is called before the first frame update
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

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

        CinemachineConfiner confiner = FindAnyObjectByType<CinemachineConfiner>();
        if (confiner != null && confiner.m_BoundingShape2D != null)
        {
            saveData.mapBoundary = confiner.m_BoundingShape2D.gameObject.name;
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

            CinemachineConfiner confiner = FindAnyObjectByType<CinemachineConfiner>();
            if (confiner != null && !string.IsNullOrEmpty(saveData.mapBoundary))
            {
                GameObject boundary = GameObject.Find(saveData.mapBoundary);
                if (boundary != null)
                {
                    confiner.m_BoundingShape2D = boundary.GetComponent<PolygonCollider2D>();
                }
            }
        } 
        else
        {
            SaveGame();
        }
    }
}
