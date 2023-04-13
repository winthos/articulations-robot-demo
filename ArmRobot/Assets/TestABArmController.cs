using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

//this tests controlling the arm parts moving with force
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

    //callback that runs on loop when H or N keys are pressed to lift or lower arm rig
    public void OnMoveArmLift(InputAction.CallbackContext context)
    {
        if(joints[0].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController lift = joints[0].robotPart;
        var input = context.ReadValue<float>();
        //Debug.Log($"ArmMoveState input is: {input}");
        lift.SetArmLiftState(LiftStateFromInput(input));
        //Debug.Log($"Lift liftState set to {lift.liftState}");
        //now pass the arm state to the arm joint controller here I guess inside this callback?
        
    }

    //reads input from the H and N keys via Player Input to lift or lower the arm rig
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

    public void OnMoveArmJoint1(InputAction.CallbackContext context) 
    {
        if(joints[1].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[1].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));
    }

    public void OnMoveArmJoint2(InputAction.CallbackContext context) 
    {
        if(joints[2].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[2].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));
    }

    public void OnMoveArmJoint3(InputAction.CallbackContext context) 
    {
        if(joints[3].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[3].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));

    }

    public void OnMoveArmJoint4(InputAction.CallbackContext context) 
    {
        if(joints[4].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[4].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));

    }

    //reads input from the "JKL;" and "M,./" keys via Player Input to move first joint forward and back
    ArmExtendState ExtendStateFromInput (float input)
    {
        if(input > 0)
        {
            return ArmExtendState.MovingForward;
        }

        else if (input < 0)
        {
            return ArmExtendState.MovingBackward;
        }
        else
        {
            return ArmExtendState.Fixed;
        }
    }






}
