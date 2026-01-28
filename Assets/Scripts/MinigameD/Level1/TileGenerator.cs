using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject tilePrefab; 
    public int numberOfTiles = 5; 
    public float tileHeight; 

    void Start()
    {
        SpriteRenderer spriteRenderer = tilePrefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // read the sprite's height in world units
            tileHeight = spriteRenderer.bounds.size.y;
        }
        else
        {
            Debug.LogError("Tile prefab does not have a SpriteRenderer component!");
            return;
        }

        for (int i = 1; i <= numberOfTiles; i++)
        {
            Vector3 position = new Vector3(0, i * tileHeight, 0);
            Instantiate(tilePrefab, position, Quaternion.identity, this.transform);
        }
    }
}
