using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestABController : MonoBehaviour
{
    public ArticulationBody ab;
    public float moveSpeed;
    public float rotateSpeed;
    private Vector2 move;
    private float look;

    public void OnMove(InputAction.CallbackContext context) 
    {
        move = context.ReadValue<Vector2>();
        Debug.Log($"OnMove input is: {move}");
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<float>();
    }

    private void FixedUpdate() 
    {
        Move();
        //turn
        ab.AddTorque(Vector3.up * look * rotateSpeed);
    }

    void LateUpdate() 
    {

    }

    void Move()
    {
        //find target velocity
        Vector3 currentVelocity = ab.velocity;
        Vector3 targetvelocity = new Vector3(move.x, 0, move.y);
        targetvelocity *= moveSpeed;

        //allign direction
        targetvelocity = transform.TransformDirection(targetvelocity);

        //calculate forces
        Vector3 velocityChange = (targetvelocity - currentVelocity);

        ab.AddForce(velocityChange);
    }
}
