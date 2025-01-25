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

    // INPUTS
    Vector2 input;
    bool jumpInput;
    bool dodgeInput;
    bool dashInput;
    // TODO: remove the dashing boolean variable and replace logic with animation
    bool dashing = false;

    // ROTATION VARIABLES
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

        if (!dodgeInput) {
            SetDodging(false);
        }

        // Jump if the player is grounded or if the player is in coyote time.
        if ((jumpInput && IsGrounded()) || (jumpInput && coyoteFrames > 0 && coyoteFrames < coyoteFramesAllowed)) {
            Jump();
        } else if (jumpInputCut && IsJumping()) {
            // If the player releases the jump button, we want to cut the jump.
            // This is done by setting the player velocity to 0.
            SetGravity(gravityValue * gravityMultiplier * 0.5f);
        } else if (!IsJumping() && dodgeInput && !IsDodging()) {
            Debug.Log("Setting dodging animation");
            SetToDodge();
        } else if (!dashing && dashInput && !IsDodging()) {
            // Dash only if the player is not dodging
            Dash();
        }

        // Fall according to gravity
        Fall();
        
        // if no input, do not rotate and set to idle if not in jumping state
        // *** 
        // currently set to 0.05 instead of 1. changing this fixed a bug where the player
        // was not moving diagonally and had choppy movement.
        // ***
        if (Mathf.Abs(input.x) < 0.05 && Mathf.Abs(input.y) < 0.05) {
            if (!IsJumping() && !IsDodging() && !dashing) {
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
            dodgeInput = gamepad.circleButton.wasPressedThisFrame;
            dashInput = gamepad.squareButton.wasPressedThisFrame;
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
        if (IsJumping() || IsDodging()) return;
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

    void Dash() {
        StartCoroutine(EnableDashing());
    }

    IEnumerator EnableDashing() {
        dashing = true;
        // Dash for 0.25 seconds
        float dashTime = 0.25f;
        // Save the time in which the dashing started
        float startTime = Time.time;
        // Save the original player speed
        float originalPlayerSpeed = playerSpeed;
        // Multiply the player speed to dash
        playerSpeed *= 4;
        while (Time.time < startTime + dashTime) {
            yield return null;
        }
        // // Now that we have dashed, cool down for 0.25 seconds
        // float coolDownTime = 0.25f;
        // // Reduce the player original speed by half during the cool down
        // playerSpeed = originalPlayerSpeed / 2;
        // // Cool down for some time
        // float coolDownStartTime = Time.time;
        // while (Time.time < coolDownStartTime + coolDownTime) {
        //     yield return null;
        // }
        // Revert the speed to the original speed
        playerSpeed = originalPlayerSpeed;
        dashing = false;
    }

    /// <summary>
    /// Set robot to use idle animation.
    /// </summary>
    void SetToIdle() {
        SetIdle(true);
        SetJumping(false);
        SetWalking(false);
        SetDodging(false);
    }

    /// <summary>
    /// Set robot to use walking animation.
    /// </summary>
    void SetToWalk() {
        SetJumping(false);
        SetIdle(false);
        SetWalking(true);
        SetDodging(false);
    }

    /// <summary>
    /// Set the player to jump.
    /// </summary>
    void SetToJump() {
        SetGrounded(false);
        SetWalking(false);
        SetIdle(false);
        SetJumping(true);
        SetDodging(false);
    }

    /// <summary>
    /// Set the player to grounded.
    /// </summary>
    void SetToGrounded() {
        SetVerticalVelocity(0);
        SetGrounded(true);
        SetJumping(false);
        SetDodging(false);
    }

    void SetToDodge() {
        SetGrounded(false); // we are not grounded if dodging
        SetWalking(false); // we are not walking
        SetIdle(false); // we are not idle
        SetJumping(false); // we are not jumping (ideally this is already false)
        
        // set the dodge animation
        SetDodging(true);
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
        Debug.Log("Setting jumping to " + jumping);
        animator.SetBool("isJumping", jumping);
    }

    /// <summary>
    /// Set the dodging animation.
    /// </summary>
    /// <param name="dodging">bool</param>
    void SetDodging(bool dodging) {
        Debug.Log("Set isDodging to " + dodging);
        animator.SetBool("isDodging", dodging);
    }

    bool IsDodging() {
        return animator.GetBool("isDodging");
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
