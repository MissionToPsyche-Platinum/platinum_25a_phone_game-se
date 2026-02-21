using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinigameBCoinScript : MonoBehaviour, IPointerDownHandler, IDropHandler, IPointerUpHandler
{
    [SerializeField] private int coinWeight;
    public int CoinWeight
    {
        get { return coinWeight; }
        set { coinWeight = value; }
    }
    private bool held;

    public GameObject targetArea;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.position = Input.mousePosition;
        OnPointerDown(null);
    }

    private void Update()
    {
        if (held)
        {
            rectTransform.position = Input.mousePosition;
        }
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        held = true;
    }

    public void OnDrop(PointerEventData pointerEventData)
    {
        held = false;
        Collider2D target = targetArea.GetComponent<Collider2D>();
        if (!target.OverlapPoint(this.transform.position))
        {
            Destroy(this.gameObject);
        }
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        held = false;
        Collider2D target = targetArea.GetComponent<Collider2D>();
        if (!target.OverlapPoint(this.transform.position))
        {
            Destroy(this.gameObject);
        }
    }
}
