using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookManager : MonoBehaviour
{
    private GameObject[] hookList;
    public GameObject closestHook;

    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");

        // Layer 9 is the "Hook" layer
        if (hookList == null)
            hookList = FindObjectsByLayer(9);
    }

    // Update is called once per frame
    void Update()
    {
        GetClosestHook();
    }

    private void GetClosestHook()
    {
        float closestDistance = float.MaxValue;
        foreach(GameObject h in hookList)
        {
            h.GetComponent<Hook>().distaceFromPlayer = Vector2.Distance(player.transform.position, h.transform.position);

            if (h.GetComponent<Hook>().distaceFromPlayer < closestDistance &&
                h.GetComponent<Hook>().transform.position.y >= player.transform.position.y)
            {
                closestDistance = h.GetComponent<Hook>().distaceFromPlayer;
                closestHook = h;
            }
        }
    }

    // SOURCE: https://stackoverflow.com/questions/44456133/find-inactive-gameobject-by-name-tag-or-layer
    GameObject[] FindObjectsByLayer(int layer)
    {
        List<GameObject> validTransforms = new List<GameObject>();
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];

        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].gameObject.layer == layer)
            {
                validTransforms.Add(objs[i].gameObject);
            }
        }

        return validTransforms.ToArray();
    }
}
