using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera : MonoBehaviour
{
    public GameObject player;
    public Robot robot;
    // Set the offset of the camera from the player
    public Vector3 offset;

    // Inputs to move the camera
    Vector2 cameraRotateInput;
    Gamepad gamepad;
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position;
        robot = player.GetComponent<Robot>();

        gamepad = Gamepad.current;
    }

    // Update is called once per frame
    void Update()
    {
        GetGamePadInput();
        RotateCameraAroundPlayer();

        // Only update the camera position if player is walking
        //transform.position = new Vector3(player.transform.position.x + offset.x, transform.position.y, player.transform.position.z + offset.z);
    }

    void GetGamePadInput() {
        if (gamepad == null) return;
        cameraRotateInput = gamepad.rightStick.ReadValue();
    }

    void RotateCameraAroundPlayer() {
        if (cameraRotateInput == null) return;
        float horizontalInput = cameraRotateInput.x;
        float verticalInput = cameraRotateInput.y;
        float sensitivity = 0.25f;
        // Rotate in regards to the X axis (side to side)
        transform.RotateAround(player.transform.position, -Vector3.up, cameraRotateInput.x * sensitivity);
        // Rotate in regards to the X axis (side to side)
        transform.RotateAround(Vector3.zero, transform.right, cameraRotateInput.y * sensitivity);
    }
}
