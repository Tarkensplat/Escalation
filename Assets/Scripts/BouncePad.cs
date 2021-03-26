using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public GameObject player;

    public float bounceForce;
    public string direction;

    void OnCollisionEnter(Collision collision)
    {
        var rb = collision.rigidbody;
        rb.velocity = new Vector3(0, 0, 0);

        switch (direction)
        {
            case "L":
                player.GetComponent<PlayerMovement>().Jump(Vector3.left, bounceForce, false);
                // rb.AddForce(Vector3.left * bounceForce, ForceMode.Impulse);
                break;
            case "R":
                player.GetComponent<PlayerMovement>().Jump(Vector3.right, bounceForce, false);
                // rb.AddForce(Vector3.right * bounceForce, ForceMode.Impulse);
                break;
            case "LU":
                player.GetComponent<PlayerMovement>().Jump(new Vector3(-1, 1, 0), bounceForce, false);
                // rb.AddForce(new Vector3(-1, 1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "RU":
                player.GetComponent<PlayerMovement>().Jump(new Vector3(1, 1, 0), bounceForce, false);
                // rb.AddForce(new Vector3(1, 1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "D":
                player.GetComponent<PlayerMovement>().Jump(Vector3.down, bounceForce, false);
                // rb.AddForce(Vector3.down * bounceForce, ForceMode.Impulse);
                break;
            case "LD":
                player.GetComponent<PlayerMovement>().Jump(new Vector3(-1, -1, 0), bounceForce, false);
                // rb.AddForce(new Vector3(-1, -1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "RD":
                player.GetComponent<PlayerMovement>().Jump(new Vector3(1, -1, 0), bounceForce, false);
                // rb.AddForce(new Vector3(1, -1, 0) * bounceForce, ForceMode.Impulse);
                break;
            default:
                player.GetComponent<PlayerMovement>().Jump(Vector3.up, bounceForce, false);
                // rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
                break;
        }
    }
}
