using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class Robot_UsingPlayerInput : MonoBehaviour
{
    // COMPONENTS
    private CharacterController characterController;
    private Animator animator;

    // PUBLIC VARIABLES
    public float jumpHeight;
    public float gravityFallingMultiplier;

    // PRIVATE VARIABLES
    private Vector3 playerVelocity;
    private float gravityValue = Physics.gravity.y;
    private float coyoteFrames = 0;
    private float coyoteFramesAllowed = 10;
    

    // INPUT VARIABLES
    private bool jumpInput;
    private bool jumpInputCut;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool dodgeInput;
    private bool dashInput;
    private bool sprintInput;

    /// <summary>
    /// Store the jump input as a boolean.
    /// </summary>
    /// <param name="context"></param>
    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                // Set jump input to true if it has started
                jumpInput = true;
                break;
            case InputActionPhase.Performed:
                // Check if the interaction has been released in the duration to be a tap interaction.
                // If it is, we will cut the jump.
                if (context.interaction is TapInteraction)
                {
                    jumpInputCut = true;
                }
                break;
        }
    }

    /// <summary>
    /// Store the move input as a vector.
    /// </summary>
    /// <param name="context"></param>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Store the look input as a vector.
    /// </summary>
    /// <param name="context"></param>
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Store the dodge input as a boolean.
    /// </summary>
    /// <param name="context"></param>
    public void OnDodge(InputAction.CallbackContext context)
    {
        dodgeInput = context.ReadValue<bool>();
    }

    /// <summary>
    /// Store the dash input as a boolean.
    /// </summary>
    /// <param name="context"></param>
    public void OnDash(InputAction.CallbackContext context)
    {
        dashInput = context.ReadValue<bool>();
    }

    /// <summary>
    /// Store the sprinting input as a boolean.
    /// </summary>
    /// <param name="context"></param>
    public void OnSprint(InputAction.CallbackContext context)
    {
        sprintInput = context.ReadValue<bool>();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (characterController.isGrounded) {
            SetJumping(false);
            playerVelocity.y = 0;
        }

        // Jump if there is jump input
        if (jumpInput && characterController.isGrounded) {
            Jump();
            jumpInput = false;
        }
        // Calculate gravity
        HandleVerticalVelocity();
        // Rotate character in direction of movement before moving
        // TODO: add special dodge or go backwards if dodging but no movement
        // TODO: add dashing forwards when idle
        // Move (includes dodging, dashing)
        Move();
    }

    /// <summary>
    /// Increase the player's Y-axis velocity and reset coyote frames if any.
    /// This method is called in the start of the jumping animation attached to the robot prefab.
    /// </summary>
    public void Jump() {
        Debug.Log("Start jumping!");
        // The jumping action corresponds to increasing the player's Y-axis velocity
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        Debug.Log("Calculated player's Y velocity = " + playerVelocity.y);
        // Reset the coyote frames since we have started jumping
        coyoteFrames = 0;
        // // Set the jumping animation
        SetJumping(true);
    }

    /// <summary>
    /// Caculate the movement direction and move the character.
    /// TODO: Move relative to the camera.
    /// </summary>
    private void Move() {
        // Get the movement direction
        Vector3 movementDirection = new Vector3(moveInput.x, playerVelocity.y, moveInput.y);
        // Move the character in the movement direction with the player speed
        // This last move call with update the collision flags
        // It is recommended to only have one move method per frame since each call updates the
        // collision flags
        characterController.Move(movementDirection * Time.deltaTime);
        Debug.Log("Movement direction: " + movementDirection + " ## isGrounded: " + characterController.isGrounded);
    }

    private void HandleVerticalVelocity() {
        // clamp player velocity
        if (characterController.isGrounded && playerVelocity.y < 0) {
            // Hitting this part of the code means we were jumping and are done.
            // Let's reset the jump animation
            Debug.Log("character is grounded now");
            // SetJumping(false);
            playerVelocity.y = 0f;
            coyoteFrames = 0;
            SetJumping(false);
        }
        float gravityScale = gravityValue;
        // player has reached peak of jump and is starting to fall
        if (playerVelocity.y < 0) {
            // multiply gravity scale by multiplier for faster fall
            gravityScale *= gravityFallingMultiplier;
        }
        // increase the player velocity by gravity scale to make player jump or fall
        playerVelocity.y += gravityScale * Time.deltaTime;
        // move the player down (0, falling velocity, 0)
        // characterController.Move(playerVelocity * Time.deltaTime);
        if (IsFalling()) {
            coyoteFrames++;
        }
    }

    private bool IsFalling() {
        return characterController.isGrounded && animator.GetBool("isJumping");
    }

    private void SetJumping(bool jumping) {
        animator.SetBool("isJumping", jumping);
    }
}
