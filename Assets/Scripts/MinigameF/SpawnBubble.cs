using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnBubble : MonoBehaviour    
{
    [SerializeField] private GameObject redCirc;
    [SerializeField] private GameObject blueCirc;
    [SerializeField] private GameObject greenCirc;
    

    private int max = 500000;
    private float radius = 2.5f;
    private int spawns = 0;
    private int spawnTimer = 250;
    private bool spawn = true;

    private bool collide = false;
    
    
    bool spawned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
        
    

    // Update is called once per frame
    // Spawns bubbles at a predetermined rate and at a random possition on a rotating sphere
    void Update()
    {
        if (spawn)
        {
            if (spawns >= max)
            {

            }
            else if (spawned)
            {
                if (spawnTimer > 0)
                {
                    spawnTimer--;
                }
                else
                {
                    spawnTimer = 100;
                    spawned = false;
                }
            }
            else
            {

                Vector3 vector = generateSpawn();

                spawnObj(vector);
                spawned = true;
                ;
            }
        }

    }   

    public float getRadius()
    {
        return this.radius;
    }

    public void setRadius(float radius)
    {
        this.radius = radius;
    }
    
    public void setSpawn(bool spawn)
    {
        this.spawn = spawn;
    }

    // Based off of a random number
    // Spawns a red bubble on the sphere object if its 0
    // Spawns a blue bubble if its 1
    // Spawns a green bubble if its 2
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
    
    public bool checkSpawnFull(Vector3 pos)
    {
        bool full = false;

        Collider[] colliders = Physics.OverlapSphere(pos, 500, 8);

        if(colliders.Length > 0)
        {
            full = true;
            Debug.Log("HEEEEELP");
        }
                
        return full;
    }

    public Vector3 generateSpawn()    {
        
        float y = UnityEngine.Random.Range(-2f, 2f);
        float z = Mathf.Sqrt(3 + (radius * radius) - (y * y));

        Vector3 vector = new Vector3(0, y, z);

        

        if (checkSpawnFull(vector))
        {
            generateSpawn();
        }


        return new Vector3(0, y, z);
    }
    

    
}
