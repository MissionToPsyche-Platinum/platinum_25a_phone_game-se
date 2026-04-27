using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class minigameFFontmanager : MonoBehaviour
{
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private Toggle fontToggle;

    [SerializeField] private bool defaultTutorial = true;
    [SerializeField] private bool defaultFont = false;

    private const string PREF_FONT_KEY = "AccessibleFont";
    private const string PREF_TUT_KEY = "TutorialsOn";


    private void Awake()
    {

        bool savedTutorial = PlayerPrefs.GetInt(PREF_TUT_KEY, defaultTutorial ? 1 : 0) == 1;
        if (tutorialToggle != null)
        {
            tutorialToggle.isOn = savedTutorial;
        }
        bool savedFont = PlayerPrefs.GetInt(PREF_FONT_KEY, defaultFont ? 1 : 0) == 1;

        

        if (fontToggle != null)
        {
            fontToggle.isOn = savedFont;
        }
    }    

    // Start is called before the first frame update
    public void toggleTutorials()
    {
        defaultTutorial = !defaultTutorial;
        PlayerPrefs.SetInt(PREF_TUT_KEY, defaultTutorial ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void toggleFont()
    {
        Debug.Log(defaultFont);
        defaultFont = !defaultFont;
        PlayerPrefs.SetInt(PREF_FONT_KEY, defaultFont ? 1 : 0);
        PlayerPrefs.Save();
    }
}
