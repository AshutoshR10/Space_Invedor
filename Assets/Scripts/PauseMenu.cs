using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused;

    public GameObject pauseMenu;
    private GameManager gameManager;

    void Start()
    {
        pauseMenu.SetActive(false);
        gameManager = Object.FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found in the scene. Recording pause/resume will not work.");
        }
    }
    public void ReplayGame()
    {
        Time.timeScale = 1f;
        if (isPaused)
        {
            isPaused = false;
            gameManager.ResumeRecording();
        }
        gameManager.StopRecording();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        gameManager.ResumeRecording();
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
                //gameManager.ResumeRecording();
            }
            else
            {
                gameManager.PauseRecording();
                PauseGame();
            }
        }
    }
}
