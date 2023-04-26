using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestABController_Rails_Basic : MonoBehaviour
{
    public GameObject forceTarget = null;

    [SerializeField]
    [Header("Control with Keyboard or Action")]
    ABControlMode controlMode = ABControlMode.Keyboard_Input;

    [Header("Reference to this Articulation Body")]
    public ArticulationBody ab;
    public List<ArticulationBody> abChildren = new List<ArticulationBody>();

    [Header("Speed for movement and rotation")]
    public float moveSpeed;
    public float rotateSpeed;
    private float move;
    private float look;
    
    public void OnMove(InputAction.CallbackContext context) 
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        move = context.ReadValue<float>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        look = context.ReadValue<float>();
    }


    private void FixedUpdate() 
    {
        Move();
        Rotate();
    }
    void Move()
    {
        //find target velocity
        Vector3 currentVelocity = ab.velocity;
        Vector3 targetvelocity = new Vector3(0, 0, move);
        targetvelocity *= moveSpeed;

        //allign direction
        targetvelocity = ab.transform.TransformDirection(targetvelocity);

        //calculate forces
        Vector3 velocityChange = (targetvelocity - currentVelocity);

        //ab.AddForce(velocityChange);
        Vector3 forcePosition = new Vector3();
        if(forceTarget != null)
        {
            forcePosition = forceTarget.transform.position;
        }

        else
        {
            //forcePosition = ab.worldCenterOfMass;
        }
        // Debug.Log("Applying force of " + velocityChange);

        // change center of mass
        //ab.centerOfMass = Vector3.zero;
        foreach (ArticulationBody abChild in abChildren) {
            //abChild.centerOfMass = abChild.transform.InverseTransformPoint(ab.worldCenterOfMass);
            Debug.Log(abChild.gameObject.name + "'s new centerOfMass is (" + abChild.centerOfMass.x + ", " + abChild.centerOfMass.y + ", " + abChild.centerOfMass.z + ")");
        }

        // Debug.Log("New center of mass is " + ab.centerOfMass);
        ab.AddForceAtPosition(velocityChange, forcePosition);
    }

    void Rotate()
    {
        //note 2021 unity adds a forceMode paramater to AddTorque so uhhhh
        ab.AddTorque(Vector3.up * look * rotateSpeed);
    }

}
