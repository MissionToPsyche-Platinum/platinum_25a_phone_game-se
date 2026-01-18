using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{

    

    private int difficulty = 0;
    private float time = 10f;
    private int stage = 0;
    private float stageTime = 10f;


    public string SceneName;
    [SerializeField] private Transform obj;
    [SerializeField] private SpawnBubble bubble;

    // Start is called before the first frame update
    void Start()
    {
        

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

    void setStage(int stage)
    {
        if (stage == 0)
        {

        }
        else if (stage == 1)
        {
            Vector3 vector = new Vector3(7.5f,7.5f,7.5f);
            bubble.setRadius(7.5f / 2);
            difficulty = 1;
            obj.transform.localScale = vector;

        }
        else if (stage == 2)
        {
            Vector3 vector = new Vector3(10f, 10f, 10f);
            bubble.setRadius(10f / 2);
            difficulty = 2;
            obj.transform.localScale = vector;
        }
        else if(stage == 3)
        {
            SceneManager.LoadScene(SceneName);
        }

    }
}
