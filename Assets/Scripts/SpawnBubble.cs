using System;
using System.Collections;
using UnityEngine;

public class SpawnBubble : MonoBehaviour    
{

    [SerializeField] private GameObject redCirc;
    private int max = 10;
    private int spawns = 0;
    float x = 5f;
    float y = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {    
        if (spawns >= max)
        {
            Debug.Log("Nice Try Buckaroo!");
        }
        else
        {            
            float x = UnityEngine.Random.Range(0f, 3f);
            float y = UnityEngine.Random.Range(0f, 3f);

            Vector2 vector = new Vector2(x - 1f, y - 1f);
            spawnObject(vector);
        }      
        
       
    }
      

    // Spawns a bubble on the sphere object
    public void spawnObject(Vector2 pos)
    {
        StartCoroutine(spawnWaiter());

        GameObject circ = Instantiate(redCirc, pos, transform.rotation);
        circ.transform.parent = this.transform;
        spawns++;        
        Debug.Log(spawns);        
    }

    // Makes spawn wait 
    public IEnumerator spawnWaiter()
    {
        yield return new WaitForSecondsRealtime(2);
    }
    

    
    

    
}
