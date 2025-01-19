using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public GameObject player;
    public Robot robot;
    // Set the offset of the camera from the player
    public Vector3 offset;
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position;
        robot = player.GetComponent<Robot>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only update the camera position if player is walking
        transform.position = new Vector3(player.transform.position.x + offset.x, transform.position.y, player.transform.position.z + offset.z);
    }
}
