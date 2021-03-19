using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckNotoriety : MonoBehaviour
{


    [Tooltip("The minimum notoriety value for this object to spawn.  Set to -1 to ignore")]
    public float spawnMin;

    [Tooltip("The maximum notoriety value for this object to spawn.  Set to -1 to ignore")]
    public float spawnMax;

    // Start is called before the first frame update
    void Start()
    {
        if ((spawnMin != -1 && spawnMin > NotorietyManager.Notoriety) || (spawnMax != -1 && spawnMax < NotorietyManager.Notoriety))
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
