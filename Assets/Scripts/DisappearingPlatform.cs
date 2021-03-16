using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float disappear;
    public float reappear;
    float timer;
    Collider c;
    Renderer r;

    // Start is called before the first frame update
    void Start()
    {
        c = GetComponent<Collider>();
        r = GetComponent<Renderer>();
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        while (timer < disappear)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        r.enabled = false;
        c.enabled = false;
        while (timer < reappear)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        r.enabled = true;
        c.enabled = true;
        timer = 0;
    }
}
