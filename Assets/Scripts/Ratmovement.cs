using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ratmovement : MonoBehaviour
{
    private Rigidbody rb; // Player rigidbody component
    private RigidbodyConstraints groundedConstraints; // Stores rigidbody constraints for when grounded in case we need to change them in the air.
    private Vector3 mousePos; // Position of mouse cursor in world environment

    [Header("Setup")]
    [Tooltip("How fast the rat runs")]
    public float moveSpeed = 20f;
    [Tooltip("Max speed the rat runs")]
    public float maxSpeed = 20f;
    [Tooltip("How HIGH the rat jumps")]
    public float jumpPower = 600f;
    [Tooltip("How fast the rat turns")]
    public float turnPower = 100f;
    [Tooltip("How FAR the rat jumps")]
    public float jumpForce = 16f;
    [Tooltip("How long after jumping before the Rat can re-enter grounded state")]
    public float jumpLockOutTime = 0.3f;

    [Tooltip("How hard the rat spins, pure style points")]
    public Vector3 spinForce = new Vector3(0, 0, 0);

    [Tooltip("If true, can freely rotate while jumping")]
    public bool canSpin = false;

    public enum jumpFreedom
    {
        Locked,
        SteerAllowed,
        SpeedControl,
        FreeMovement
    }

    [Tooltip("Controls how much freedom player has while jumping")]
    public jumpFreedom jumpStyle = jumpFreedom.Locked;

    [Tooltip("Iterated by number keys, sets movespeed and maxspeed for testing speed change")]
    public Vector2[] speedStates;

    [Header("Debug")]
    public bool moveState = true;
    public bool isJump = false;
    public float prevAngle = 0f;

    public float jumpLockOut = 0f; // How long before the player is allowed to land on an object when jumping, designed to prevent the player triggering ground state at the start of a jump.

    private WallClimbing wallClimbing;
    private WallClimbing_2 wallClimbing_2;
    private LedgeClimbing ledgeClimbing;
    private LedgeClimbing_2 ledgeClimbing_2;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get rat rigidbody
        groundedConstraints = rb.constraints;
        wallClimbing = GetComponent<WallClimbing>();
        wallClimbing_2 = GetComponent<WallClimbing_2>();
        ledgeClimbing = GetComponent<LedgeClimbing>();
        ledgeClimbing_2 = GetComponent<LedgeClimbing_2>();
    }

    // Update is called once per frame
    void Update()
    {
        // Prevent movement and rotation logic when climbing or ledge grabbing
        if (wallClimbing.isClimbing || wallClimbing_2.isClimbing || ledgeClimbing.isClimbing || ledgeClimbing_2.isStickingToLedge)
        {
            rb.freezeRotation = true; // Disable rotation when climbing
            return; // Exit Update if climbing
        }
        else
        {
            rb.freezeRotation = false; // Allow full rotation when not climbing
        }

        mousePos = Input.mousePosition; // Get mouse position from input
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;
        // Get the difference between the Mouse position and Rat position

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        // Get the angle to the mouse position

        AimRat(angle); // Rotate the rat to face the mouse pointer

        if (moveState || jumpStyle != jumpFreedom.Locked)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Space)) // JUMP INPUT
            {
                JumpRat();
            }

            // If Player is currently mid-jump with jump steering allowed, allow them to change the rat's direction by holding the forward key.
            if (isJump && jumpStyle == jumpFreedom.SteerAllowed && (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.W)))
            {
                rb.velocity = new Vector3(transform.forward.x * jumpForce, rb.velocity.y, transform.forward.z * jumpForce);
                // Maintain the XZ velocity to jumpForce
            }
        }

        // If Collision breaks, pressing X should force the player to re-enter grounded state
        if (Input.GetKeyDown(KeyCode.X))
        {
            EnterGrounded();
        }

        jumpLockOut -= Time.deltaTime;

        // Speed change testing
        if (Input.GetKeyDown(KeyCode.Alpha1)) { ChangeSpeed(0); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { ChangeSpeed(1); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { ChangeSpeed(2); }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { ChangeSpeed(3); }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) { ChangeSpeed(4); }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) { ChangeSpeed(4); }

        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -moveSpeed, moveSpeed), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -moveSpeed, moveSpeed));
        // Limit speed to the max of moveSpeed
    }

    void EnterGrounded()
    {
        if (jumpLockOut < 0f)
        {
            isJump = false;
            moveState = true;
            rb.constraints = groundedConstraints;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        EnterGrounded();
        // Enters grounded state on collision with anything
    }

    void FixedUpdate()
    {
        if (moveState || jumpStyle != jumpFreedom.Locked) // steer, speed, and free can pass
        {
            MoveRat();
        }
    }

    public void AimRat(float angle)
    {
        if (moveState || jumpStyle != jumpFreedom.SpeedControl) // steer and free can pass
        {
            Vector3 newDirection = Vector3.RotateTowards(transform.right, new Vector3(0, -angle, 0), turnPower * Time.deltaTime, 0.0f);
            float turnDist = Quaternion.Angle(Quaternion.Euler(new Vector3(0, -angle, 0)), Quaternion.Euler(new Vector3(0, -prevAngle, 0)));
            turnDist = Mathf.Clamp(turnDist, -turnPower, turnPower);
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, -angle, 0));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnPower);
            prevAngle = angle;
        }
    }

    public void MoveRat()
    {
        if (moveState || jumpStyle != jumpFreedom.SteerAllowed) // speed and free can pass
        {
            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.W))
            {
                rb.AddForce(transform.right * moveSpeed, ForceMode.Impulse);
                // Accelerate Rat.
            }
        }
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -maxSpeed, maxSpeed));
        // Limits speed to the max of maxSpeed
    }

    public void JumpRat()
    {
        moveState = false; // Player not grounded
        isJump = true; // Player is airborne (from a jump)
        jumpLockOut = jumpLockOutTime;

        if (!canSpin)
            rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotationZ;

        rb.velocity = new Vector3(transform.right.x * jumpForce, jumpPower, transform.right.z * jumpForce);
        // Apply force to make the rat jump, should feel fairly "set"
        rb.AddRelativeTorque(spinForce);
    }

    public void ChangeSpeed(int i)
    {
        if (speedStates[i] != null)
        {
            moveSpeed = speedStates[i].x;
            maxSpeed = speedStates[i].y;
        }
    }
}


/* Todo

Unlock rat rotation for easier ramp access (also makes the rat hop when flipped, explore this) (Rat resets rotation on landing, hopping is due to force carry over i think? nothing to actually change here) (DONE)
Lock rat rotation when jumping (make this an option, making the cube flip is funny) DONE
Create three different settings for jump controls (locked, steering allowed, free, etc) DONE
Investigate different force movement to allow different jump settings to have more options (air steering is currently useless) DONE

Try putting a placeholder model on
Setup some form of camera control
Give the rat a tail object?
Option for rat to auto slow down to stop exactly on mouse pointer, rather than always charging at it

*/