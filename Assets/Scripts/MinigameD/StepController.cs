using UnityEngine;

public class StepController : MonoBehaviour
{
    private const string PREF_TUT_PART_KEY = "MinigameD-Tutorial-Part";
    private int tutorialPart = 1;

    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    public GameObject page4;

    public void Start()
    {
        tutorialPart = PlayerPrefs.GetInt(PREF_TUT_PART_KEY);
        if (tutorialPart == 1)
        {
            ShowPage1();
        } else if (tutorialPart == 2)
        {
            ShowPage4();
        }

        PlayerPrefs.DeleteKey(PREF_TUT_PART_KEY);
    }

    public void ShowPage1() {
        page1.SetActive(true);
        page2.SetActive(false);
        page3.SetActive(false);
        page4.SetActive(false);
    }

    public void ShowPage2() {
        MinigameD_AudioManager.Instance.buttonClick();

        page1.SetActive(false);
        page2.SetActive(true);
        page3.SetActive(false);
        page4.SetActive(false);
    }

    public void ShowPage3(){
        MinigameD_AudioManager.Instance.buttonClick();

        page1.SetActive(false);
        page2.SetActive(false);
        page3.SetActive(true);
        page4.SetActive(false);
    }

    public void ShowPage4(){
        page1.SetActive(false);
        page2.SetActive(false);
        page3.SetActive(false);
        page4.SetActive(true);
    }
}