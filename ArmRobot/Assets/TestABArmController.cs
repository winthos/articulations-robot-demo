using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//this tests controlling the arm parts moving with force

//this enum can be used to determine the move state of any given arm joint
public enum ArmMoveState {Fixed = 0, MovingForward = 1, MovingBackward = -1};

public class TestABArmController : MonoBehaviour
{
    // private ArticulationBody myAB;
    // public float moveSpeed;
    // private float armTarget;

    [System.Serializable]
    public struct Joint
    {
        public string nameOfRobotPart;
        public TestABArmJointController robotPart;
    }

    public Joint[] joints;

    void Start()
    {
        //myAB = GetComponent<ArticulationBody>();
    }

    //set the arm state to actively moving forward, actively moving back, or fixed in place based on 2d axis input
    //can be used generically for any arm joint
    ArmMoveState MoveStateFromInput (float input)
    {
        if(input > 0)
        {
            return ArmMoveState.MovingForward;
        }

        else if (input < 0)
        {
            return ArmMoveState.MovingBackward;
        }
        else
        {
            return ArmMoveState.Fixed;
        }
    }

    ArmLiftState LiftStateFromInput (float input)
    {
        if (input > 0)
        {
            return ArmLiftState.MovingUp;
        }
        else if (input < 0)
        {
            return ArmLiftState.MovingDown;
        }
        else
        {
            return ArmLiftState.Fixed;
        }
    }

    //move the first arm joint via inputs from J and M keys
    public void OnMoveArmLift(InputAction.CallbackContext context)
    {
        TestABArmJointController lift = joints[0].robotPart;
        var input = context.ReadValue<float>();
        //Debug.Log($"ArmMoveState input is: {input}");
        lift.liftState = LiftStateFromInput(input);
        Debug.Log($"MoveState set to {lift.liftState}");
        //now pass the arm state to the arm joint controller here I guess inside this callback?
        
    }

    public void OnMoveArmJoint1() 
    {

    }

}
