using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static bool paused;

    // Start is called before the first frame update
    void Start()
    {
        paused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseApplication();
        }
    }

    private void PauseApplication()
    {
        if (paused)
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
        }
        else
        {
            Time.timeScale = 0;
            AudioListener.pause = true;
        }
        paused = !paused;
    }
}
