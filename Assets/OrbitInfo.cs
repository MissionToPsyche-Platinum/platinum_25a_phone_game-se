using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class OrbitInfo : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] TMP_Text text;
    private bool filled = false;

    string[] eduInfo = new string[6];
    void Start()
    {
       


        eduInfo[0] = "The Orbit Phase of the Psyche Mission consists of 4 phases.";
        eduInfo[1] = "Phase A will orbit the asteroid for 56 days or approximately 41 orbits. During this phase the satellite will gather preliminary information about the asteroid to use in the later phases";
        eduInfo[2] = "Phase B will be split into two separate phases B1 and B2 both will study the topogrophy of the asteroid.";
        eduInfo[3] = "Phase B1 will orbit the asteroid for 92 days or approximately 190 orbits.";
        eduInfo[4] = "Phase B2 will orbit the asteroid for 100 days or approximately 206 orbits.";
        eduInfo[5] = "Phase C will orbit the asteroid for 100 days or approximately 333 orbits and will study the gravity of the asteroid.";
        eduInfo[6] = "Phase D will be the closest orbit of the asteroid and will last 100 days or 666 orbits around the asteroid. This Phase will study the elemental mapping of the asteroid.";


        

    }

    // Update is called once per frame
    void Update()
    {
        if (!filled)
        {
            int x = UnityEngine.Random.Range(0, 6);

            text.text = eduInfo[x];
            filled = true;
        }

    }
}
