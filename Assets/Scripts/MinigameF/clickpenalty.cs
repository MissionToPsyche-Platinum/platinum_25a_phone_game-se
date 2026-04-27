using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickpenalty : MonoBehaviour
{

    [SerializeField] displayScore score;
    [SerializeField] ParticleSystem clickParticles;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        int penalty = -2;
        score.addScore(penalty);

        if (Time.timeScale == 1)
        {
            if (clickParticles != null)
            {
                clickParticles.transform.position = transform.position;
                clickParticles.Play();
                MinigameFAudioManager.Instance.playFail();
            }
                   


        
        }
    }
}
