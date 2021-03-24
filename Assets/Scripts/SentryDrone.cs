using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryDrone : MonoBehaviour

   
{
    public GameObject[] path;
    int pathIndex;
    Vector3 velocity;
    public bool patrolling = true;
    float zDestroy = -20.0f;
    public GameObject[] copies;
    float alertDelay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        pathIndex = 0;
        velocity = path[pathIndex].GetComponent<Path>().seekingVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        //patroll normally
        if (patrolling)
        {
            transform.Translate(velocity * Time.deltaTime);
        }
        //delay when spotted TODO: play alert animation
        else if(alertDelay > 0)
        {
            alertDelay -= Time.deltaTime;
        }
        //fly away after done spotting player
        else
        {
            transform.Translate(new Vector3(0, 10.0f, 3.0f) * Time.deltaTime);
            if(transform.position.z < zDestroy)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //change direction when you collide with the path point
        if(collision.gameObject.tag == "SentryPath")
        {
            pathIndex++;
            if(pathIndex < path.Length)
            {
                velocity = path[pathIndex].GetComponent<Path>().seekingVelocity;
            }
            else
            {
                pathIndex = 0;
                velocity = path[pathIndex].GetComponent<Path>().seekingVelocity;
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            //decrease notoriety by 5, don't go below 0
            NotorietyManager.Notoriety = NotorietyManager.Notoriety > 5 ? NotorietyManager.Notoriety - 5 : 0;
            for (int i = 0; i < copies.Length; i++)
            {
                copies[i].GetComponent<SentryDrone>().patrolling = false;
            }
            patrolling = false;
        }
    }
}
