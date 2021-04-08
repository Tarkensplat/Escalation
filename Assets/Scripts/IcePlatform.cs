using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcePlatform : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player.GetComponent<PlayerMovement>().curIceVel = collision.rigidbody.velocity;
            player.GetComponent<PlayerMovement>().onIce = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player.GetComponent<PlayerMovement>().onIce = false;
        }
    }
}
