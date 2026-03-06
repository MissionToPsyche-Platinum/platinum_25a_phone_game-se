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

    public static List<string> premiseKeys = new List<string>(premises.Keys);

    private static List<string> cluesA = new List<string>()
    {
        "1 is primarily composed of 2.",
        "The main component of 1 is 2.",
        "1's composition is mainly 2.",
        "1 consists mostly of 2.",
        "2 makes up the majority of 1.",
        "1 is largely made up of 2.",
        "3 is not mostly made up of 4.",
        "3 contains very little 4.",
        "3 lacks significant amounts of 4.",
        "4 is not a major component of 3.",
    };

    private static List<string> cluesB = new List<string>()
    {
        "1 is classified as a(n) 2.",
        "1 falls under the category of a(n) 2.",
        "1 is considered a type of 2.",
        "Many people call 1 a(n) 2.",
        "1 is a 2.",
        "A good example of a(n) 2 is 1.",
        "3 is not classified as a(n) 4.",
        "3 does not fall under the category of a(n) 4.",
        "3 is not considered a type of 4.",
        "No one calls 3 a(n) 4."
    };

    public static List<string>[] clues = { cluesA, cluesB };
}
