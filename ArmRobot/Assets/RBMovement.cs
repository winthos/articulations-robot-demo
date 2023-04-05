using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RBMovement : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject camHolder;
    public float MoveSpeed, LookSensitivity, maxForce;
    private Vector2 move;
    private float look;
    private float lookRotation;

    public void OnMove(InputAction.CallbackContext context) 
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<float>();
    }

    private void FixedUpdate() 
    {
        Move();
    }

    void LateUpdate() 
    {
        //turn
        transform.Rotate(Vector3.up * look * Time.deltaTime * LookSensitivity);
    }

    void Move()
    {
        //find target velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetvelocity = new Vector3(move.x, 0, move.y);
        targetvelocity *= MoveSpeed;

        //allign direction
        targetvelocity = transform.TransformDirection(targetvelocity);

        //calculate forces
        Vector3 velocityChange = (targetvelocity - currentVelocity);

        //limit force
        Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}
