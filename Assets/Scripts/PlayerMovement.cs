using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public int maxJumps = 2;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;
    private int jumps = 0;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;

    // Wall Climb States
    private bool onGround;
    private bool onWall;
    private bool onRightWall;
    private bool onLeftWall;
    private int wallSide;

    // Collision Variables
    [Space]
    [Header("Collison")]
    public LayerMask Platforms;
    public float wallCollisionRadius = 0.25f;
    public float groundCollisionRadius = 0.15f;
    public Vector3 bottomOffset, topRightOffset, bottomRightOffset, topLeftOffset, bottomLeftOffset;
    private Color debugCollisionColor = Color.red;

    // Collision Variables
    [Space]
    [Header("Dash Camera Shake")]
    public float shakeDuration = 0.3f;
    public float shakeAmplitude = 1.2f;
    public float shakeFrequency = 2.0f;

    private float shakeElapsedTime = 0f;

    [Space]
    private bool groundTouch;
    private bool hasDashed;

    public int side = 1;

    CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;
    public float minX, maxX;

    ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        vcam = GameObject.FindGameObjectWithTag("Cinemachine Camera").GetComponent<CinemachineVirtualCamera>();
        virtualCameraNoise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        ps = GetComponent<ParticleSystem>();

        jumps = maxJumps;
    }

    // Update is called once per frame
    void Update()
    {
        // Set the current climbing state every frame
        CheckClimbState();

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);

        if (onWall && Input.GetButton("Fire3") && canMove)
        {
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetButtonUp("Fire3") || !onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }

        if (wallGrab && !isDashing)
        {
            rb.useGravity = false;
            if (x > .2f || x < -.2f)
                rb.velocity = new Vector3(rb.velocity.x, 0, 0);

            float speedModifier = y > 0 ? .5f : 1;

            rb.velocity = new Vector3(0, y * (speed * speedModifier), 0);
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
                WallSlide();
            }
        }

        if (!onWall || onGround)
            wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            if (onWall && !onGround)
            {
                WallJump();
            }
            else if(jumps > 0)
            {
                Jump(Vector2.up, false);
                jumps--;
            }
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed)
        {
            if (xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if (!onGround && groundTouch)
        {
            groundTouch = false;
        }

        if (wallGrab || wallSlide || !canMove)
            return;

        if (x > 0)
        {
            side = 1;
        }
        if (x < 0)
        {
            side = -1;
        }

        CameraShake();

        // Wrap around the map
        if (rb.position.x < minX)
        {
            Vector3 oldPos = rb.position;
            rb.position = new Vector3(maxX, transform.position.y, transform.position.z);
            Vector3 newPos = rb.position;
            vcam.OnTargetObjectWarped(transform, newPos - oldPos);
        }

        if (rb.position.x > maxX)
        {
            Vector3 oldPos = rb.position;
            rb.position = new Vector3(minX, transform.position.y, transform.position.z);
            Vector3 newPos = rb.position;
            vcam.OnTargetObjectWarped(transform, newPos - oldPos);
        }

    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        jumps = maxJumps;
    }

    private void Dash(float x, float y)
    {
        hasDashed = true;
        shakeElapsedTime = shakeDuration;

        rb.velocity = Vector3.zero;
        Vector3 dir = new Vector3(x, y, 0);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        ps.Play();
        rb.useGravity = false;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        ps.Stop();
        rb.useGravity = true;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (onGround)
            hasDashed = false;
    }

    private void CameraShake()
    {
        // Adapted from https://github.com/Lumidi/CameraShakeInCinemachine

        if (shakeElapsedTime > 0)
        {
            // Set Cinemachine Camera Noise parameters
            virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
            virtualCameraNoise.m_FrequencyGain = shakeFrequency;

            // Update Shake Timer
            shakeElapsedTime -= Time.deltaTime;
        }
        else
        {
            // If Camera Shake effect is over, reset variables
            virtualCameraNoise.m_AmplitudeGain = 0f;
            shakeElapsedTime = 0f;
        }
    }

    private void WallJump()
    {
        if ((side == 1 && onRightWall) || side == -1 && !onRightWall)
        {
            side *= -1;
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        if (!canMove)
            return;

        bool pushingWall = false;
        if ((rb.velocity.x > 0 && onRightWall) || (rb.velocity.x < 0 && onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            rb.velocity = new Vector3(dir.x * speed, rb.velocity.y, 0);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector3 dir, bool wall)
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, 0);
        rb.velocity += dir * jumpForce;
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void CheckClimbState()
    {
        onGround = Physics.OverlapSphere(transform.position + bottomOffset, groundCollisionRadius, Platforms).Length > 0;
        onWall = Physics.OverlapCapsule(transform.position + topRightOffset, transform.position + bottomRightOffset, wallCollisionRadius, Platforms).Length > 0
            || Physics.OverlapCapsule(transform.position + topLeftOffset, transform.position + bottomLeftOffset, wallCollisionRadius, Platforms).Length > 0;

        onRightWall = Physics.OverlapCapsule(transform.position + topRightOffset, transform.position + bottomRightOffset, wallCollisionRadius, Platforms).Length > 0;
        onLeftWall = Physics.OverlapCapsule(transform.position + topLeftOffset, transform.position + bottomLeftOffset, wallCollisionRadius, Platforms).Length > 0;

        wallSide = onRightWall ? -1 : 1;
    }

    // Draw red climbing gitboxes for reference
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset, topRightOffset, bottomRightOffset, topLeftOffset, bottomLeftOffset };

        Gizmos.DrawWireSphere(transform.position + bottomOffset, groundCollisionRadius);
        Gizmos.DrawWireSphere(transform.position + topRightOffset, wallCollisionRadius);
        Gizmos.DrawWireSphere(transform.position + bottomRightOffset, wallCollisionRadius);
        Gizmos.DrawWireSphere(transform.position + topLeftOffset, wallCollisionRadius);
        Gizmos.DrawWireSphere(transform.position + bottomLeftOffset, wallCollisionRadius);
    }
}