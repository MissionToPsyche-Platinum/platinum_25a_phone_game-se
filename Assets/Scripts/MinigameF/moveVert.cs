using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveVert : MonoBehaviour
{
    [SerializeField] Transform obj1;


    bool obj1direction = true;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (obj1.position.y >= 5)
        {
            obj1direction = false;
        }

        if (obj1.position.y <= -5)
        {
            obj1direction = true;
        }



        movehorz(obj1, obj1.position.x, obj1.position.y, obj1.position.z, obj1direction);


    }

    void movehorz(Transform tob, float x, float y, float z, bool dir)
    {


        float change = 0.1f;

        if (dir)
        {
            tob.position = new Vector3(x, y + change, z);

        }
        else if (!dir)
        {
            tob.position = new Vector3(x, y - change, z);

        }

    }
}
