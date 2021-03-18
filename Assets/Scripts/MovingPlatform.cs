using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float distance;
    public bool retracting;  //Check if you want the platform to go in and out.  Otherwise it will go left to right
    public float delay;

    float timer;
    Vector3 start;
    float depth;
    Collider c;

    // Start is called before the first frame update
    void Start()
    {
        c = GetComponent<Collider>();
        start = c.bounds.center;
        depth = c.bounds.max.z - c.bounds.min.z;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (retracting)
        {
            if (start.z > transform.position.z)
            {
                StartCoroutine(Delay(-moveSpeed));
                moveSpeed = 0;
                transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(start.z*10)/10);//Why isn't there a round override for precision?
            }
            else if (start.z + depth < transform.position.z)
            {
                StartCoroutine(Delay(-moveSpeed));
                moveSpeed = 0;
                transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round((start.z+depth)*10)/10);//Also this fixes a rounding error
            }
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + moveSpeed * Time.deltaTime);
        }
        else
        {
            if (start.x - distance > transform.position.x)
            {
                StartCoroutine(Delay(-moveSpeed));
                moveSpeed = 0;
                transform.position = new Vector3(Mathf.Round((start.x - distance) * 10) / 10, transform.position.y, transform.position.z);
            }
            else if (start.x + distance < transform.position.x)
            {
                StartCoroutine(Delay(-moveSpeed));
                moveSpeed = 0;
                transform.position = new Vector3(Mathf.Round((start.x + distance) * 10) / 10, transform.position.y, transform.position.z);
            }
            transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        //if (!retracting)
        //    collision.gameObject.GetComponent<PlayerController>().outsideForce = new Vector3(0, 0, moveSpeed * Time.deltaTime);
    }


    IEnumerator Delay(float newSpeed)
    {
        while (timer < delay)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        timer = 0;
        moveSpeed = newSpeed;
    }
}
