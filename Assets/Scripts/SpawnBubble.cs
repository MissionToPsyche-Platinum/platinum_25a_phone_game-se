using System;
using System.Collections;
using UnityEngine;

public class SpawnBubble : MonoBehaviour    
{
    [SerializeField] private GameObject redCirc;
    [SerializeField] private GameObject blueCirc;
    [SerializeField] private GameObject greenCirc;    
    private int max = 50;
    private int spawns = 0;
    private int spawnTimer = 50;
    
    Boolean spawned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
        
    

    // Update is called once per frame
    void FixedUpdate()
    {
        if (spawns >= max)
        {
            
        }
        else if (spawned)
        {
            if (spawnTimer > 0)
            {
                spawnTimer--;
            }else
            {
                spawnTimer = 20;
                spawned = false;
            }            
        }
        else
        {
            float x = UnityEngine.Random.Range(-2f, 2f);
            float y = UnityEngine.Random.Range(-2f, 2f);
            float z = Mathf.Sqrt(25f - (x * x) - (y * y));
           

            Vector3 vector = new Vector3(x, y, z);
            spawnObj(vector);

            spawned = true;
;
        }

    }

    

         

    // Spawns a red bubble on the sphere object
    public void spawnObj(Vector3 pos)
    {
        int x = UnityEngine.Random.Range(0, 3);        
        GameObject circ = new GameObject();     

        if (x == 0)
        {
            circ = Instantiate(redCirc, pos, transform.rotation);

        }
        else if (x == 1)
        {
            circ = Instantiate(blueCirc, pos, transform.rotation);

        }
        else if (x == 2)
        {
            circ = Instantiate(greenCirc, pos, transform.rotation);
        }


        circ.transform.parent = this.transform;        
        spawns++;
        spawned = true;
        
                
    }  
    

    
}
