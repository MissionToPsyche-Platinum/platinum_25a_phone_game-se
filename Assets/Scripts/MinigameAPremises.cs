using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MinigameAPremises
{
    public static Dictionary<string, string[]> premises = new Dictionary<string, string[]>{
        { "Psyche", new string[] { "Metal", "Asteroid" } },
        { "The Sun", new string[] { "Plasma", "Star" } },
        { "Jupiter", new string[] { "Gas", "Planet" } },
        { "Europa", new string[] { "Rock", "Moon" } },
        { "Tempel 1", new string[] { "Ice", "Comet" } },
        { "Sagittarius A", new string[] { "Singularity", "Black Hole" } }
    };
}
