using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        // if(Input.GetKeyDown(KeyCode.G))
        // {
        //     LiftUp(0.3f);
        // }

        // if(Input.GetKeyDown(KeyCode.B))
        // {
        //     LiftDown(0.3f);
        // }
        // Debug.Log(myAB.velocity);
    }

    //ok so this literally just moves the drive to some offset specified
    //drive applies force with: F = stiffness * (currentPosition - target) - damping * (currentVelocity - targetVelocity)
    // private void LiftUp(float distance)
    // {
    //     var drive = myAB.yDrive;
    //     float yDrivePosition = myAB.jointPosition[0];
    //     drive.target = yDrivePosition + distance;
    //     myAB.yDrive = drive;    
    // }

    // private void LiftDown(float distance)
    // {
    //     var drive = myAB.yDrive;
    //     float yDrivePosition = myAB.jointPosition[0];
    //     drive.target = yDrivePosition - distance;
    //     myAB.yDrive = drive;    
    // }

    public void ControlJointFromAction(float distance)
    {
        if(jointAxisType == JointAxisType.Lift && liftState == ArmLiftState.Idle)
        {
            if(distance < 0)
            {
                liftState = ArmLiftState.MovingDown;
            }

            else
            {
                liftState = ArmLiftState.MovingUp;
            }

            var drive = myAB.yDrive;
            float yDrivePosition = myAB.jointPosition[0];
            drive.target = yDrivePosition + distance;
            myAB.yDrive = drive;       

            //launch coroutine to check when the movement has finished
            StartCoroutine(AreWeDoneMoving());
            return;
        }
    }

    public float TimeOut = 5.0f;
    public float Tolerance = 1e-3f;
    public int NumberOfCachedVelocities = 5;

    //how to determine if we have "reached target succesfully"
    // compare velocity? if it hasn't changed or reached zero, we have stopped moving
    // compare target position and current joint position? If close enough, we have reached target
    // some sort of time based check, calculate how long it would take to move to target and time out?
    private IEnumerator AreWeDoneMoving()
    {
        float timePassed = 0;
        //float lastVelocityMagnitude = 0f;
        float[] cachedVelocities = new float[NumberOfCachedVelocities];
        int oldestCachedIndex = 0;

        while(liftState != ArmLiftState.Idle)
        {   
            yield return new WaitForFixedUpdate();
            timePassed += Time.deltaTime;

            var currentVelocityMagnitude = myAB.velocity.magnitude;

            cachedVelocities[oldestCachedIndex] = currentVelocityMagnitude;
            //update the last cached velocities up to the max number we wanted to cache
            oldestCachedIndex = (oldestCachedIndex + 1) % NumberOfCachedVelocities;

            //update the oldest index updated in the cached velocities I guess????
            if(oldestCachedIndex == 0)
            {
                cachedVelocities[oldestCachedIndex] = currentVelocityMagnitude;
            }

            //compare all cached velocities to see if they are all within the threshold
            if(CheckArrayWithinStandardDeviation(cachedVelocities, Tolerance))
            {
                Debug.Log($"last {NumberOfCachedVelocities} velocities were within tolerence: {Tolerance}");
                liftState = ArmLiftState.Idle;
            }

            //hard timeout check in case for some reason the cached velocity comparisons go infinite
            if(timePassed >= TimeOut)
            {
                Debug.Log("hard time out check happened");
                liftState = ArmLiftState.Idle;
            }

            //Debug.Log(myAB.jointPosition[0]);
            //Debug.Log(myAB.yDrive.target);
        }

        Debug.Log("done moving!");
        yield return null;
    }

bool CheckArrayWithinStandardDeviation(float[] values, float standardDeviation)
{
    // Calculate the mean value of the array
    float mean = values.Average();

    // Calculate the sum of squares of the differences between each value and the mean
    float sumOfSquares = 0.0f;
    foreach (float value in values)
    {
        sumOfSquares += (value - mean) * (value - mean);
    }

    // Calculate the standard deviation of the array
    float arrayStdDev = (float)Mathf.Sqrt(sumOfSquares / values.Length);

    // Check if the standard deviation of the array is within the specified range
    return arrayStdDev <= standardDeviation;
}


    private bool SeeIfTheFloatsAreWithinTheTolerance(float a, float b, float tolerance)
    {
        var diff = Mathf.Abs(a - b);
        return diff <= tolerance || diff <= Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)) * tolerance;
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

    // private int count = 0;

    // private void ControlJointFromAction()
    // {
    //     if (currentArmMoveParams.timeTakenSoFar < currentArmMoveParams.totalTimeNeededToReachDistanceAtSomeSpeed)
    //     {
    //         //Debug.Log($"time taken moving so far: {currentArmMoveParams.timeTakenSoFar}");
    //         float yDrivePosition = myAB.jointPosition[0];
    //         Debug.Log($"yDrivePosition: {yDrivePosition}");

    //         var drive = myAB.yDrive;

    //         //ok we haven't reached the last set target position yet, so skip
    //         if(yDrivePosition < currentArmMoveParams.lastTargetPosition)
    //         {
    //             Debug.Log("in second loop");
    //             drive.target = currentArmMoveParams.lastTargetPosition;
    //             myAB.yDrive = drive;

    //             currentArmMoveParams.timeTakenSoFar += Time.deltaTime;
    //             count++;
    //             return;
    //         }

    //         //set the new target position we want this body to reach
    //         float targetPosition = yDrivePosition + currentArmMoveParams.distanceToChangeWithEachTimeStep;

    //         currentArmMoveParams.lastTargetPosition = targetPosition;

    //         //ok all this sets the drive to move toward the current targetPosition
    //         drive.target = targetPosition;
    //         myAB.yDrive = drive;

    //         currentArmMoveParams.timeTakenSoFar += Time.deltaTime;
    //         count++;
    //         Debug.Log($"count of fixed updates: {count}");
    //     }

    //     else
    //     {
    //         //we have finished moving to the target position so set arm to idle and stop moving
    //         SetArmLiftState(ArmLiftState.Idle);
    //     }
    // }

    private void FixedUpdate()
    { 
        //keyboard input
        if(myABArmControllerComponent.controlMode == ABControlMode.Keyboard_Input)
        {
            ControlJointFromKeyboardInput();
        }

        //action input, pass in direction, speed, etc
        // else if(myABArmControllerComponent.controlMode == ABControlMode.Actions)
        // {
        //     if(liftState != ArmLiftState.Idle)
        //     {
        //         //Debug.Log("try to move arm lift up with action");
        //         ControlJointFromAction();
        //     }
        // }
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
