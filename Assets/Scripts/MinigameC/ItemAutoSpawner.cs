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

    [Header("Spawn Bias")]
    [Tooltip("Chance (0-1) to spawn an item that is needed for the current step. Makes the right parts appear more often.")]
    [SerializeField] [Range(0f, 1f)] private float preferCurrentStepChance = 0.65f;

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

            Item itemPrefab = PickItemPrefab();
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

    private Item PickItemPrefab()
    {
        if (itemDictionary == null || itemDictionary.itemPrefabs == null || itemDictionary.itemPrefabs.Count == 0)
            return null;

        PhaseCAssemblyController controller = PhaseCAssemblyController.Instance;
        if (controller != null && Random.value < preferCurrentStepChance)
        {
            var requiredIds = controller.GetCurrentStepRequiredItemIds();
            if (requiredIds != null && requiredIds.Count > 0)
            {
                int id = requiredIds[Random.Range(0, requiredIds.Count)];
                int index = id - 1;
                if (index >= 0 && index < itemDictionary.itemPrefabs.Count)
                {
                    Item prefab = itemDictionary.itemPrefabs[index];
                    if (prefab != null) return prefab;
                }
            }
        }

        return itemDictionary.itemPrefabs[Random.Range(0, itemDictionary.itemPrefabs.Count)];
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
