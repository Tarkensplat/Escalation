using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject pause;
    public GameObject game;
    public GameObject start;
    public GameObject controls;

    public string nextScene;
    private bool showControls;

    // Start is called before the first frame update
    void Start()
    {
        showControls = false;
        DontDestroyOnLoad(gameObject);
        //Call OnSceneLoaded each time a new scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameStateManager.paused)
            {
                pause.SetActive(true);
                game.SetActive(false);
            }
            else
            {
                pause.SetActive(false);
                game.SetActive(true);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "Start")
        {
            start.SetActive(true);
        }
        else
        {
            start.SetActive(false);
        }
    }

    public void ChangeControls()
    {
        showControls = !showControls;
        if (showControls)
        {
            start.SetActive(false);
            controls.SetActive(true);
        }
        else
        {
            start.SetActive(true);
            controls.SetActive(false);
        }
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(nextScene);
        foreach (Transform child in controls.transform)
        {
            //disable the button for pause menu
            child.gameObject.SetActive(false);
        }
    }
}
