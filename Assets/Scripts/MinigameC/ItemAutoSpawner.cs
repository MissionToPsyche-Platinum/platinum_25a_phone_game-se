using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemAutoSpawner : MonoBehaviour
{
    private const string TargetSceneName = "MinigameC";

    [Header("Spawn Timing")]
    [SerializeField] private float spawnIntervalSeconds = 5f;
    [SerializeField] private int maxItemsInScene = 8;

    [Header("Spawn Area")]
    [SerializeField] private float minSpawnRadius = 3f;
    [SerializeField] private float maxSpawnRadius = 8f;
    [SerializeField] private float spawnCheckRadius = 0.4f;
    [SerializeField] private int spawnAttempts = 12;

    private float nextSpawnTime;
    private ItemDictionary itemDictionary;
    private GameObject player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureSpawner()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != TargetSceneName)
        {
            return;
        }

        if (FindFirstObjectByType<ItemAutoSpawner>() == null)
        {
            GameObject spawner = new GameObject("ItemAutoSpawner");
            spawner.AddComponent<ItemAutoSpawner>();
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != TargetSceneName)
        {
            Destroy(gameObject);
            return;
        }

        itemDictionary = FindFirstObjectByType<ItemDictionary>();
        player = GameObject.FindGameObjectWithTag("Player");
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (Time.time < nextSpawnTime)
        {
            return;
        }

        if (itemDictionary == null || itemDictionary.itemPrefabs == null || itemDictionary.itemPrefabs.Count == 0)
        {
            ScheduleNextSpawn();
            return;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            ScheduleNextSpawn();
            return;
        }

        int currentItemCount = GameObject.FindGameObjectsWithTag("Item").Length;
        if (currentItemCount >= maxItemsInScene)
        {
            ScheduleNextSpawn();
            return;
        }

        TrySpawnItem();
        ScheduleNextSpawn();
    }

    private void TrySpawnItem()
    {
        Vector2 playerPosition = player.transform.position;

        for (int i = 0; i < spawnAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector2 spawnPosition = playerPosition + offset;

            if (HasItemAtPosition(spawnPosition))
            {
                continue;
            }

            Item itemPrefab = itemDictionary.itemPrefabs[Random.Range(0, itemDictionary.itemPrefabs.Count)];
            if (itemPrefab == null)
            {
                continue;
            }

            GameObject spawnedItem = Instantiate(itemPrefab.gameObject, spawnPosition, Quaternion.identity);
            if (!spawnedItem.CompareTag("Item"))
            {
                spawnedItem.tag = "Item";
            }
            return;
        }
    }

    private bool HasItemAtPosition(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, spawnCheckRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag("Item"))
            {
                return true;
            }
        }

        return false;
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + spawnIntervalSeconds;
    }
}
