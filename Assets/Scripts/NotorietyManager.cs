using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NotorietyManager
{
    private static float notoriety;

    public static float Notoriety
    {
        get
        {
            return notoriety;
        }
        set
        {
            notoriety = value;
        }
    }
}
