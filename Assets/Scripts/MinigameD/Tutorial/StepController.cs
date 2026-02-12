using UnityEngine;

public class StepController : MonoBehaviour
{
    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    public GameObject page4;

    public void ShowPage1() {
        MinigameD_AudioManager.Instance.playButton();

        page1.SetActive(true);
        page2.SetActive(false);
        page3.SetActive(false);
        page4.SetActive(false);
    }

    public void ShowPage2() {
        MinigameD_AudioManager.Instance.playButton();

        page1.SetActive(false);
        page2.SetActive(true);
        page3.SetActive(false);
        page4.SetActive(false);
    }

    public void ShowPage3(){
        MinigameD_AudioManager.Instance.playButton();

        page1.SetActive(false);
        page2.SetActive(false);
        page3.SetActive(true);
        page4.SetActive(false);
    }

    public void ShowPage4(){
        MinigameD_AudioManager.Instance.playButton();

        page1.SetActive(false);
        page2.SetActive(false);
        page3.SetActive(false);
        page4.SetActive(true);
    }
}