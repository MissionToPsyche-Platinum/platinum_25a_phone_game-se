using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class obtacles : MonoBehaviour
{
    // Start is called before the first frame update
    public int diff = 0;

    [SerializeField] GameObject ob1;
    [SerializeField] GameObject ob2;
    [SerializeField] GameObject ob3;
    [SerializeField] GameObject ob4;
   

    void Start()
    {
        diff = setDiff.Instance.getDiff();
        // 0 = easy
        // 1 = normal
        // 2 = hard
        if (diff == 0) {
            
            Destroy(ob2);
            Destroy(ob3); 
            Destroy(ob4);
            
        

        }else if (diff == 1)
        {
            
            Destroy(ob2);
            
            Destroy(ob4);

        }
        else if (diff == 2)
        {
            
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        

    }    

    

}
