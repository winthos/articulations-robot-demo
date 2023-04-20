using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

//this tests controlling the arm parts moving with force
public enum ABControlMode {Keyboard_Input, Actions};
public class TestABArmController : MonoBehaviour
{
    [SerializeField]
    public ABControlMode controlMode = ABControlMode.Keyboard_Input;

    [System.Serializable]
    public struct Joint
    {
        public TestABArmJointController robotPart;
    }

    public Joint[] joints;

    void Start()
    {
        //myAB = GetComponent<ArticulationBody>();
        foreach (Joint j in joints)
        {
            j.robotPart.GetComponent<TestABArmJointController>().myABArmControllerComponent = this.GetComponent<TestABArmController>();
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ActionMoveArmLiftUp(1.0f, 1.0f);
        }
    }

    public void ActionMoveArmLiftUp(float distance, float speed)
    {
        if(controlMode != ABControlMode.Actions)
        {
            return;
        }

        TestABArmJointController lift = joints[0].robotPart;

        //pre calculate all the things we need for physics
        var totalTimeNeededToReachDistanceAtSomeSpeed = distance/speed;
        var totalNumberOfTimeSteps = totalTimeNeededToReachDistanceAtSomeSpeed / Time.fixedDeltaTime;
        var distanceToChangeWithEachTimeStep = distance/totalNumberOfTimeSteps;

        lift.currentArmMoveParams = new ArmMoveParams()
        {
            distance = distance,
            speed = speed,
            timeTakenSoFar = 0.0f,
            totalTimeNeededToReachDistanceAtSomeSpeed = totalTimeNeededToReachDistanceAtSomeSpeed,
            totalNumberOfTimeSteps = totalNumberOfTimeSteps,
            distanceToChangeWithEachTimeStep = distanceToChangeWithEachTimeStep
        };

        lift.SetArmLiftState(ArmLiftState.MovingUp);
    }

    //callback that runs on loop when H or N keys are pressed to lift or lower arm rig
    public void OnMoveArmLift(InputAction.CallbackContext context)
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

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

    //reads input from the Player Input component to move an arm joint up and down along its local Y axis
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
            return ArmLiftState.Idle;
        }
    }

    public void OnMoveArmJoint1(InputAction.CallbackContext context) 
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        if(joints[1].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[1].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));
    }

    public void OnMoveArmJoint2(InputAction.CallbackContext context) 
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        if(joints[2].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[2].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));
    }

    public void OnMoveArmJoint3(InputAction.CallbackContext context) 
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        if(joints[3].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[3].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));

    }

    public void OnMoveArmJoint4(InputAction.CallbackContext context) 
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        if(joints[4].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[4].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));

    }


    //reads input from Player Input component to extend and retract arm joint
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
            return ArmExtendState.Idle;
        }
    }

    public void OnMoveArmWrist(InputAction.CallbackContext context)
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }
        
        if(joints[5].robotPart == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[5].robotPart;
        var input = context.ReadValue<float>();
        joint.SetArmRotateState(RotateStateFromInput(input));
    }

    //reads input from Player Input component to rotate arm joint
    ArmRotateState RotateStateFromInput (float input)
    {
        if(input > 0)
        {
            return ArmRotateState.Positive;
        }

        else if (input < 0)
        {
            return ArmRotateState.Negative;
        }
        else
        {
            return ArmRotateState.Idle;
        }
    }




}
