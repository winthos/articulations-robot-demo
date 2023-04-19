using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmLiftState { Idle = 0, MovingDown = -1, MovingUp = 1 };
public enum ArmExtendState {Idle = 0, MovingBackward = -1, MovingForward = 1};
public enum ArmRotateState {Idle = 0, Negative = -1, Positive = 1};

public enum JointAxisType {Unassigned, Extend, Lift, Rotate};
public class TestABArmJointController : MonoBehaviour
{
    public JointAxisType jointAxisType = JointAxisType.Unassigned;
    private ArmRotateState rotateState = ArmRotateState.Idle;
    private ArmLiftState liftState = ArmLiftState.Idle;

    public void SetArmLiftState(ArmLiftState armState)
    {
        liftState = armState;
    }

    private ArmExtendState extendState = ArmExtendState.Idle;
    public void SetArmExtendState (ArmExtendState armState) 
    {
        extendState = armState;
    }

    public void SetArmRotateState (ArmRotateState armState)
    {
        rotateState = armState;
    } 

    public float speed = 1.0f;
    public ArticulationBody myAB;

    void Start() 
    {
        myAB = this.GetComponent<ArticulationBody>();
        //Debug.Log(myAB.linearLockX);
    }    

    private void FixedUpdate()
    {
        //raise and lower along the Y axis
        if(jointAxisType == JointAxisType.Lift) 
        {
            if (liftState != ArmLiftState.Idle)
            {
                //get jointPosition along y axis
                float yDrivePostion = myAB.jointPosition[0];
                //Debug.Log(xDrivePostion);

                //increment this y position
                float targetPosition = yDrivePostion + (float)liftState * Time.fixedDeltaTime * speed;

                //set joint Drive to new position
                var drive = myAB.yDrive;
                drive.target = targetPosition;
                myAB.yDrive = drive;
            }
        }

        //extend and move forward along the Z axis
        else if(jointAxisType == JointAxisType.Extend) 
        {
            if(extendState != ArmExtendState.Idle) 
            {
                //get jointPosition along y axis
                float zDrivePostion = myAB.jointPosition[0];
                //Debug.Log(xDrivePostion);

                //increment this y position
                float targetPosition = zDrivePostion + (float)extendState * Time.fixedDeltaTime * speed;

                //set joint Drive to new position
                var drive = myAB.zDrive;
                drive.target = targetPosition;
                myAB.zDrive = drive;
            }
        }

        //rotate about the Y axis
        else if (jointAxisType == JointAxisType.Rotate) 
        {
            if(rotateState != ArmRotateState.Idle)
            {
                //note the {speed} for rotation seems to need to scale due to it being in radians/rotational degrees
                //so at least for testing at the moment the "speed" value of the wrist is really high
                float rotationChange = (float)rotateState * speed * Time.fixedDeltaTime;
                float rotationGoal = CurrentPrimaryAxisRotation() + rotationChange;
                RotateTo(rotationGoal);
            }
        }
    }

    // MOVEMENT HELPERS for rotate

    //get current rotation value in degrees
    float CurrentPrimaryAxisRotation()
    {
        float currentRotationRads = myAB.jointPosition[0];
        float currentRotation = Mathf.Rad2Deg * currentRotationRads;
        return currentRotation;
    }

    void RotateTo(float primaryAxisRotation)
    {
        var drive = myAB.xDrive;
        drive.target = primaryAxisRotation;
        myAB.xDrive = drive;
    }
}
