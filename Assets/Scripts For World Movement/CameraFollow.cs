using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    //we create a smooth speed variable so the camera dosen't just teleport to where the player is and look janky
    [SerializeField] Transform target;
    [SerializeField] float smoothSpeed = 0.5f;
    [SerializeField] Vector3 offset; 

    private void LateUpdate()
    {
        //if the target dosen't exist we don't update the camera
        if(target == null) return;

        Vector3 desiredPosition = target.position + offset;
        //Lerp allows us to smoothly transition from postion to desired positon at a smooth speed
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = new Vector3(smoothPosition.x, smoothPosition.y, transform.position.z);
    }
    
}
