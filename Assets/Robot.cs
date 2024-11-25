using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        // controller.center = new Vector3(0, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer) {
            Debug.Log("Player is grounded");
        }

        // move the player according to gravity value
        playerVelocity.y += gravityValue * Time.deltaTime;
        
        // move the player based on velocity
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
