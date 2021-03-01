using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float maxSpeed = 7.0f;
    public float jumpSpeed = 500.0f;
    public float counterJump = 100.0f;
    float xVelocity = 0.0f;
    float yVelocity = 0.0f;
    public float minX, maxX;
    int jumps = 2;
    bool isJumping = false;
    Rigidbody rb;
    CinemachineVirtualCamera vcam;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        vcam = GameObject.FindGameObjectWithTag("Cinemachine Camera").GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        xVelocity = 0.0f;
        yVelocity = rb.velocity.y;
        //side to side movement
        if (Input.GetKey(KeyCode.A))
        {
            xVelocity = -1.0f * moveSpeed;
            if (rb.position.x < minX)
            {
                Vector3 oldPos = rb.position;
                rb.position = new Vector3(maxX, transform.position.y, transform.position.z);
                Vector3 newPos = rb.position;
                vcam.OnTargetObjectWarped(transform, newPos - oldPos);
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            xVelocity = moveSpeed;
            if (rb.position.x > maxX)
            {
                Vector3 oldPos = rb.position;
                rb.position = new Vector3(minX, transform.position.y, transform.position.z);
                Vector3 newPos = rb.position;
                vcam.OnTargetObjectWarped(transform, newPos - oldPos);
            }
        }

        //jump
        if (jumps > 0)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                isJumping = true;
                yVelocity = jumpSpeed;
                jumps--;
            }
        }

        //reduce the jump force if the player lets go early
        if(isJumping && Input.GetKeyUp(KeyCode.W) && rb.velocity.y > 2)
        {
            yVelocity -= counterJump;
        }

        //set player velocity
        rb.velocity = new Vector3(xVelocity, yVelocity, rb.velocity.z);

    }

    void OnCollisionEnter(Collision collision)
    {
        var normal = collision.contacts[0].normal;
        //reset jumps on landing if needed
        if (jumps != 2)
        {
            //check to see if the bottom or side of the player is colliding
            if (normal.y > 0 || normal.x != 0)
            {
                isJumping = false;
                jumps = 2;
            }

        }
    }
}
