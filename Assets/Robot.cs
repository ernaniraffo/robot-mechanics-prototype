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
    private float jumpHeight = 1.0f;
    private float rotationSpeed = 25f;

    Vector2 input;
    bool jumpInput;
    private float angle;
    Quaternion targetRotation;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        // Jump if needed and fall
        if (jumpInput && IsGrounded()) {
            Jump();
        }
        Fall();
        
        // if no input, do not rotate !
        if (Mathf.Abs(input.x) < 1 && Mathf.Abs(input.y) < 1) {
            Debug.Log("Stop rotating!");
            return;
        }
        // Debug.Log("Movement: " + input);

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
        Debug.Log("Performing a jump");
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
    }

    /// <summary>
    /// Move the player towards input direction.
    /// </summary>
    void Move() {
        Vector3 movementDirection = new Vector3(input.x, 0, input.y);
        controller.Move(movementDirection * playerSpeed * Time.deltaTime);
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
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Direction relative to the camera's rotation
    /// </summary>
    void CalculateDirection() {
        angle = Mathf.Atan2(input.x, input.y);
        angle = Mathf.Rad2Deg * angle;
        // Debug.Log("Angle: " + angle);
    }

    /// <summary>
    /// Rotate towards the calculate angle
    /// </summary>
    void Rotate() {
        targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
