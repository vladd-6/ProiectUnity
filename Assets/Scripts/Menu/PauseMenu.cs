using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField]
    private GameObject pauseMenu;

    // Update is called once per frame
    void Start()
    {
        Time.timeScale = 1;
    }
    void Update()
    {
        ReadInput();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        /*Cursor.lockState = CursorLockMode.Locked;*/
    }

    public void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Exit()
    {
        //TO DO: option to save game before quit
        Application.Quit();
        Cursor.lockState = CursorLockMode.Locked;
    }
}
