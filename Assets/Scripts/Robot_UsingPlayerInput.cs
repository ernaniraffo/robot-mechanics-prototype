using UnityEngine;
using UnityEngine.InputSystem;

public class Robot_UsingPlayerInput : MonoBehaviour
{
    #region COMPONENTS
    private CharacterController characterController;
    private Animator animator;
    #endregion

    #region PUBLIC VARIABLES
    public float jumpHeight;
    public float gravityFallingMultiplier;
    #endregion

    #region PRIVATE VARIABLES
    private Vector3 playerVelocity;
    private float gravityValue = Physics.gravity.y;
    private float coyoteFrames = 0;
    private float coyoteFramesAllowed = 10;
    private bool smallJump = false;
    private bool mediumJump = false;
    private float playerMoveSpeed = 5.0f;
    private float maxFallSpeed = 10f;
    private float rotationSpeed = 15.0f;
    #endregion
    

    #region INPUT VARIABLES
    private bool jumpInput;
    private bool jumpInputCut;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool dodgeInput;
    private bool dashInput;
    private bool sprintInput;
    #endregion

    #region INPUT FUNCTIONS
    /// <summary>
    /// Store the jump input as a boolean.
    /// </summary>
    /// <param name="context"></param>
    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.action.WasPressedThisFrame();
        jumpInputCut = context.action.WasReleasedThisFrame();
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
    #endregion

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
            gravityValue = Physics.gravity.y;
        }

        // Jump if there is jump input
        if (jumpInput && characterController.isGrounded) {
            Jump();
        } else if (jumpInputCut && IsJumping() && playerVelocity.y > 0) {
            gravityValue *= gravityFallingMultiplier * 0.5f;
            jumpInputCut = false;
        }
        // Calculate gravity
        HandleVerticalVelocity();
        // TODO: add special dodge or go backwards if dodging but no movement
        // TODO: add dashing forwards when idle
        // Move (includes rotating the player)
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
        Vector3 movementDirection = CalculateMovementDirection();
        // Rotate the player in the movement direction. x-axis is horizontal, z-axis is vertical.
        Rotate(movementDirection.x, movementDirection.z);
        // This is the last move call which will update the collision flags
        // It is recommended to only have one move method per frame since each call updates the
        // collision flags
        characterController.Move(movementDirection * Time.deltaTime);
        // Debug.Log("Movement direction: " + movementDirection + " ## isGrounded: " + characterController.isGrounded);
    }

    private Vector3 CalculateMovementDirection() {
        // Get the movement direction
        // Get the camera's forward and right vectors
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        // Create a movement direction vector from the camera's forward and right vector and the
        // player input
        Vector3 forward = moveInput.y * cameraForward;
        Vector3 right = moveInput.x * cameraRight;
        // Move the character in the movement direction with the player speed
        Vector3 movementDirection = (forward + right) * playerMoveSpeed;
        // The Y axis is reserved for the gravity
        movementDirection.y = playerVelocity.y;
        return movementDirection; 
    }

    private void Rotate(float horizontal, float vertical) {
        if (Mathf.Abs(horizontal) < 0.05 && Mathf.Abs(vertical) < 0.05) {
            // don't rotate the character if it is not moving
            // removing this would cause the character to reset it's rotation when movement stops.
            return;
        }
        // calculate the rotation
        float angle = Mathf.Rad2Deg * Mathf.Atan2(horizontal, vertical);
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        // set the rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
            Debug.Log("New gravity scale: " + gravityScale);
            // clamp the player velocity so our fall speed is consistent
            playerVelocity.y = Mathf.Max(playerVelocity.y, -maxFallSpeed);
        }
        // increase the player velocity by gravity scale to make player jump or fall
        playerVelocity.y += gravityScale * Time.deltaTime;
        Debug.Log("Player velocity: " + playerVelocity);

        if (IsFalling()) {
            coyoteFrames++;
        }
    }

    #region SETTERS
    private void SetJumping(bool jumping) {
        animator.SetBool("isJumping", jumping);
    }
    #endregion

    #region GETTERS
    private bool IsFalling() {
        return characterController.isGrounded && animator.GetBool("isJumping");
    }

    private bool IsJumping() {
        return animator.GetBool("isJumping");
    }
    #endregion
}
