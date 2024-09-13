using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************
* RotateEarth
* 
* Script to rotate an object with the mouse
* 
***********************************************************************/
public class V360_RotateEarth : MonoBehaviour
{
    private bool isDragging = false;
    private Vector2 lastMousePos;
    public float speed;
    public int coolDown;
    private float currentSpeed = 0;
    private Vector2 dp;

    /*************************************************************************
    * Update
    * 
    * Runs once per frame
    * 
    ***********************************************************************/
    void Update()
    {
        DoMouseRotation();
    }

    /*************************************************************************
    * DoMouseRotation
    * 
    * Rotates an object spherically using only pitch and yaw
    * 
    ***********************************************************************/
    private void DoMouseRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) == true && isDragging)
        {
            Vector2 currPos = Input.mousePosition;
            Vector2 deltaPos = currPos - lastMousePos;

            this.transform.Rotate(this.transform.InverseTransformDirection(this.transform.up), deltaPos.x * -speed);
            this.transform.Rotate(this.transform.InverseTransformDirection(Vector3.left), deltaPos.y * -speed);

            lastMousePos = Input.mousePosition;
            dp = deltaPos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            currentSpeed = speed;
        }
        else if (currentSpeed > 0)
        {
            this.transform.Rotate(this.transform.InverseTransformDirection(this.transform.up), dp.x * -currentSpeed);
            this.transform.Rotate(this.transform.InverseTransformDirection(Vector3.left), dp.y * -currentSpeed);
            currentSpeed -= speed / (float)coolDown;

        }
    }
}
