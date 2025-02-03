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

    void Awake()
    {
        // Store the character controller component
        characterController = gameObject.GetComponent<CharacterController>();
        Debug.Log("Character controller: " + characterController);
        animator = gameObject.GetComponent<Animator>();
        // Store the animator component to enable/disable animations
    }

    void Update()
    {
        // Calculate gravity
        // Rotate character in direction of movement before moving
        // TODO: add special dodge or go backwards if dodging but no movement
        // TODO: add dashing forwards when idle
        // Move (includes dodging, dashing)
    }

    /// <summary>
    /// Store the jump input as a boolean.
    /// </summary>
    /// <param name="context"></param>
    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump action pressed");
        switch (context.phase)
        {
            case InputActionPhase.Started:
                // Set jump input to true if it has started
                Debug.Log("Setting jumpInput to true");
                jumpInput = true; // TODO: this may be redundant line
                animator.SetBool("isJumping", jumpInput);
                break;
            case InputActionPhase.Performed:
                // Check if the interaction has been released in the duration to be a tap interaction.
                // If it is, we will cut the jump.
                if (context.interaction is TapInteraction)
                {
                    jumpInputCut = true;
                    Debug.Log("Cutting jump input");
                }
                break;
            case InputActionPhase.Canceled:
                // The action has stopped
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

    /// <summary>
    /// Increase the player's Y-axis velocity and reset coyote frames if any.
    /// This method is called in the start of the jumping animation attached to the robot prefab.
    /// </summary>
    public void Jump() {
        Debug.Log("Start jumping!");
        // The jumping action corresponds to increasing the player's Y-axis velocity
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        // Reset the coyote frames since we have started jumping
        coyoteFrames = 0;
    }

    /// <summary>
    /// Reset the jumping animation.
    /// </summary>
    // public void ResetJump() {
    //     // Mark the animation
    //     SetJumpAnimation(true);
    // }

    // private void SetJumpAnimation(bool val) {
    //     Debug.Log("Jump animation is set to " + animator.GetBool("isJumping"));
    //     Debug.Log("Setting jump animation to " + val);
    //     animator.SetBool("isJumping", true);
    //     Debug.Log("Jump animation is set to " + animator.GetBool("isJumping"));
    // }
}
