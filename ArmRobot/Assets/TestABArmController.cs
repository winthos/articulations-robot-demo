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
    //public ABControlMode controlMode = ABControlMode.Keyboard_Input;

    [System.Serializable]
    public struct Joint
    {
        public TestABArmJointController joint;
        //public float JointDistanceToMove;
    }

    public Joint[] joints;

    void Start()
    {
        //assign this controller as a reference in all joints
        foreach (Joint j in joints)
        {
            j.joint.GetComponent<TestABArmJointController>().myABArmControllerComponent = this.GetComponent<TestABArmController>();
        }
    }

    public void MoveArmBaseUp (float distance, float speed, float tolerance, float maxTimePassed, int positionCacheSize)
    {
        MoveArmBase(                    
            distance: distance,
            speed: speed,
            tolerance: tolerance,
            maxTimePassed: maxTimePassed,
            positionCacheSize: positionCacheSize,
            direction: 1 //going up
        );
    }

    public void MoveArmBaseDown (float distance, float speed, float tolerance, float maxTimePassed, int positionCacheSize)
    {
        MoveArmBase(                    
            distance: distance,
            speed: speed,
            tolerance: tolerance,
            maxTimePassed: maxTimePassed,
            positionCacheSize: positionCacheSize,
            direction: -1 //going down
        );
    }

    public void MoveArmBase (float distance, float speed, float tolerance, float maxTimePassed, int positionCacheSize, int direction)
    {
        //create a set of movement params for how we are about to move
        ArmMoveParams amp = new ArmMoveParams{
            distance = distance,
            speed = speed,
            tolerance = tolerance,
            maxTimePassed = maxTimePassed,
            positionCacheSize = positionCacheSize,
            direction = direction 
        };

        TestABArmJointController liftJoint = joints[0].joint;
        liftJoint.PrepToControlJointFromAction(amp);

    }

    //callback that runs on loop when H or N keys are pressed to lift or lower arm rig
    public void OnMoveArmLift(InputAction.CallbackContext context)
    {
        if(context.started == true)
        {
            if(LiftStateFromInput(context.ReadValue<float>()) == ArmLiftState.MovingUp)
            {
                //these parameters here act as if a researcher has put them in as an action
                MoveArmBaseUp(
                    distance: 0.5f,
                    speed: 4.0f,
                    tolerance: 1e-3f,
                    maxTimePassed: 5.0f,
                    positionCacheSize: 10
                );
            }

            else if(LiftStateFromInput(context.ReadValue<float>()) == ArmLiftState.MovingDown)
            {
                //these parameters here act as if a researcher has put them in as an action
                MoveArmBaseDown(
                    distance: 0.5f,
                    speed: 4.0f,
                    tolerance: 1e-3f,
                    maxTimePassed: 5.0f,
                    positionCacheSize: 10
                );
            }
        }
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


        if(joints[1].joint == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[1].joint;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));
    }

    public void OnMoveArmJoint2(InputAction.CallbackContext context) 
    {


        if(joints[2].joint == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[2].joint;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));
    }

    public void OnMoveArmJoint3(InputAction.CallbackContext context) 
    {


        if(joints[3].joint == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[3].joint;
        var input = context.ReadValue<float>();
        joint.SetArmExtendState(ExtendStateFromInput(input));

    }

    public void OnMoveArmJoint4(InputAction.CallbackContext context) 
    {


        if(joints[4].joint == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[4].joint;
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
        
        if(joints[5].joint == null) {
            throw new ArgumentException("Yo its null, please make not null");
        }
        TestABArmJointController joint = joints[5].joint;
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
