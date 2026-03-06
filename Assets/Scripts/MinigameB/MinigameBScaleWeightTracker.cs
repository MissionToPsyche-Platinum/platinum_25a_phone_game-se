using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameBScaleWeightTracker : MonoBehaviour
{
    [SerializeField] private AudioClipManager audioClipManager;

    private HashSet<Collider2D> colliders = new HashSet<Collider2D>();
    private int weight = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!colliders.Contains(collision))
        {
            colliders.Add(collision);
        }
        try
        {
            audioClipManager.PlayCoinDrop();
            MinigameBCoinScript coin = collision.GetComponent<MinigameBCoinScript>();
            if (coin != null)
            {
                weight += coin.CoinWeight;
            }
        }
        catch
        {
            Debug.Log("Not a coin.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (colliders.Contains(collision))
        {
            try
            {
                MinigameBCoinScript coin = collision.GetComponent<MinigameBCoinScript>();
                if (coin != null)
                {
                    weight -= coin.CoinWeight;
                }
            }
            catch
            {
                Debug.Log("Not a coin.");
            }
            colliders.Remove(collision);
        }
    }

    public int CalculateWeight()
    {
        return weight;
    }
}
