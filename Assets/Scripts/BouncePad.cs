using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    private GameObject player;

    public float bounceForce;
    public string direction;
    public float disableTime;

    void Start()
    {
        player = GameObject.Find("Player");
    }

    void OnCollisionEnter(Collision collision)
    {
        var rb = collision.rigidbody;
        rb.velocity = new Vector3(0, 0, 0);

        // Reset jump count and dash
        player.GetComponent<PlayerMovement>().currentJumps =
            player.GetComponent<PlayerMovement>().maxJumps;

        player.GetComponent<PlayerMovement>().hasDashed = false;

        switch (direction)
        {
            case "L":
                player.GetComponent<PlayerMovement>().PointLaunch(Vector3.left, bounceForce, disableTime);
                // rb.AddForce(Vector3.left * bounceForce, ForceMode.Impulse);
                break;
            case "R":
                player.GetComponent<PlayerMovement>().PointLaunch(Vector3.right, bounceForce, disableTime);
                // rb.AddForce(Vector3.right * bounceForce, ForceMode.Impulse);
                break;
            case "LU":
                player.GetComponent<PlayerMovement>().PointLaunch(new Vector3(-1, 1, 0), bounceForce, disableTime);
                // rb.AddForce(new Vector3(-1, 1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "RU":
                player.GetComponent<PlayerMovement>().PointLaunch(new Vector3(1, 1, 0), bounceForce, disableTime);
                // rb.AddForce(new Vector3(1, 1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "D":
                player.GetComponent<PlayerMovement>().PointLaunch(Vector3.down, bounceForce, disableTime);
                // rb.AddForce(Vector3.down * bounceForce, ForceMode.Impulse);
                break;
            case "LD":
                player.GetComponent<PlayerMovement>().PointLaunch(new Vector3(-1, -1, 0), bounceForce, disableTime);
                // rb.AddForce(new Vector3(-1, -1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "RD":
                player.GetComponent<PlayerMovement>().PointLaunch(new Vector3(1, -1, 0), bounceForce, disableTime);
                // rb.AddForce(new Vector3(1, -1, 0) * bounceForce, ForceMode.Impulse);
                break;
            default:
                player.GetComponent<PlayerMovement>().PointLaunch(Vector3.up, bounceForce, disableTime);
                // rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
                break;
        }
    }
}
