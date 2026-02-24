using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingGenerator : MonoBehaviour
{
    public GameObject boostPrefab; 
    public GameObject penaltyPrefab;
    public GameObject shieldPrefab;
    public GameObject jumpPrefab;

    public GameObject tilePrefab; 
    public int numberOfTiles = 5;

    public int difficulty = 1; // difficulty = level

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

            if (difficulty == 1)
            {
                if (i % 3 == 0) // spawn 1 penalty ring every 3 tiles
                {
                    spawnRing(tilePosition, penaltyPrefab);
                }

                for (int j = 0; j < 2; j++) // spawn 2 boost rings per tile
                {
                    spawnRing(tilePosition, boostPrefab);
                }
            } else if (difficulty == 2)
            {
                if (i % 2 == 0) // spawn 1 penalty ring every 2 tiles
                {
                    spawnRing(tilePosition, penaltyPrefab);
                }

                float randomChance = Random.value;
                if (randomChance <= 0.05f) // 5% chance of spawning a shield ring
                {
                    spawnRing(tilePosition, shieldPrefab);
                }

                if (randomChance <= 0.025f) // 2.5% chance of spawning a jump ring
                {
                    spawnRing(tilePosition, jumpPrefab);
                }

                // spawn 1 boost ring per tile
                spawnRing(tilePosition, boostPrefab);
            } else if (difficulty == 3)
            {
                // spawn 1 boost ring per tile
                spawnRing(tilePosition, boostPrefab);

                if (i % 2 == 0) // spawn 1 penalty ring every 2 tiles
                {
                    spawnRing(tilePosition, penaltyPrefab);
                }

                float randomChance = Random.value;
                if (randomChance <= 0.025f) // 2.5% chance of spawning a shield ring
                {
                    spawnRing(tilePosition, shieldPrefab);
                }

                if (randomChance <= 0.0125f) // 1.25% chance of spawning a jump ring
                {
                    spawnRing(tilePosition, jumpPrefab);
                }
            }
        }
    }

    private void spawnRing(Vector3 tilePosition, GameObject prefab)
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

        Instantiate(prefab, spawnPos, Quaternion.identity, this.transform);
    }
}
