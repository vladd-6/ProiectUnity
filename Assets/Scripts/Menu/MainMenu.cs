using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private GameObject options;

    [SerializeField]
    private GameObject mainMenu;

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void GoToOptions()
    {
        mainMenu.SetActive(false);
        options.SetActive(true);
        
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void exitOptions()
    {
        options.SetActive(false);
        mainMenu.SetActive(true);
    }
}
