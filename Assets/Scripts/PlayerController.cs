using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    // Wall Climb States
    bool onGround;
    bool onWall;
    bool onRightWall;
    bool onLeftWall;
    int wallSide;

    public LayerMask Platforms;
    public float wallCollisionRadius = 0.25f;
    public float groundCollisionRadius = 0.15f;
    public Vector3 bottomOffset, topRightOffset, bottomRightOffset, topLeftOffset, bottomLeftOffset;
    private Color debugCollisionColor = Color.red;

    public float moveSpeed = 3.0f;
    public float maxSpeed = 7.0f;
    public float jumpSpeed = 500.0f;
    public float wallJumpSpeed = 50.0f;
    public float counterJump = 100.0f;
    public float dashSpeed = 5.0f;
    public int dashReducer = 4;
    public float slideSpeed = 5;
    float dashTimer = 0.0f;
    float dashCooldown = 0.25f;
    bool dashAvailable = true;
    float xVelocity = 0.0f;
    float yVelocity = 0.0f;
    public float minX, maxX;
    int jumps = 2;
    float jumpResetCooldown = 0.25f;
    float jumpResetTimer = 0.0f;
    bool isJumping = false;
    bool wallGrab;
    // bool wallJumped;
    bool wallSlide;
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
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        direction = "";
        yVelocity = rb.velocity.y;

        // Set the current climbing state every frame
        CheckClimbState();

        // Side to side movement
        if (Input.GetKey(KeyCode.A) && (!onLeftWall || onGround))
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
        else if (Input.GetKey(KeyCode.D) && (!onRightWall || onGround))
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
        else if (Input.GetKeyUp(KeyCode.D))
        {
            xVelocity = 0.0f;
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            xVelocity = 0.0f;
        }
        else
        {
            xVelocity = rb.velocity.x;
        }

        if (onWall && Input.GetKey(KeyCode.LeftShift))
        {
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || !onWall)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (wallGrab)
        {
            rb.useGravity = false;
            if (x > .2f || x < -.2f)
                rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;

            rb.velocity = new Vector2(0, y * (moveSpeed * speedModifier));
        }
        else
        {
            rb.useGravity = true;
        }

        if (onWall && !onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                dashAvailable = true;

                WallSlide();
            }
        }

        if (!onWall || onGround)
            wallSlide = false;

        //jump
        if (jumps > 0)
        {
            if (Input.GetKeyDown(KeyCode.W) && !onWall)
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
                    rb.AddForce(new Vector3(-1, 1, 0) * (dashSpeed / dashReducer), ForceMode.Impulse);
                    break;
                case "DW":
                    psShape.rotation = new Vector3(45, 270, 0);
                    ps.Play();
                    rb.AddForce(new Vector3(1, 1, 0) * (dashSpeed / dashReducer), ForceMode.Impulse);
                    break;
                default:
                    psShape.rotation = new Vector3(90, 90, 0);
                    ps.Play();
                    rb.AddForce(Vector3.up * (dashSpeed / dashReducer), ForceMode.Impulse);
                    break;
            }
            if (!onGround && !onWall)
            {
                dashAvailable = false;
            }
        }

        //set player velocity
        if(!wallGrab && !wallSlide)
            rb.velocity = new Vector3(xVelocity, yVelocity, rb.velocity.z);
    }

    void CheckClimbState()
    {
        onGround = Physics.OverlapSphere(transform.position + bottomOffset, groundCollisionRadius, Platforms).Length > 0;
        onWall = Physics.OverlapCapsule(transform.position + topRightOffset, transform.position + bottomRightOffset, wallCollisionRadius, Platforms).Length > 0
            || Physics.OverlapCapsule(transform.position + topLeftOffset, transform.position + bottomLeftOffset, wallCollisionRadius, Platforms).Length > 0;

        onRightWall = Physics.OverlapCapsule(transform.position + topRightOffset, transform.position + bottomRightOffset, wallCollisionRadius, Platforms).Length > 0;
        onLeftWall = Physics.OverlapCapsule(transform.position + topLeftOffset, transform.position + bottomLeftOffset, wallCollisionRadius, Platforms).Length > 0;

        wallSide = onRightWall ? -1 : 1;

        if(onGround && jumps != 2 && jumpResetTimer >= jumpResetCooldown)
        {
            Debug.Log("reset");

            dashAvailable = true;

            isJumping = false;
            jumps = 2;

            jumpResetTimer = 0;
        }
        else if(!onGround && !onWall)
        {
            jumpResetTimer += Time.deltaTime;
        }
    }

    private void WallSlide()
    {
        bool pushingWall = false;
        if ((rb.velocity.x > 0 && onRightWall) || (rb.velocity.x < 0 && onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : xVelocity;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    // Draw red climbing gitboxes for reference
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;

    //    var positions = new Vector2[] { bottomOffset, topRightOffset, bottomRightOffset, topLeftOffset, bottomLeftOffset };

    //    Gizmos.DrawWireSphere(transform.position + bottomOffset, groundCollisionRadius);
    //    Gizmos.DrawWireSphere(transform.position + topRightOffset, wallCollisionRadius);
    //    Gizmos.DrawWireSphere(transform.position + bottomRightOffset, wallCollisionRadius);
    //    Gizmos.DrawWireSphere(transform.position + topLeftOffset, wallCollisionRadius);
    //    Gizmos.DrawWireSphere(transform.position + bottomLeftOffset, wallCollisionRadius);
    //}
}
