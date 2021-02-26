using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float maxSpeed = 10.0f;
    public float jumpSpeed = 500.0f;
    public float counterJump = 180.0f;
    int jumps = 2;
    bool isJumping = false;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //side to side movement
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(Vector2.left * moveSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(Vector2.right * moveSpeed);
        }

        //jump
        if (jumps > 0)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                isJumping = true;
                rb.AddForce(Vector3.up * jumpSpeed);
                jumps--;
            }
        }

        //reduce the jump force if the player lets go early
        if(isJumping && Input.GetKeyUp(KeyCode.W) && rb.velocity.y > 2)
        {
            rb.AddForce(Vector3.down * counterJump);
        }

        //clamp velocity
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var normal = collision.contacts[0].normal;
        //reset jumps on landing if needed
        if (jumps != 2)
        {
            Debug.Log("Normal" + normal);

            //check to see if the bottom or side of the player is colliding
            if (normal.y > 0 || normal.x != 0)
            {
                Debug.Log("Reset");
                isJumping = false;
                jumps = 2;
            }

        }
    }
}
