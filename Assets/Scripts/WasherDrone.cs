using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WasherDrone : MonoBehaviour
{
    public GameObject[] path;
    public float delayTime = 2.0f;
    int pathIndex;
    Rigidbody rb;
    GameObject nextPath;

    // Start is called before the first frame update
    void Start()
    {
        pathIndex = 0;
        rb = gameObject.GetComponent<Rigidbody>();
        nextPath = path[pathIndex];
        rb.velocity = nextPath.GetComponent<Path>().seekingVelocity;
    }


    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Invoke("RestartDrone", delayTime);
        }
    }

    void RestartDrone()
    {
        rb.velocity = nextPath.GetComponent<Path>().seekingVelocity;
    }

    void OnTriggerEnter(Collider collider)
    {
        //change direction when you collide with the path point
        if (collider.gameObject.tag == "SentryPath" && collider.gameObject.name == nextPath.name)
        {
            pathIndex++;
            if (pathIndex < path.Length)
            {
                nextPath = path[pathIndex];
                rb.velocity = nextPath.GetComponent<Path>().seekingVelocity;
            }
            else
            {
                pathIndex = 0;
                nextPath = path[pathIndex];
                rb.velocity = nextPath.GetComponent<Path>().seekingVelocity;
            }
        }
    }
}
