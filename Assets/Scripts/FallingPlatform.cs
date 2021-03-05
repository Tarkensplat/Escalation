using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float delay;
    float fallSpeed;
    float timer;
    Bounds b;

    // Start is called before the first frame update
    void Start()
    {
        b = GetComponent<Collider>().bounds;
        timer = 0;
        fallSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - fallSpeed*Time.deltaTime, transform.position.z);//Why isn't there a round override for precision?
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        while (timer < delay)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        fallSpeed = moveSpeed;
        timer = 0;
    }
}
