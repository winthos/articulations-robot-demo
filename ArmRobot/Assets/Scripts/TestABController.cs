using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MoveState {Idle = 0, Backward = -1, Forward = 1};

public enum RotateState {Idle = 0, Negative = -1, Positive = 1};

public class TestABController : MonoBehaviour
{
    public GameObject forceTarget = null;

    [SerializeField]
    [Header("Control with Keyboard or Action")]
    ABControlMode controlMode = ABControlMode.Keyboard_Input;

    [Header("Reference to this Articulation Body")]
    public ArticulationBody ab;

    [Header("Speed for movement and rotation")]
    public float moveSpeed;
    public float rotateSpeed;
    private float move;
    private float look;

    [Header("Colliders used when moving or rotating")]
    public GameObject MoveColliders;
    public GameObject RotateColliders;

    [Header("Debug Move and Rotate states")]
    public MoveState moveState = MoveState.Idle;
    public RotateState rotateState = RotateState.Idle;
    
    public void OnMove(InputAction.CallbackContext context) 
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        move = context.ReadValue<float>();
        SetMoveState(MoveStateFromInput(move));
    }

    private void SetMoveState (MoveState state) 
    {
        moveState = state;
    }

    MoveState MoveStateFromInput (float input)
    {
        if (input > 0)
        {
            return MoveState.Forward;
        }
        else if (input < 0)
        {
            return MoveState.Backward;
        }
        else
        {
            return MoveState.Idle;
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        look = context.ReadValue<float>();
        SetRotateState(RotateStateFromInput(look));
    }

    private void SetRotateState(RotateState state)
    {
        rotateState = state;
    }

    RotateState RotateStateFromInput (float input)
    {
        if (input > 0)
        {
            return RotateState.Positive;
        }
        else if (input < 0)
        {
            return RotateState.Negative;
        }
        else
        {
            return RotateState.Idle;
        }    
    }

    private void FixedUpdate() 
    {
        UpdateCollidersForMovement();
        Move();
        Rotate();
    }
    void Move()
    {
        if(rotateState == RotateState.Idle && moveState != MoveState.Idle)
        {
            //find target velocity
            Vector3 currentVelocity = ab.velocity;
            Vector3 targetvelocity = new Vector3(0, 0, move);
            targetvelocity *= moveSpeed;

            //allign direction
            targetvelocity = transform.TransformDirection(targetvelocity);

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
                forcePosition = ab.transform.position;
            }

            ab.AddForceAtPosition(velocityChange, forcePosition);
        }
    }

    void Rotate()
    {
        if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
        {
            //note 2021 unity adds a forceMode paramater to AddTorque so uhhhh
            ab.AddTorque(Vector3.up * look * rotateSpeed);
        }
    }

    public void UpdateCollidersForMovement()
    {
        if(MoveColliders == null)
        {
            return;
        }

        if(RotateColliders == null)
        {
            return;
        }

        //moving forward/backward
        if((moveState == MoveState.Forward || moveState == MoveState.Backward) && rotateState == RotateState.Idle)
        {
            if(MoveColliders.activeSelf == false)
            MoveColliders.SetActive(true);

            if(RotateColliders.activeSelf == true)
            RotateColliders.SetActive(false);
        }

        //rotating
        else if ((rotateState == RotateState.Positive || rotateState == RotateState.Negative) && moveState == MoveState.Idle)
        {
            if(MoveColliders.activeSelf == true)
            MoveColliders.SetActive(false);

            if(RotateColliders.activeSelf == false)
            RotateColliders.SetActive(true);
        }
    }

}
