using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestABWheelsController : MonoBehaviour
{
    public ArticulationBody ab;
    public ArticulationBody[] wheels;
    public float moveSpeed;
    public float rotateSpeed;
    private float move;
    private float look;

    public void OnMove(InputAction.CallbackContext context) 
    {
        move = context.ReadValue<float>();
        Debug.Log(move);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<float>();
        Debug.Log(look);
    }

    private void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            Debug.Log("Changing joint!");
            GameObject.Find("stretch_robot_lift").GetComponent<ArticulationBody>().jointType = ArticulationJointType.FixedJoint;
        }
    }

    private void FixedUpdate() 
    {
        Move();
        Rotate();
        //turn
        
    }

    void LateUpdate() 
    {

    }

    void Move()
    {
        // //find target velocity
        // Vector3 currentVelocity = ab.velocity;
        // Vector3 targetvelocity = new Vector3(0, 0, move);
        // targetvelocity *= moveSpeed;

        // //align direction
        // targetvelocity = transform.TransformDirection(targetvelocity);

        // //calculate forces
        // Vector3 velocityChange = (targetvelocity - currentVelocity);

        // ab.AddForce(velocityChange);

        
        wheels[0].AddTorque(wheels[0].transform.right * move * moveSpeed);
        wheels[1].AddTorque(wheels[1].transform.right * move * moveSpeed);
        Debug.Log("Torquing wheels " + wheels[1].transform.right * move * moveSpeed);
    }

    void Rotate() {
        
        wheels[0].AddTorque(wheels[0].transform.right * look * rotateSpeed);
        wheels[1].AddTorque(-wheels[1].transform.right * look * rotateSpeed);
        Debug.Log("Torquing " + Vector3.right * look * rotateSpeed);
    }
}
