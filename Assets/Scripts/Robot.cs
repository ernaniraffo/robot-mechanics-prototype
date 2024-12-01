using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;
    private float playerSpeed = 5.0f;
    public float jumpHeight;
    private float rotationSpeed = 15.0f;
    public float gravityMultiplier;
    float timeAtJump;
    float timeAtFall;
    float airTime = -1f;

    Vector2 input;
    bool jumpInput;
    private float angle;
    Quaternion targetRotation;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if (IsGrounded()) {
            SetToGrounded();
        }

        // Jump if needed and fall
        if (jumpInput && IsGrounded()) {
            Jump();
        }
        Fall();
        
        // if no input, do not rotate and set to idle if not in jumping state
        if (Mathf.Abs(input.x) < 1 && Mathf.Abs(input.y) < 1) {
            if (!IsJumping()) {
                SetToIdle();
            }
            return;
        }

        CalculateDirection();
        Rotate();
        Move();
    }

    /// <summary>
    /// Get the raw input from player. Value for axis is either -1, 0, 1.
    /// </summary>
    void GetInput() {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetButton("Jump");
    }

    /// <summary>
    /// Perform a jump.
    /// </summary>
    void Jump() {
        // Debug.Log("Performing a jump");
        airTime = -1f;
        // If we are performing a jump, we were grounded: we can set the player velocity directly
        // to which height we want to reach.
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        // timeAtJump = Time.time;
        
        // set jumping animations parameters
        SetToJump();
    }

    /// <summary>
    /// Move the player towards input direction.
    /// </summary>
    void Move() {
        Vector3 movementDirection = new Vector3(input.x, 0, input.y);
        controller.Move(movementDirection * playerSpeed * Time.deltaTime);
        if (IsJumping()) return;
        SetToWalk();
    }

    /// <summary>
    /// Check if the player is grounded.
    /// </summary>
    /// <returns>bool</returns>
    bool IsGrounded() {
        return controller.isGrounded;
    }

    /// <summary>
    /// Make the player fall downwards according to gravity. Set player velocity to 0. 
    /// </summary>
    void Fall() {
        groundedPlayer = IsGrounded();
        // clamp player velocity
        if (groundedPlayer && playerVelocity.y < 0) {
            playerVelocity.y = 0f;
        }
        // make the player fall according to gravity
        float gravityScale = gravityValue;
        // player has reached peak of jump and is starting to fall
        if (playerVelocity.y < 0) {
            // debug air time
            // if (airTime == -1f) {
            //     timeAtFall = Time.time;
            //     airTime = timeAtFall - timeAtJump;
            //     Debug.Log("Total air time to peak: " + airTime);
            // }
            // multiply gravity scale by multiplier for faster fall
            gravityScale *= gravityMultiplier;
        }
        // increase the player velocity by gravity scale and delta time to make player fall
        playerVelocity.y += gravityScale * Time.deltaTime;
        // move the player down (0, falling velocity, 0)
        controller.Move(playerVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Direction relative to the camera's rotation
    /// </summary>
    void CalculateDirection() {
        angle = Mathf.Atan2(input.x, input.y);
        angle = Mathf.Rad2Deg * angle;
    }

    /// <summary>
    /// Rotate towards the calculate angle
    /// </summary>
    void Rotate() {
        targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Set robot to use idle animation.
    /// </summary>
    void SetToIdle() {
        SetIdle(true);
        SetJumping(false);
        SetWalking(false);
    }

    /// <summary>
    /// Set robot to use walking animation.
    /// </summary>
    void SetToWalk() {
        SetJumping(false);
        SetIdle(false);
        SetWalking(true);
    }

    void SetToJump() {
        SetGrounded(false);
        SetWalking(false);
        SetIdle(false);
        SetJumping(true);
    }

    void SetToGrounded() {
        SetVerticalVelocity(0);
        SetGrounded(true);
        SetJumping(false);
    }

    bool IsJumping() {
        return animator.GetBool("isJumping");
    }

    void SetGrounded(bool grounded) {
        animator.SetBool("isGrounded", grounded);
    }

    void SetWalking(bool walking) {
        animator.SetBool("isWalking", walking);
    }

    void SetIdle(bool idle) {
        animator.SetBool("isIdle", idle);
    }

    void SetJumping(bool jumping) {
        animator.SetBool("isJumping", jumping);
    }

    void SetVerticalVelocity(float val) {
        playerVelocity.y = val;
    }
}
