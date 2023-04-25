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

    public float MaxTimeTaken = 5.0f;
    private float timeTaken = 0.0f;
    public float Tolerance = 1e-3f;
    public int NumberOfCachedPositions = 5;
    private float[] cachedPositions;
    private float[] cachedDistanceMagnitudes;
    private int oldestCachedIndex;
    private float distanceToMove;
    //public float distanceToMoveEpsilon= 1e-1f;
    private float initialPosition;

    private float distanceToRotate;

    void Start() 
    {
        myAB = this.GetComponent<ArticulationBody>();
        //Debug.Log(myAB.linearLockX);
    }    

    private void Update()
    {

    }

    //do all this stuff once before we start moving this body
    public void PrepToControlJointFromAction(float distance)
    {
        if(Mathf.Approximately(distance, 0.0f))
        {
            Debug.Log("Error! distance to move must be nonzero");
            return;
        }

        //we are a lift type joint
        if(jointAxisType == JointAxisType.Lift)
        {
            if(liftState == ArmLiftState.Idle)
            {
                //zero out the distanceToMove
                distanceToMove = Mathf.Abs(distance);
                Debug.Log($"distance to move is: {distanceToMove}");
                //clear out the position cache
                cachedPositions = new float[NumberOfCachedPositions];
                cachedDistanceMagnitudes = new float[NumberOfCachedPositions];
                //clear out time taken
                timeTaken = 0.0f;
                oldestCachedIndex = 0;
                initialPosition = myAB.jointPosition[0];

                //set if we are moving up or down based on sign of distance from input
                if(distance < 0)
                {
                    Debug.Log("setting lift state to move down");
                    liftState = ArmLiftState.MovingDown;
                }

                else
                {
                    Debug.Log("setting lift state to move up");
                    liftState = ArmLiftState.MovingUp;
                }

                //now that lift state is set, on next fixed update
                //ControlJointFromAction will start updating the drive position
            }
        }
    }

    //this is called once every frame update
    public void ControlJointFromAction()
    {
        //we are a lift type joint
        if(jointAxisType == JointAxisType.Lift)
        {
            //if instead we are moving up or down actively
            if(liftState != ArmLiftState.Idle)
            {
                var drive = myAB.yDrive;
                float yDrivePosition = myAB.jointPosition[0];
                float targetPosition = yDrivePosition + (float)liftState * Time.fixedDeltaTime * speed;    
                //Debug.Log(targetPosition);            
                drive.target = targetPosition;
                myAB.yDrive = drive;      

                //now cache and check positions based on some tolerance value or iterate time out
                //see if we are close to the distanceToMove
                var currentPosition = yDrivePosition;
                Debug.Log($"currentPosition: {currentPosition}");
                //cache the position at the moment
                cachedPositions[oldestCachedIndex] = currentPosition;

                Debug.Log($"initialPosition: {initialPosition}");

                var distanceMovedSoFar = Mathf.Abs(currentPosition - initialPosition);
                Debug.Log($"distance moved so far is: {distanceMovedSoFar}");

                //iterate next index in cache
                oldestCachedIndex = (oldestCachedIndex + 1) % NumberOfCachedPositions;

                //if we have looped around at least once, now start checking
                if(oldestCachedIndex == 0)
                {
                    cachedPositions[oldestCachedIndex] = currentPosition;
                    //compare the distance from current position to the target position (distanceToMove)
                    //for the last {NumberOfCachedPositions} positions, and if that amount hasn't changed
                    //by the {tolerance} deviation then we have stopped moving
                    if(CheckArrayWithinStandardDeviation(cachedPositions, Tolerance))
                    {
                        Debug.Log($"last {NumberOfCachedPositions} positions were about the same");
                        liftState = ArmLiftState.Idle;
                    }
                }


                if(distanceMovedSoFar >= distanceToMove)
                {
                    Debug.Log("we have moved to or a little beyond the distance specified to move so STOOOP");
                    liftState = ArmLiftState.Idle;
                }
                

                //also check if we actually reached close to the target position becuase that means
                //we got there unobstructed

                //this one seems to miss the check because sometimes the difference between this and last fixed update is greater
                //than the epsilon so uhhhhhhhhhhhhh
                // if(CheckIfDistanceMagnitudesAreCloseToDistanceToMove(distanceMovedSoFar, distanceToMove, distanceToMoveEpsilon))
                // {
                //     Debug.Log($"distance reached");
                //     liftState = ArmLiftState.Idle;
                // }
                
                //otherwise we have a hard timer to stop movement so we don't move forever and crash unity
                timeTaken += Time.deltaTime;

                if(timeTaken >= MaxTimeTaken)
                {
                    liftState = ArmLiftState.Idle;
                    return;
                }
            }

            return;
        }
    }


    //how to determine if we have "reached target succesfully"
    // compare velocity? if it hasn't changed or reached zero, we have stopped moving
    // compare target position and current joint position? If close enough, we have reached target
    // some sort of time based check, calculate how long it would take to move to target and time out?
    // private IEnumerator AreWeDoneMoving()
    // {
    //     float timePassed = 0;
    //     //float lastVelocityMagnitude = 0f;
    //     float[] cachedVelocities = new float[NumberOfCachedVelocities];
    //     int oldestCachedIndex = 0;

    //     while(liftState != ArmLiftState.Idle)
    //     {   
    //         yield return new WaitForFixedUpdate();
    //         timePassed += Time.deltaTime;

    //         var currentVelocityMagnitude = myAB.velocity.magnitude;

    //         cachedVelocities[oldestCachedIndex] = currentVelocityMagnitude;
    //         //update the last cached velocities up to the max number we wanted to cache
    //         oldestCachedIndex = (oldestCachedIndex + 1) % NumberOfCachedVelocities;

    //         //update the oldest index updated in the cached velocities I guess????
    //         if(oldestCachedIndex == 0)
    //         {
    //             cachedVelocities[oldestCachedIndex] = currentVelocityMagnitude;
    //         }

    //         //compare all cached velocities to see if they are all within the threshold
    //         if(CheckArrayWithinStandardDeviation(cachedVelocities, Tolerance))
    //         {
    //             Debug.Log($"last {NumberOfCachedVelocities} velocities were within tolerence: {Tolerance}");
    //             liftState = ArmLiftState.Idle;
    //         }

    //         //hard timeout check in case for some reason the cached velocity comparisons go infinite
    //         if(timePassed >= TimeOut)
    //         {
    //             Debug.Log("hard time out check happened");
    //             liftState = ArmLiftState.Idle;
    //         }

    //         Debug.Log(myAB.jointPosition[0]);
    //         Debug.Log(myAB.yDrive.target);
    //     }

    //     Debug.Log("done moving!");
    //     yield return null;
    // }

    bool CheckArrayWithinStandardDeviation(float[] values, float standardDeviation)
    {
        // Calculate the mean value of the array
        float mean = values.Average();

        // Calculate the sum of squares of the differences between each value and the mean
        float sumOfSquares = 0.0f;
        foreach (float value in values)
        {
            //Debug.Log(value);
            sumOfSquares += (value - mean) * (value - mean);
        }

        // Calculate the standard deviation of the array
        float arrayStdDev = (float)Mathf.Sqrt(sumOfSquares / values.Length);

        // Check if the standard deviation of the array is within the specified range
        return arrayStdDev <= standardDeviation;
    }

    bool CheckIfDistanceMagnitudesAreCloseToDistanceToMove(float distanceMovedSoFar, float distanceToMove, float epsilon)
    {
        var diff = Mathf.Abs(distanceToMove - distanceMovedSoFar);
        return diff <= epsilon || diff <= Mathf.Max(Mathf.Abs(distanceToMove), Mathf.Abs(distanceMovedSoFar)) * epsilon;
    }

    //this is called every fixed update
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

        if(myABArmControllerComponent.controlMode == ABControlMode.Actions)
        {
            ControlJointFromAction();
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
