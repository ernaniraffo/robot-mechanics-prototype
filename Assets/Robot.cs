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
    private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // min move distance set to 0 to avoid inconsistency in grounded player
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0) {
            playerVelocity.y = 0f;
        }

        // move the player based on input direction
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(move * Time.deltaTime * playerSpeed);

        // set the forward vector of the gameobject to the direction we are going
        // TODO: fix robot's forward resetting abruptly
        if (move != Vector3.zero) {
            gameObject.transform.forward = move;
        }

        // make the player jump based on input
        if (Input.GetButton("Jump") && groundedPlayer) {
            Debug.Log("Performing a jump");
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        // move the player according to gravity value
        playerVelocity.y += gravityValue * Time.deltaTime;
        
        // move the player based on velocity
        controller.Move(playerVelocity * Time.deltaTime);

        // draw ray to show player's forward direction
        Debug.DrawRay(gameObject.transform.position, gameObject.transform.forward * 5, Color.red);
    }
}
