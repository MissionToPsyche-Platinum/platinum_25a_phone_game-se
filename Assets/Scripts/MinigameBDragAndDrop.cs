using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameBDragAndDrop : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject targetArea;
    [SerializeField] private GameObject parentCanvas;
    public void SpawnCoin()
    {
        GameObject coin = Instantiate(coinPrefab);
        coin.GetComponent<MinigameBCoinScript>().targetArea = targetArea;
        coin.transform.SetParent(parentCanvas.transform, false);
    }
}
