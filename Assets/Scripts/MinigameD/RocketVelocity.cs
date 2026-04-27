using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RocketVelocity : MonoBehaviour
{
    public float initialVelocity = 15f;
    public float initialGravity = -2.5f;

    private float currentVelocity;
    private float currentGravity;
    private bool ascending = true;
    private bool collisionEnabled = true;

    private bool shielded = false; // is the shield ring enabled
    private float shieldStartTime = 0f;
    private float shieldDuration = 5f; // effect lasts 5 seconds

    public GameObject shieldObject;
    private Image shieldIcon;

    public SpriteRenderer rocket;
    public Image miniRocket;

    private Coroutine fadeCoroutine; // track effects

    void Start()
    {
        currentVelocity = initialVelocity;
        currentGravity = initialGravity;
        if (shieldObject != null ) 
           shieldIcon = shieldObject.GetComponent<Image>();
    }

    private void Update()
    {
        // tracks duration for shield ring
        if (shielded)
        {
            if (Time.time - shieldStartTime >= shieldDuration)
            {
                shielded = false;
                shieldStartTime = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        // checks if jump ring is enabled
        if (Mathf.Abs(currentVelocity) > initialVelocity)
        {
            // disable collision
            collisionEnabled = false;

            // set gravity to one-third of initialVelocity, so that invulnerability ends after 3 seconds
            currentGravity = -(initialVelocity / 3f);
        } else
        {
            // re-enable collision once velocity is back to normal
            if (!collisionEnabled)
            {
                collisionEnabled = true;
                currentGravity = initialGravity; // reset to original gravity
            }
        }

        if (ascending)
        {
            currentVelocity += currentGravity * Time.deltaTime;
            transform.position += new Vector3(0, currentVelocity * Time.deltaTime, 0);

            // check if velocity has reached zero (peak)
            if (currentVelocity <= 0)
            {
                currentVelocity = 0;
                ascending = false;
            }
        }
        else
        {
            // falling down
            currentVelocity += currentGravity * Time.deltaTime;
            transform.position += new Vector3(0, currentVelocity * Time.deltaTime, 0);

            // check if velocity is positive again (ring interaction)
            if (currentVelocity > 0)
            {
                ascending = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!collisionEnabled)
            return; // ignore collisions when disabled

        if (other.CompareTag("BoostRing"))
        {
            boostVelocity();

            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("PenaltyRing"))
        {
            if (shielded)
                return; // ignore collisions if shield ring is enabled

            decreaseVelocity();

            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("JumpRing"))
        {
            doubleVelocity();

            other.gameObject.SetActive(false);
        } else if (other.CompareTag("ShieldRing"))
        {
            MinigameD_AudioManager.Instance.playShieldRing();

            shielded = true;
            shieldStartTime = Time.time;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(ShieldActivated());

            other.gameObject.SetActive(false);
        }
    }

    // boost ring: stabilize velocity
    public void boostVelocity()
    {
        MinigameD_AudioManager.Instance.playBoostRing();
        currentVelocity = initialVelocity;
    }

    // penalty ring: decrease velocity by 20%
    public void decreaseVelocity()
    {
        MinigameD_AudioManager.Instance.playPenaltyRing();
        currentVelocity = currentVelocity * (float)0.80;
    }

    // jump ring: double velocity + start invulnerability
    public void doubleVelocity()
    {
        MinigameD_AudioManager.Instance.playJumpRing();
        currentVelocity = initialVelocity * 2;
    }

    // handles visual for shield effect
    private IEnumerator ShieldActivated()
    {
        if (shieldObject != null)
            shieldObject.SetActive(true);
        if (rocket != null)
            rocket.color = Color.cyan;
        if (miniRocket != null)
            miniRocket.color = Color.cyan;

        if (shieldIcon != null)
        {
            Color color = shieldIcon.color;
            color.a = 1f;
            shieldIcon.color = color;
        }

        float flickerStartTime = shieldDuration - 1f;
        if (flickerStartTime > 0)
            yield return new WaitForSeconds(flickerStartTime);
        else
            yield return new WaitForSeconds(0f);

        float flickerDuration = 1f;
        float flickerInterval = 0.35f;
        float elapsed = 0f;

        while (elapsed < flickerDuration)
        {
            // flicker rocket & mini rocket color
            rocket.color = new Color(150f / 255f, 1f, 1f);
            miniRocket.color = new Color(150f / 255f, 1f, 1f);
            // flicker icon
            Color color = shieldIcon.color;
            color.a = 0.5f;
            shieldIcon.color = color;

            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;

            if (elapsed >= flickerDuration) break;

            rocket.color = Color.cyan;
            miniRocket.color = Color.cyan;
            color = shieldIcon.color;
            color.a = 1f;
            shieldIcon.color = color;

            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }

        rocket.color = Color.white;
        miniRocket.color = Color.white;
        shieldObject.SetActive(false);
        fadeCoroutine = null;
    }
}
