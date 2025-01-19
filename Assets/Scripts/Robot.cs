using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Robot : MonoBehaviour
{
    CharacterController controller;
    private Vector3 playerVelocity;
    private float gravityValue = Physics.gravity.y;
    private float playerSpeed = 5.0f;
    public float jumpHeight;
    private float rotationSpeed = 15.0f;
    public float gravityMultiplier;

    Vector2 input;
    bool jumpInput;
    private float angle;
    Quaternion targetRotation;

    // Variable for coyote time
    private float coyoteFramesAllowed = 10;
    private float coyoteFrames = 0;

    private Animator animator;

    // Variables for jump input cut
    private bool jumpInputCut;

    // Testing controller input
    Gamepad gamepad;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Find a game controller
        var devices = InputSystem.devices;
        for (var i = 0; i < devices.Count; ++i)
        {
            var device = devices[i];
            if (device is Joystick || device is Gamepad)
            {
                gamepad = Gamepad.current;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if (IsGrounded()) {
            SetToGrounded();
            ResetGravity();
        }

        // Jump if the player is grounded or if the player is in coyote time.
        if ((jumpInput && IsGrounded()) || (jumpInput && coyoteFrames > 0 && coyoteFrames < coyoteFramesAllowed)) {
            Jump();
        } else if (jumpInputCut && IsJumping()) {
            // If the player releases the jump button, we want to cut the jump.
            // This is done by setting the player velocity to 0.
            SetGravity(gravityValue * gravityMultiplier * 0.5f);
        }
        // Fall according to gravity
        Fall();
        
        // if no input, do not rotate and set to idle if not in jumping state
        // *** 
        // currently set to 0.05 instead of 1. changing this fixed a bug where the player
        // was not moving diagonally and had choppy movement.
        // ***
        if (Mathf.Abs(input.x) < 0.05 && Mathf.Abs(input.y) < 0.05) {
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
        if (gamepad != null) {
            // Debug.Log("Left stick X axis input: " + gamepad.leftStick.x.ReadValue());
            // Debug.Log("Left stick Y axis input: " + gamepad.leftStick.y.ReadValue());
            input.x = gamepad.leftStick.x.ReadValue();
            input.y = gamepad.leftStick.y.ReadValue();
            jumpInput = gamepad.crossButton.isPressed;
            jumpInputCut = gamepad.crossButton.wasReleasedThisFrame;
        } else {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            jumpInput = Input.GetButton("Jump");
            jumpInputCut = Input.GetButtonUp("Jump");
        }
    }

    /// <summary>
    /// Perform a jump.
    /// </summary>
    void Jump() {
        // If we are performing a jump, we were grounded: we can set the player velocity directly
        // to which height we want to reach.
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        // set jumping animations parameters
        SetToJump();
        // reset coyote frames
        ResetCoyoteFrames();
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
        // clamp player velocity
        if (IsGrounded() && playerVelocity.y < 0) {
            playerVelocity.y = 0f;
            // f = 0
            ResetCoyoteFrames();
        }
        float gravityScale = gravityValue;
        // player has reached peak of jump and is starting to fall
        if (playerVelocity.y < 0) {
            // multiply gravity scale by multiplier for faster fall
            gravityScale *= gravityMultiplier;
        }
        // increase the player velocity by gravity scale and delta time to make player fall
        playerVelocity.y += gravityScale * Time.deltaTime;
        // move the player down (0, falling velocity, 0)
        controller.Move(playerVelocity * Time.deltaTime);

        if (IsFalling()) {
            coyoteFrames++;
        }
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

    /// <summary>
    /// Set the player to jump.
    /// </summary>
    void SetToJump() {
        SetGrounded(false);
        SetWalking(false);
        SetIdle(false);
        SetJumping(true);
    }

    /// <summary>
    /// Set the player to grounded.
    /// </summary>
    void SetToGrounded() {
        SetVerticalVelocity(0);
        SetGrounded(true);
        SetJumping(false);
    }

    /// <summary>
    /// Return true if the player is jumping.
    /// </summary>
    /// <returns>bool</returns>
    bool IsJumping() {
        return animator.GetBool("isJumping");
    }

    /// <summary>
    /// Set the grounded animation.
    /// </summary>
    /// <param name="grounded">bool</param>
    void SetGrounded(bool grounded) {
        animator.SetBool("isGrounded", grounded);
    }

    /// <summary>
    /// Set the walking animation.
    /// </summary>
    /// <param name="walking">bool</param>
    void SetWalking(bool walking) {
        animator.SetBool("isWalking", walking);
    }

    /// <summary>
    /// Return true if the player is walking.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsWalking() {
        return animator.GetBool("isWalking");
    }

    /// <summary>
    /// Set the idle animation.
    /// </summary>
    /// <param name="idle">bool</param>
    void SetIdle(bool idle) {
        animator.SetBool("isIdle", idle);
    }

    /// <summary>
    /// Set the jumping animation.
    /// </summary>
    /// <param name="jumping">bool</param>
    void SetJumping(bool jumping) {
        animator.SetBool("isJumping", jumping);
    }

    /// <summary>
    /// Set the vertical velocity of the player.
    /// </summary>
    /// <param name="val">float</param>
    void SetVerticalVelocity(float val) {
        playerVelocity.y = val;
    }

    /// <summary>
    /// Reset coyote frames.
    /// </summary>
    private void ResetCoyoteFrames() {
        coyoteFrames = 0;
    }
    
    /// <summary>
    /// Return true if the player is falling.
    /// The player is falling if he is not grounded and not jumping.
    /// </summary>
    /// <returns></returns>
    bool IsFalling() {
        return !IsGrounded() && !IsJumping();
    }

    /// <summary>
    /// Reset the gravity value to the default gravity value.   
    /// </summary>
    void ResetGravity() {
        gravityValue = Physics.gravity.y;
    }

    /// <summary>
    /// Set the gravity value.
    /// </summary>
    /// <param name="val">float</param> 
    void SetGravity(float val) {
        gravityValue = val;
    }
}
