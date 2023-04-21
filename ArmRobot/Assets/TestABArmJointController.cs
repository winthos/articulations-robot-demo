using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmLiftState { Idle = 0, MovingDown = -1, MovingUp = 1 };
public enum ArmExtendState {Idle = 0, MovingBackward = -1, MovingForward = 1};
public enum ArmRotateState {Idle = 0, Negative = -1, Positive = 1};
public enum JointAxisType {Unassigned, Extend, Lift, Rotate};

public class ArmMoveParams
{
    public float distance;
    public float speed;
    public float timeTakenSoFar;
    public float totalTimeNeededToReachDistanceAtSomeSpeed;
    public float totalNumberOfTimeSteps;
    public float distanceToChangeWithEachTimeStep;
    public float lastTargetPosition;
}

public class TestABArmJointController : MonoBehaviour
{
    public JointAxisType jointAxisType = JointAxisType.Unassigned;
    public ArmRotateState rotateState = ArmRotateState.Idle;
    public ArmLiftState liftState = ArmLiftState.Idle;
    public ArmExtendState extendState = ArmExtendState.Idle;
    public ArmMoveParams currentArmMoveParams;

    public void SetArmLiftState(ArmLiftState armState)
    {
        liftState = armState;
    }

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
    public TestABArmController myABArmControllerComponent;

    void Start() 
    {
        myAB = this.GetComponent<ArticulationBody>();
        //Debug.Log(myAB.linearLockX);
    }    

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            LiftUp(0.3f);
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            LiftDown(0.3f);
        }
    }

    //ok so this literally just moves the drive to some offset specified
    //drive applies force with: F = stiffness * (currentPosition - target) - damping * (currentVelocity - targetVelocity)
    private void LiftUp(float distance)
    {
        var drive = myAB.yDrive;
        float yDrivePosition = myAB.jointPosition[0];
        drive.target = yDrivePosition + distance;
        myAB.yDrive = drive;    
    }

    private void LiftDown(float distance)
    {
        var drive = myAB.yDrive;
        float yDrivePosition = myAB.jointPosition[0];
        drive.target = yDrivePosition - distance;
        myAB.yDrive = drive;    
    }

    private void ControlJointFromKeyboardInput()
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
                //speed = dist/time
                //so time.fixedDelta * (m/s), results in some distance position change
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
                //Debug.Log(zDrivePostion);

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

    private int count = 0;

    private void ControlJointFromAction()
    {
        if (currentArmMoveParams.timeTakenSoFar < currentArmMoveParams.totalTimeNeededToReachDistanceAtSomeSpeed)
        {
            //Debug.Log($"time taken moving so far: {currentArmMoveParams.timeTakenSoFar}");
            float yDrivePosition = myAB.jointPosition[0];
            Debug.Log($"yDrivePosition: {yDrivePosition}");

            var drive = myAB.yDrive;

            //ok we haven't reached the last set target position yet, so skip
            if(yDrivePosition < currentArmMoveParams.lastTargetPosition)
            {
                Debug.Log("in second loop");
                drive.target = currentArmMoveParams.lastTargetPosition;
                myAB.yDrive = drive;

                currentArmMoveParams.timeTakenSoFar += Time.deltaTime;
                count++;
                return;
            }

            //set the new target position we want this body to reach
            float targetPosition = yDrivePosition + currentArmMoveParams.distanceToChangeWithEachTimeStep;

            currentArmMoveParams.lastTargetPosition = targetPosition;

            //ok all this sets the drive to move toward the current targetPosition
            drive.target = targetPosition;
            myAB.yDrive = drive;

            currentArmMoveParams.timeTakenSoFar += Time.deltaTime;
            count++;
            Debug.Log($"count of fixed updates: {count}");
        }

        else
        {
            //we have finished moving to the target position so set arm to idle and stop moving
            SetArmLiftState(ArmLiftState.Idle);
        }
    }

    private void FixedUpdate()
    { 
        //keyboard input
        if(myABArmControllerComponent.controlMode == ABControlMode.Keyboard_Input)
        {
            ControlJointFromKeyboardInput();
        }

        //action input, pass in direction, speed, etc
        else if(myABArmControllerComponent.controlMode == ABControlMode.Actions)
        {
            if(liftState != ArmLiftState.Idle)
            {
                //Debug.Log("try to move arm lift up with action");
                ControlJointFromAction();
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
