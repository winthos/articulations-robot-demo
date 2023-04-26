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
    //distance to move either in meters or radians?
    public float distance;
    public float speed;
    public float tolerance;
    public float maxTimePassed;
    public int positionCacheSize;

    //these are used during movement in fixed update
    public int direction;
    public float timePassed = 0.0f;
    public float[] cachedPositions;
    public int oldestCachedIndex;
    public float initialJointPosition;
}

public class TestABArmJointController : MonoBehaviour
{
    [Header("What kind of joint is this?")]
    public JointAxisType jointAxisType = JointAxisType.Unassigned;

    [Header("State of this joint's movements")]
    [SerializeField]
    private ArmRotateState rotateState = ArmRotateState.Idle;
    [SerializeField]
    private ArmLiftState liftState = ArmLiftState.Idle;
    [SerializeField]
    private ArmExtendState extendState = ArmExtendState.Idle;

    //pass in arm move parameters for Action based movement
    private ArmMoveParams currentArmMoveParams;

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

    public ArticulationBody myAB;
    public TestABArmController myABArmControllerComponent;

    void Start() 
    {
        myAB = this.GetComponent<ArticulationBody>();
        //Debug.Log(myAB.linearLockX);
    }    

    private void Update()
    {

    }

    //do all this stuff once before we start moving this body
    public void PrepToControlJointFromAction(ArmMoveParams armMoveParams)
    {
        if(Mathf.Approximately(armMoveParams.distance, 0.0f))
        {
            Debug.Log("Error! distance to move must be nonzero");
            return;
        }

        //we are a lift type joint, moving along the local y axis
        if(jointAxisType == JointAxisType.Lift)
        {
            if(liftState == ArmLiftState.Idle)
            {
                //set current arm move params to prep for movement in fixed update
                currentArmMoveParams = armMoveParams;

                //initialize the buffer to cache positions to check for later
                currentArmMoveParams.cachedPositions = new float[currentArmMoveParams.positionCacheSize];
                
                //snapshot the initial joint position to compare with later during movement
                currentArmMoveParams.initialJointPosition = myAB.jointPosition[0];

                //set if we are moving up or down based on sign of distance from input
                if(armMoveParams.direction < 0)
                {
                    Debug.Log("setting lift state to move down");
                    liftState = ArmLiftState.MovingDown;
                }

                else if(armMoveParams.direction > 0)
                {
                    Debug.Log("setting lift state to move up");
                    liftState = ArmLiftState.MovingUp;
                }
            }
        }

        //we are an extending joint, moving along the local z axis
        else if (jointAxisType == JointAxisType.Extend)
        {
            if(extendState == ArmExtendState.Idle)
            {
                //set current arm move params to prep for movement in fixed update
                currentArmMoveParams = armMoveParams;

                //initialize the buffer to cache positions to check for later
                currentArmMoveParams.cachedPositions = new float[currentArmMoveParams.positionCacheSize];
                
                //snapshot the initial joint position to compare with later during movement
                currentArmMoveParams.initialJointPosition = myAB.jointPosition[0];

                //set if we are moving up or down based on sign of distance from input
                if(armMoveParams.direction < 0)
                {
                    //Debug.Log("setting extend state to retracting");
                    extendState = ArmExtendState.MovingBackward;
                }

                if(armMoveParams.direction > 0)
                {
                    //Debug.Log("setting extend state to extending");
                    extendState = ArmExtendState.MovingForward;
                }
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
                //moving up/down so get the yDrive
                var drive = myAB.yDrive;
                float currentPosition = myAB.jointPosition[0];
                Debug.Log($"currentPosition: {currentPosition}");
                float targetPosition = currentPosition + (float)liftState * Time.fixedDeltaTime * currentArmMoveParams.speed;    
                drive.target = targetPosition;
                //this sets the drive to begin moving to the new target position
                myAB.yDrive = drive;     

                //begin checks to see if we have stopped moving or if we need to stop moving
                //cache the position at the moment
                currentArmMoveParams.cachedPositions[currentArmMoveParams.oldestCachedIndex] = currentPosition;

                //Debug.Log($"initialPosition: {currentArmMoveParams.initialJointPosition}");

                var distanceMovedSoFar = Mathf.Abs(currentPosition - currentArmMoveParams.initialJointPosition);
                //Debug.Log($"distance moved so far is: {distanceMovedSoFar}");

                //iterate next index in cache, loop back to index 0 as we get newer positions
                currentArmMoveParams.oldestCachedIndex = (currentArmMoveParams.oldestCachedIndex + 1) % currentArmMoveParams.positionCacheSize;

                //every time we loop back around the cached positions, check if we effectively stopped moving
                if(currentArmMoveParams.oldestCachedIndex == 0)
                {
                    //go ahead and update index 0 super quick so we don't miss it
                    currentArmMoveParams.cachedPositions[currentArmMoveParams.oldestCachedIndex] = currentPosition;
                    
                    //for the last {NumberOfCachedPositions} positions, and if that amount hasn't changed
                    //by the {tolerance} deviation then we have presumably stopped moving
                    if(CheckArrayWithinStandardDeviation(currentArmMoveParams.cachedPositions, currentArmMoveParams.tolerance))
                    {
                        Debug.Log($"last {currentArmMoveParams.positionCacheSize} positions were within tolerance, stop moving now!");
                        liftState = ArmLiftState.Idle;
                        //actionFinished(true) will go here
                        return;
                    }
                }

                if(distanceMovedSoFar >= currentArmMoveParams.distance)
                {
                    Debug.Log("we have moved to or a little beyond the distance specified to move so STOOOP");
                    liftState = ArmLiftState.Idle;
                    //actionFinished(true) will go here
                    return;
                }
                
                //otherwise we have a hard timer to stop movement so we don't move forever and crash unity
                currentArmMoveParams.timePassed += Time.deltaTime;

                if(currentArmMoveParams.timePassed >= currentArmMoveParams.maxTimePassed)
                {
                    Debug.Log($"{currentArmMoveParams.timePassed} seconds have passed. Time out happening, stop moving!");
                    liftState = ArmLiftState.Idle;
                    //actionFinished(true) will go here
                    return;
                }
            }

            //we are set to be in an idle state so return and do nothing
            return;
        }

        //for extending arm joints, don't set actionFinished here, instead we have a coroutine in the
        //TestABArmController component that will check each arm joint to see if all arm joints have either
        //finished moving their required distance, or if they have stopped moving, or if they have timed out
        else if(jointAxisType == JointAxisType.Extend)
        {
            if(extendState != ArmExtendState.Idle)
            {
                var drive = myAB.zDrive;
                float currentPosition = myAB.jointPosition[0];
                float targetPosition = currentPosition + (float)extendState * Time.fixedDeltaTime * currentArmMoveParams.speed;    
                drive.target = targetPosition;
                myAB.zDrive = drive;
            
                //begin checks to see if we have stopped moving or if we need to stop moving
                //cache the position at the moment
                currentArmMoveParams.cachedPositions[currentArmMoveParams.oldestCachedIndex] = currentPosition;

                //Debug.Log($"initialPosition: {currentArmMoveParams.initialJointPosition}");

                var distanceMovedSoFar = Mathf.Abs(currentPosition - currentArmMoveParams.initialJointPosition);
                //Debug.Log($"distance moved so far is: {distanceMovedSoFar}");

                //iterate next index in cache, loop back to index 0 as we get newer positions
                currentArmMoveParams.oldestCachedIndex = (currentArmMoveParams.oldestCachedIndex + 1) % currentArmMoveParams.positionCacheSize;

                //every time we loop back around the cached positions, check if we effectively stopped moving
                if(currentArmMoveParams.oldestCachedIndex == 0)
                {
                    //go ahead and update index 0 super quick so we don't miss it
                    currentArmMoveParams.cachedPositions[currentArmMoveParams.oldestCachedIndex] = currentPosition;
                    
                    //for the last {NumberOfCachedPositions} positions, and if that amount hasn't changed
                    //by the {tolerance} deviation then we have presumably stopped moving
                    if(CheckArrayWithinStandardDeviation(currentArmMoveParams.cachedPositions, currentArmMoveParams.tolerance))
                    {
                        Debug.Log($"tolerance reached, distance {myAB} moved so far: {distanceMovedSoFar}");
                        extendState = ArmExtendState.Idle;
                        return;
                    }
                }

                if(distanceMovedSoFar >= currentArmMoveParams.distance)
                {
                    Debug.Log($"max distance exceeded, distance {myAB} moved so far: {distanceMovedSoFar}");
                    extendState = ArmExtendState.Idle;
                    return;
                }
                
                //otherwise we have a hard timer to stop movement so we don't move forever and crash unity
                currentArmMoveParams.timePassed += Time.deltaTime;

                if(currentArmMoveParams.timePassed >= currentArmMoveParams.maxTimePassed)
                {
                    Debug.Log($"max time passed, distance {myAB} moved so far: {distanceMovedSoFar}");
                    extendState = ArmExtendState.Idle;
                    return;
                }
            }
        }
    }

    //check if all values in the array are within a standard deviation or not
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

    //maybe we don't need this but imma leave it here for now
    bool CheckIfDistanceMagnitudesAreCloseToDistanceToMove(float distanceMovedSoFar, float distanceToMove, float epsilon)
    {
        var diff = Mathf.Abs(distanceToMove - distanceMovedSoFar);
        return diff <= epsilon || diff <= Mathf.Max(Mathf.Abs(distanceToMove), Mathf.Abs(distanceMovedSoFar)) * epsilon;
    }

    private void FixedUpdate()
    { 
        ControlJointFromAction();
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
