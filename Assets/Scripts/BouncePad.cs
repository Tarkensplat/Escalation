using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{

    public float bounceForce;
    public string direction;

    void OnCollisionEnter(Collision collision)
    {
        var rb = collision.rigidbody;
        rb.velocity = new Vector3(0, 0, 0);
        switch (direction)
        {
            case "L":
                rb.AddForce(Vector3.left * bounceForce, ForceMode.Impulse);
                break;
            case "R":
                rb.AddForce(Vector3.right * bounceForce, ForceMode.Impulse);
                break;
            case "LU":
                rb.AddForce(new Vector3(-1, 1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "RU":
                rb.AddForce(new Vector3(1, 1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "D":
                rb.AddForce(Vector3.down * bounceForce, ForceMode.Impulse);
                break;
            case "LD":
                rb.AddForce(new Vector3(-1, -1, 0) * bounceForce, ForceMode.Impulse);
                break;
            case "RD":
                rb.AddForce(new Vector3(1, -1, 0) * bounceForce, ForceMode.Impulse);
                break;
            default:
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
                break;
        }
    }
}
