using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{

    
    [SerializeField] pause asteroid;
    [SerializeField] GameObject endGame;
    [SerializeField] GameObject menu;
    
    [SerializeField] orbitPschye speed;

    //first stage horizontal 2nd stage obstacle 1 both 
    [SerializeField] moveVert obstacle1;
    [SerializeField] moveVert obstacle2;
    //first stage vertical 2nd stage obsacle 3 both
    [SerializeField] moveHorz obstacle3;
    [SerializeField] moveHorz obstacle4;

    public int difficulty = 0;
    
    private float time = 60f;
    private int stage = 0;
    private float stageTime = 60f;

    
    [SerializeField] private Transform obj;
    [SerializeField] private TMP_Text timer;
    



     

    // Start is called before the first frame update
    void Start()
    {
        difficulty = setDiff.Instance.getDiff();
        Debug.Log(difficulty);
        timer.text = stageTime + "";

    }

    // Update is called once per frame
    void Update()
    {
        stageTime -= Time.deltaTime;
        timer.text = (int) stageTime + "";

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
            
            StartCoroutine(scaleOverTime(7.5f, 1.25f));           
            
            speed.setSpeed(-0.75f);
            obstacle1.enabled = true;
            obstacle3.enabled = true;
            

        }
        else if (stage == 2)
        {

            StartCoroutine(scaleOverTime(7.5f, 1.40f));
            
            
            speed.setSpeed(-1f);
            obstacle2.enabled = true;
            obstacle4.enabled = true;
            


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
        stageTime = 0;

        
        while (elapsed < duration)
        {
            var t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;

            yield return null;
        }
        Debug.Log("Check");

        
        transform.localScale = endScale;
        stageTime = 60;
        

    }
    
}


