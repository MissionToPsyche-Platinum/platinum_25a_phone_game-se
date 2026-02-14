using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{

    [SerializeField] pause asteroid;
    [SerializeField] GameObject endGame;
    [SerializeField] GameObject menu;

    [SerializeField] spinDiff diff;
    [SerializeField] orbitPschye speed;
    [SerializeField] setDiff dif;

    public int difficulty = 0;
    
    private float time = 10f;
    private int stage = 0;
    private float stageTime = 30f;


    public string SceneName;
    [SerializeField] private Transform obj;
    [SerializeField] private SpawnBubble bubble;
    





    Vector3 scale = new Vector3(5,5,5);

    // Start is called before the first frame update
    void Start()
    {
        difficulty = dif.getDiff();
        Debug.Log(difficulty);

    }

    // Update is called once per frame
    void Update()
    {
        stageTime -= Time.deltaTime;
        //Debug.Log("timer = " + stageTime);

        if (stageTime <= 0f)        {
            
            stage++;
            setStage(stage);
            stageTime = time;
        }

    }

    public void setDifficulty(int difficulty)
    {
        this.difficulty = difficulty;
    }

    void setStage(int stage)
    {
        if (stage == 0)
        {

        }
        else if (stage == 1)
        {
            
            StartCoroutine(scaleOverTime(7.5f, 7.5f));
            
            diff.setStage(1);
            speed.setSpeed(-0.5f);
            

        }
        else if (stage == 2)
        {

            StartCoroutine(scaleOverTime(7.5f, 10f));
            
            diff.setStage(2);
            speed.setSpeed(-1f);
            


        }
        else if(stage == 3)
        {
            
            endGame.SetActive(true);
            menu.SetActive(false);

            
        }

    }

    IEnumerator scaleOverTime(float duration, float scale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * scale;
        float elapsed = 0f;

        
        while (elapsed < duration)
        {
            var t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;

            yield return null;
        }
        Debug.Log("Check");

        bubble.setRadius(((scale + 1) / 2) - 0.05f);
        transform.localScale = endScale;        
        

    }
    
}


