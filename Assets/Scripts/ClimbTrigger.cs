using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbTrigger : MonoBehaviour
{
    private Ratmovement ratMove;
    private WallClimbing wallClimbOne;
    private WallClimbing_2 wallClimbTwo;
    private LedgeClimbing ledgeClimbOne;
    private LedgeClimbing_2 ledgeClimbTwo;

    public bool wallClimbOneActive = false;
    public bool wallsClimbTwoActive = false;
    public bool ledgeClimbOneActive = false;
    public bool ledgeClimbTwoActive = false;

    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask ledgeMask;


    [Header("Debug")]
    public bool isTouchingLedge;

    public bool isLedgeClimbing = false;

    void Start()
    {
        ratMove = GetComponent<Ratmovement>();
        if (wallClimbOneActive)
        {
            wallClimbOne = GetComponent<WallClimbing>();
        }
        if (wallsClimbTwoActive)
        {
            wallClimbTwo = GetComponent<WallClimbing_2>();
        }
        if (ledgeClimbOneActive)
        {
            ledgeClimbOne = GetComponent<LedgeClimbing>();
        }
        if (ledgeClimbTwoActive) {
            ledgeClimbTwo = GetComponent<LedgeClimbing_2>();
        }
         
    }

    // Update is called once per frame
    void Update()
    {
            CheckLedgeContact();

            // Trigger climbing actions with E
            if (Input.GetKey(KeyCode.E))
            {
                if (isTouchingLedge && !isLedgeClimbing)
                {
                    triggerLedgeClimbing();
                }
            }

            // Drain stamina while sticking to ledge
            //if (isStickingToLedge)
            //{
            //    staminaController.Climbing();  // Drain stamina while stuck to the ledge
            //}

            //// Handle jumping off the ledge
            //if (isStickingToLedge && Input.GetKeyDown(KeyCode.Space))
            //{
            //    Debug.Log("Space pressed - attempting to jump off ledge.");
            //    JumpOffLedge();
            //}

            //// Handle air movement when not sticking to ledge
            //if (!isStickingToLedge && !isTouchingLedge && !ratMovement.moveState)
            //{
            //    HandleAirMovement();
            //}
    }

    void CheckLedgeContact()
    {
        isTouchingLedge = false;
        RaycastHit hit;

        // Check for ledge directly in front and above the rat
        Vector3 castStart = transform.position + Vector3.up * 1f; // Slightly offset above the rat

        // Perform SphereCast in front and upwards to detect ledge
        if (Physics.SphereCast(castStart, 0.5f, transform.forward, out hit, detectionRadius, ledgeMask))
        {
            isTouchingLedge = true;
          //  targetLedgePosition = hit.point;
            // Debugging ledge detection
            Debug.Log($"Ledge detected at position: {hit.point}");
        }
        else
        {
            // Check in an upward direction to ensure the ledge above is detected
            if (Physics.Raycast(castStart, Vector3.up, out hit, detectionRadius, ledgeMask))
            {
                isTouchingLedge = true;
            //    targetLedgePosition = hit.point;
                // Debugging ledge detection
                Debug.Log($"Ledge detected above at position: {hit.point}");
            }
        }
    }

    internal void triggerMovement()
    {
        ratMove.enabled = true;
        if (wallClimbOneActive)
        {
            wallClimbOne.enabled = false;
        }
        if (wallsClimbTwoActive)
        {
            wallClimbTwo.enabled = false;
        }
        if (ledgeClimbOneActive)
        {
            ledgeClimbOne.enabled = false;
        }
        if (ledgeClimbTwoActive)
        {
            ledgeClimbTwo.enabled = false;
        }
    }

    void triggerLedgeClimbing()
    {
 
        if (ledgeClimbOneActive)
        {
            ledgeClimbOne.enabled = true;
            ratMove.enabled = false;
        }
        if (ledgeClimbTwoActive)
        {
            ledgeClimbTwo.enabled = true;
            ratMove.enabled = false;
        }
    }


}