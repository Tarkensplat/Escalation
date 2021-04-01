using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NotorietyManager : MonoBehaviour
{
    private static float notoriety;
    public static Text notorietyUI;

    public static float Notoriety
    {
        get
        {
            return notoriety;
        }
        set
        {
            //Since static classes don't have Start(), check if the text field has been retrieved yet, we can retrieve the UI the first time the game tries to modify it
            if (notorietyUI == null)
            {
                notorietyUI = GameObject.FindObjectOfType<Canvas>().GetComponentInChildren<Text>();
            }
            notoriety = value;
            notorietyUI.text = "Notoriety: " + notoriety;
        }
    }
}
