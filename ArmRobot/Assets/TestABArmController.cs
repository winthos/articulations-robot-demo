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
        //leaving room in this struct in case we ever need to annotate more stuff per joint
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

    //Server Action format to move the base of the arm up
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

    //server action format to move the base of the arm down
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

    //actually send the arm parameters to the joint moving up/down and begin movement
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

    public void MoveArmForward(float distance, float speed, float tolerance, float maxTimePassed, int positionCacheSize)
    {
        ExtendArm(                    
            distance: distance,
            speed: speed,
            tolerance: tolerance,
            maxTimePassed: maxTimePassed,
            positionCacheSize: positionCacheSize,
            direction: 1 //extend forward
        );
    }

    public void MoveArmBackward(float distance, float speed, float tolerance, float maxTimePassed, int positionCacheSize)
    {
        ExtendArm(                    
            distance: distance,
            speed: speed,
            tolerance: tolerance,
            maxTimePassed: maxTimePassed,
            positionCacheSize: positionCacheSize,
            direction: -1 //extend backward
        );
    }

    public void ExtendArm(float distance, float speed, float tolerance, float maxTimePassed, int positionCacheSize, int direction)
    {
        //get references to each joint
        TestABArmJointController joint1 = joints[1].joint;
        TestABArmJointController joint2 = joints[2].joint;
        TestABArmJointController joint3 = joints[3].joint;
        TestABArmJointController joint4 = joints[4].joint;
        //joint 5 is the wrist so don't do that here

        float totalExtendDistance = GetDriveUpperLimit(joint1) + 
                                    GetDriveUpperLimit(joint2) + 
                                    GetDriveUpperLimit(joint3) + 
                                    GetDriveUpperLimit(joint4);

        Debug.Log($"attempting to extend arm a total of {distance}");
        Debug.Log($"max extend distance is: {totalExtendDistance}");

        Dictionary<TestABArmJointController, float> jointToArmDistanceRatios = new Dictionary<TestABArmJointController, float>();
        Dictionary<TestABArmJointController, ArmMoveParams> jointToArmParams = new Dictionary<TestABArmJointController, ArmMoveParams>();

        //get the ratio of the total amount of movement each joint should be responsible for
        jointToArmDistanceRatios.Add(joint1, GetDriveUpperLimit(joint1)/totalExtendDistance);
        jointToArmDistanceRatios.Add(joint2, GetDriveUpperLimit(joint2)/totalExtendDistance);
        jointToArmDistanceRatios.Add(joint3, GetDriveUpperLimit(joint3)/totalExtendDistance);
        jointToArmDistanceRatios.Add(joint4, GetDriveUpperLimit(joint4)/totalExtendDistance);

        float total = 0.0f;

        foreach (TestABArmJointController joint in jointToArmDistanceRatios.Keys)
        {
            //assign each joint the distance it needs to move to have the entire arm try and move the total `distance`
            float myDistance = distance * jointToArmDistanceRatios[joint];
            Debug.Log($"distance for {joint} is {myDistance}");

            total += myDistance;

            ArmMoveParams amp = new ArmMoveParams{
                distance = myDistance,
                speed = speed,
                tolerance = tolerance,
                maxTimePassed = maxTimePassed,
                positionCacheSize = positionCacheSize,
                direction = direction 
            };

            jointToArmParams.Add(joint, amp);
        }

        Debug.Log($"total distance adds up to be: {total}");
        //set each joint in motion and in the fixed update we will track how much total distance has been moved
        //with each joint. If all joints's individual distances add up to the `distance` then we have reached our target extension amount
        foreach (TestABArmJointController joint in jointToArmParams.Keys)
        {
            joint.PrepToControlJointFromAction(jointToArmParams[joint]);
        }

    }

    public float GetDriveUpperLimit(TestABArmJointController joint, JointAxisType jointAxisType = JointAxisType.Extend)
    {
        float upperLimit = 0.0f;

        if(jointAxisType == JointAxisType.Extend)
        {
            //z drive
            upperLimit = joint.myAB.zDrive.upperLimit;
        }

        if(jointAxisType == JointAxisType.Lift)
        {
            //y drive
            upperLimit = joint.myAB.yDrive.upperLimit;
        }

        return upperLimit;
    }

    public void OnMoveArmJoint1(InputAction.CallbackContext context) 
    {
        if(context.started == true)
        {
            if(ExtendStateFromInput(context.ReadValue<float>()) == ArmExtendState.MovingForward)
            {
                //these parameters here act as if a researcher has put them in as an action
                MoveArmForward(
                    distance: 0.1f,
                    speed: 0.2f,
                    tolerance: 1e-4f,
                    maxTimePassed: 5.0f,
                    positionCacheSize: 10
                );
            }

            if(ExtendStateFromInput(context.ReadValue<float>()) == ArmExtendState.MovingBackward)
            {
                //these parameters here act as if a researcher has put them in as an action
                MoveArmBackward(
                    distance: 0.1f,
                    speed: 0.2f,
                    tolerance: 1e-4f,
                    maxTimePassed: 5.0f,
                    positionCacheSize: 10
                );
            }
        }
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
