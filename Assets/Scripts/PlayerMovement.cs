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
    public float wallJumpMultiplier = 1;
    public float dashSpeed = 20;
    public int currentJumps = 0;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool forceApplied;
    public bool wallSlide;
    public bool isDashing;

    [Space]
    [Header("Collison")]
    public LayerMask Platforms;
    public float wallCollisionRadius = 0.25f;
    public float groundCollisionRadius = 0.15f;
    public Vector3 bottomOffset, topRightOffset, bottomRightOffset, topLeftOffset, bottomLeftOffset;
    private Color debugCollisionColor = Color.red;

    [Space]
    [Header("Dash Camera Shake")]
    public float shakeDuration = 0.3f;
    public float shakeAmplitude = 1.2f;
    public float shakeFrequency = 2.0f;

    private float shakeElapsedTime = 0f;

    [Space]
    [Header("Hook")]
    public SpringJoint springJoint;
    public bool grappling;

    [Space]
    private bool groundTouch;
    public bool hasDashed;

    // Wall Climb States
    private bool onGround;
    private bool onWall;
    private bool onRightWall;
    private bool onLeftWall;
    private int wallSide;

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

        springJoint = GetComponent<SpringJoint>();

        currentJumps = maxJumps;
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
            forceApplied = false;
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
                PointLaunch();
            }
            else if(currentJumps > 0)
            {
                Jump(Vector2.up, jumpForce);
                currentJumps--;
            }
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed)
        {
            if (xRaw != 0 || yRaw != 0)
            {
                Dash(xRaw, yRaw);

                hasDashed = true;
                shakeElapsedTime = shakeDuration;
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            grappling = true;
            GrappleTo();
        }
        else if(Input.GetButton("Fire2"))
        {

        }
        else if(Input.GetButtonUp("Fire2"))
        {
            grappling = false;
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

        // Elapse camera shake timer
        CameraShake();

        // Wrap around the map
        WrapMap();

        // If in a special state or unable to move, do not update side information
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
    }

    void GrappleTo()
    {
        // springJoint = 
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        currentJumps = maxJumps;
    }

    private void Dash(float x, float y)
    {
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
        forceApplied = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        ps.Stop();
        rb.useGravity = true;
        GetComponent<BetterJumping>().enabled = true;
        forceApplied = false;
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

    // PointLaunch method for the standard wall jump
    private void PointLaunch()
    {
        if ((side == 1 && onRightWall) || side == -1 && !onRightWall)
        {
            side *= -1;
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up * wallJumpMultiplier + wallDir * wallJumpMultiplier), jumpForce);

        forceApplied = true;
    }

    // Generalized form of PointLaunch() method
    // Used to apply forces to the player
    public void PointLaunch(Vector3 dir, float force, float timeDisabled)
    {
        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(timeDisabled));

        Jump(dir, force);

        forceApplied = true;
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

        if (!forceApplied)
        {
            rb.velocity = new Vector3(dir.x * speed, rb.velocity.y, 0);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector3 dir, float force)
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, 0);
        rb.velocity += dir * force;
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

    private void WrapMap()
    {
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