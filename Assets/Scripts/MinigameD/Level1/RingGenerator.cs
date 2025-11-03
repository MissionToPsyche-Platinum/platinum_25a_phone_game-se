using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingGenerator : MonoBehaviour
{
    public GameObject boostPrefab; 
    public GameObject tilePrefab; 
    public int numberOfTiles = 5; 

    private float tileWidth;
    private float tileHeight;

    void Start()
    {
        SpriteRenderer sr = tilePrefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            tileWidth = sr.bounds.size.x;
            tileHeight = sr.bounds.size.y;
        }
        else
        {
            Debug.LogError("Tile prefab does not have a SpriteRenderer component!");
            return;
        }

        for (int i = 1; i <= numberOfTiles; i++)
        {
            Vector3 tilePosition = transform.position + new Vector3(0, i * tileHeight, 0);

            for (int j = 0; j < 2; j++) // spawn 2 objects per tile
            {
                // random x position
                float minX = tilePosition.x - tileWidth / 2f;
                float maxX = tilePosition.x + tileWidth / 2f;
                float randX = Random.Range(minX, maxX);

                // random y position
                float minY = tilePosition.y - tileHeight / 2f;
                float maxY = tilePosition.y + tileHeight / 2f;
                float randY = Random.Range(minY, maxY);

                Vector3 spawnPos = new Vector3(randX, randY, tilePosition.z);
                
                Instantiate(boostPrefab, spawnPos, Quaternion.identity, this.transform);
            }
        }
    }
}
