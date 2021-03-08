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
    public float dashSpeed = 5.0f;
    public int dashReducer = 4;
    float dashTimer = 0.0f;
    float dashCooldown = 0.25f;
    bool dashAvailable = true;
    float xVelocity = 0.0f;
    float yVelocity = 0.0f;
    public float minX, maxX;
    int jumps = 2;
    bool isJumping = false;
    bool grounded = true;
    string direction = "";
    ParticleSystem ps;
    Rigidbody rb;
    CinemachineVirtualCamera vcam;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        vcam = GameObject.FindGameObjectWithTag("Cinemachine Camera").GetComponent<CinemachineVirtualCamera>();
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = "";
        xVelocity = 0.0f;
        yVelocity = rb.velocity.y;
        //side to side movement
        if (Input.GetKey(KeyCode.A))
        {
            direction = "A";
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
            direction = "D";
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

        if (Input.GetKey(KeyCode.W))
        {
            direction += "W";
        }

        //reduce the jump force if the player lets go early
        if (isJumping && Input.GetKeyUp(KeyCode.W) && rb.velocity.y > 2)
        {
            yVelocity -= counterJump;
        }

        //dash logic
        dashTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) && dashTimer <= 0.0f && dashAvailable)
        {
            var psShape = ps.shape;
            dashTimer = dashCooldown;
            switch(direction)
            {
                case "A":
                    psShape.rotation = new Vector3(0, 90, 0);
                    ps.Play();
                    rb.AddForce(Vector3.left * dashSpeed, ForceMode.Impulse);
                    break;
                case "D":
                    psShape.rotation = new Vector3(0, 270, 0);
                    ps.Play();
                    rb.AddForce(Vector3.right * dashSpeed, ForceMode.Impulse);
                    break;
                case "AW":
                    psShape.rotation = new Vector3(45, 90, 0);
                    ps.Play();
                    grounded = false;
                    rb.AddForce(new Vector3(-1, 1, 0) * (dashSpeed / dashReducer), ForceMode.Impulse);
                    break;
                case "DW":
                    psShape.rotation = new Vector3(45, 270, 0);
                    ps.Play();
                    grounded = false;
                    rb.AddForce(new Vector3(1, 1, 0) * (dashSpeed / dashReducer), ForceMode.Impulse);
                    break;
                default:
                    psShape.rotation = new Vector3(90, 90, 0);
                    ps.Play();
                    grounded = false;
                    rb.AddForce(Vector3.up * (dashSpeed / dashReducer), ForceMode.Impulse);
                    break;
            }
            if (!grounded)
            {
                dashAvailable = false;
            }
        }

        //set player velocity
        rb.velocity = new Vector3(xVelocity, yVelocity, rb.velocity.z);

    }

    void OnCollisionEnter(Collision collision)
    {
        var normal = collision.contacts[0].normal;
        if(normal.y > 0)
        {
            grounded = true;
            dashAvailable = true;
        }
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
