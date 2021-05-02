using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public HookManager hookManager;

    private Animator animator;

    [Space]
    [Header("Stats")]
    private float currentSpeed = 10.0f;
    public float baseSpeed = 10.0f;
    public float jumpForce = 50.0f;
    public int maxJumps = 2;
    public float climbTime = 3.0f;
    public float climbTimer = 0.0f;
    public float slideSpeed = 5.0f;
    public float wallJumpLerp = 10.0f;
    public float wallJumpMultiplier = 1.0f;
    public float dashSpeed = 20.0f;
    public int currentJumps = 0;
    public float jumpInfluence = 5.0f;
    public float bounceInfluence = 2.0f;
    public float grappleInfluence = 3.0f;
    public float currentInfluence;
    public float iceLerp = 0.1f;
    public Vector3 curIceVel;
    public float iceMultiplier = 5.0f;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool forceApplied;
    public bool wallSlide;
    public bool isDashing;
    public bool onIce = false;

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
    private SpringJoint grapplingHook;
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public bool grappling;

    public float maxGrappleDistance = 10.0f;
    public float grappleSpeed = 5.0f;
    public float maxHookModifier = 0.8f;
    public float minHookModifier = 0.2f;
    public float hookSpring = 4.5f;
    public float hookDamper = 7f;
    public float grappleMassScale = 4.5f;
    public float grappleDisconnectForce = 50.0f;

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

    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;
    public float minX, maxX;

    ParticleSystem ps;

    AudioSource climbSound;
    AudioSource jumpSound;
    AudioSource grappleSound;
    AudioSource dashSound;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ps = GetComponent<ParticleSystem>();
        lr = GetComponent<LineRenderer>();

        animator = GetComponentInChildren<Animator>();

        Camera.main.layerCullSpherical = true;
        
        //Updates the notoriety UI on scene change
        NotorietyManager.Notoriety += 0;

        vcam = GameObject.FindGameObjectWithTag("Cinemachine Camera").GetComponent<CinemachineVirtualCamera>();
        virtualCameraNoise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        currentSpeed = baseSpeed;
        currentJumps = maxJumps;
        currentInfluence = jumpInfluence;

        climbTimer = climbTime;

        // Dash once to set drag parameters correctly
        Dash(0, 0);
        
        SoundManager sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        climbSound = sm.climbing;
        jumpSound = sm.jump;
        grappleSound = sm.grapple;
        dashSound = sm.dash;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameStateManager.paused)
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

                if (climbTimer > 0)
                    climbTimer -= Time.deltaTime;
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
                GetComponent<BetterJumping>().lowJumpMultiplier = currentInfluence;
            }

            if (wallGrab && !isDashing && climbTimer > 0)
            {
                rb.useGravity = false;
                if (x > .2f || x < -.2f)
                    rb.velocity = new Vector3(rb.velocity.x, 0, 0);

                float speedModifier = y > 0 ? .5f : 1;

                rb.velocity = new Vector3(0, y * (currentSpeed * speedModifier), 0);
                if (y > 0 && wallGrab)
                {
                    animator.SetBool("WallMove", true);
                }
                else
                {
                    animator.SetBool("WallMove", false);
                }

                if (rb.velocity.y > 0 && !climbSound.isPlaying)
                {
                    //play climbing sound
                    climbSound.Play();
                }
            }
            else
            {
                rb.useGravity = true;
                animator.SetBool("WallMove", false);
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
                    //jump sound
                    jumpSound.Play();
                    PointLaunch();
                }
                else if (currentJumps > 0)
                {
                    //jump sound
                    jumpSound.Play();
                    currentInfluence = jumpInfluence;
                    GetComponent<BetterJumping>().lowJumpMultiplier = currentInfluence;
                    Jump(Vector2.up, jumpForce);
                    currentJumps--;
                }
            }

            if (Input.GetButtonDown("Fire1") && !hasDashed)
            {
                if (xRaw != 0 || yRaw != 0)
                {
                    //dash sound
                    dashSound.Play();
                    Dash(xRaw, yRaw);

                    hasDashed = true;
                    shakeElapsedTime = shakeDuration;
                }
            }

            if (Input.GetButtonDown("Fire2") && !isDashing &&
                hookManager.closestHook.GetComponent<Hook>().distaceFromPlayer <= maxGrappleDistance)
            {
                //play grapple shot sound
                grappleSound.Play();
                grappling = true;
                forceApplied = true;
                currentInfluence = grappleInfluence;
                GetComponent<BetterJumping>().lowJumpMultiplier = currentInfluence;

                GrappleTo();
            }
            else if (!Input.GetButton("Fire2") && grappling)
            {
                grappling = false;

                ReleaseGrapple();
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

            if (groundTouch && currentJumps < maxJumps)
                currentJumps = maxJumps;

            if (grappling)
            {
                RotatePlayer();
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }

            // Elapse camera shake timer
            CameraShake();

            // Wrap around the map
            WrapMap();

            // Update Animation
            UpdateAnimation(x, y);

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
    }

    // Call RenderGrapple() in LateUpdate() so that it accurately tracks the player
    void LateUpdate()
    {
        RenderHook();
    }

    private void UpdateAnimation(float xAxis, float yAxis)
    {
        if (xAxis != 0 && onGround)
        {
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Run", false);
        }

        if(onWall && !onGround && !grappling)
        {
            animator.SetBool("Wall", true);
        }
        else
        {
            animator.SetBool("Wall", false);
        }

        if (side < 0)
        {
            transform.rotation = Quaternion.LookRotation(-transform.forward);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(transform.forward);
        }
    }

    private void GrappleTo()
    {
        grapplePoint = hookManager.closestHook.transform.position;

        grapplingHook = gameObject.AddComponent<SpringJoint>();
        grapplingHook.autoConfigureConnectedAnchor = false;
        grapplingHook.connectedAnchor = grapplePoint;

        float hookDistance = Vector3.Distance(transform.position, grapplePoint);

        grapplingHook.maxDistance = hookDistance * maxHookModifier;
        grapplingHook.minDistance = hookDistance * minHookModifier;

        // Spring characteristics of grapple point
        grapplingHook.spring = hookSpring;
        grapplingHook.damper = hookDamper;
        grapplingHook.massScale = grappleMassScale;

        currentSpeed = grappleSpeed;

        // 2 being the number of points that make up the hook line
        lr.positionCount = 2;

        currentJumps = maxJumps;
        forceApplied = true;
    }

    private void ReleaseGrapple()
    {
        currentSpeed = baseSpeed;

        lr.positionCount = 0;

        currentJumps = maxJumps;
        hasDashed = false;
        forceApplied = false;

        Destroy(grapplingHook);

        Jump(rb.velocity.normalized, grappleDisconnectForce);
    }

    private void RenderHook()
    {
        // Do not render line when not grappled
        if (!grapplingHook) return;

        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, transform.InverseTransformPoint(grapplePoint));
    }

    private void RotatePlayer()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = grapplePoint - transform.position;

        // Applies rotation to this object
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    void GroundTouch()
    {
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Land");
        animator.SetTrigger("Land");

        hasDashed = false;
        isDashing = false;

        climbTimer = climbTime;
        currentJumps = maxJumps;
    }

    private void Dash(float x, float y)
    {
        if(onGround && y <= 0)
        {
            animator.ResetTrigger("GroundDash");
            animator.SetTrigger("GroundDash");
        }
        else
        {
            animator.ResetTrigger("Dash");
            animator.SetTrigger("Dash");
        }

        rb.velocity = Vector3.zero;
        Vector3 dir = new Vector3(x, y, 0);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 1, .8f, RigidbodyDrag);

        GetComponent<BetterJumping>().enabled = false;

        ps.Play();
        rb.useGravity = false;
        forceApplied = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        ps.Stop();
        rb.useGravity = true;
        forceApplied = false;
        isDashing = false;

        GetComponent<BetterJumping>().enabled = true;
        GetComponent<BetterJumping>().lowJumpMultiplier = currentInfluence;

        animator.ResetTrigger("DashEnd");
        animator.SetTrigger("DashEnd");
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (onGround)
            hasDashed = false;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
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

    // Generalized form of PointLaunch() method
    // Used to apply forces to the player
    public void PointLaunch(Vector3 dir, float force, float timeDisabled)
    {
        currentInfluence = bounceInfluence;
        GetComponent<BetterJumping>().lowJumpMultiplier = currentInfluence;

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(timeDisabled));

        Jump(dir, force);

        forceApplied = true;
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

        if (onIce && !forceApplied)
        {
            rb.velocity = new Vector3(Mathf.Lerp(curIceVel.x, dir.x * currentSpeed, iceMultiplier * iceLerp * iceLerp * Time.deltaTime), rb.velocity.y, 0);
            curIceVel = rb.velocity;
            //adapted from https://answers.unity.com/questions/240557/icyslippery-floor.html
        }
        else if (!forceApplied)
        {
            rb.velocity = new Vector3(dir.x * currentSpeed, rb.velocity.y, 0);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, (new Vector2(dir.x * currentSpeed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector3 dir, float force)
    {
        animator.ResetTrigger("Jump");
        animator.SetTrigger("Jump");

        rb.velocity = new Vector3(rb.velocity.x, 0, 0);
        rb.velocity += dir * force;
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
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