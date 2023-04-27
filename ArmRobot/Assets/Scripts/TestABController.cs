using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RandomExtensions;

public enum ABControlMode {Keyboard_Input, Actions};
//this tests controlling the body moving with force
public enum MoveState {Idle = 0, Backward = -1, Forward = 1};
public enum RotateState {Idle = 0, Negative = -1, Positive = 1};
// public class AgentParams
// {
//     //distance to move either in meters or radians?
//     public float distance;
//     public float maxVelocity;
//     public float acceleration;
//     public float accelerationTime;
//     //these are used during movement in fixed update
//     public int direction;
//     public float timePassed = 0.0f;
// }

public static class PretendToBeInTHOR
{
    public static void actionFinished(bool result)
    {
        Debug.Log($"Action Finished: {result}!");
    }
}

public class TestABController : MonoBehaviour
{
    [SerializeField]
    [Header("Control with Keyboard or Action")]
    ABControlMode controlMode = ABControlMode.Keyboard_Input;

    [Header("Reference to this Articulation Body")]
    public ArticulationBody ab;
    public List<ArticulationBody> abChildren = new List<ArticulationBody>();

    [Header("Speed for movement and rotation")]
    public float moveSpeed;
    public float rotateSpeed;
    private float move;
    private float rotate;

    [Header("Debug Move and Rotate states")]
    public MoveState moveState = MoveState.Idle;
    public RotateState rotateState = RotateState.Idle;
    
    public void Start()
    {
        print(ab.centerOfMass);

        //print(ab.centerOfMass);
        
        Debug.Log(ab.gameObject.name + "'s old centerOfMass is (" + ab.centerOfMass.x + ", " + ab.centerOfMass.y + ", " + ab.centerOfMass.z + ")");
        
        ab.centerOfMass = Vector3.zero + new Vector3(0,0,0);
        Debug.Log(ab.gameObject.name + "'s new centerOfMass is (" + ab.centerOfMass.x + ", " + ab.centerOfMass.y + ", " + ab.centerOfMass.z + ")");

        foreach (ArticulationBody abChild in abChildren) {
            abChild.centerOfMass = abChild.transform.InverseTransformPoint(ab.worldCenterOfMass);
            Debug.Log(abChild.gameObject.name + "'s new centerOfMass is (" + abChild.worldCenterOfMass.x + ", " + abChild.worldCenterOfMass.y + ", " + abChild.worldCenterOfMass.z + ")");
        }
        // ab.TeleportRoot(new Vector3(3.466904f, 0f, 30f), Quaternion.Euler(0, 90f, 0));
    }

    // public void MoveForward (float distance, float speed, float tolerance, float maxTimePassed)
    // {
    //     Move(                    
    //         distance: distance,
    //         speed: speed,
    //         tolerance: tolerance,
    //         maxTimePassed: maxTimePassed,
    //         direction: 1 //going up
    //     );
    // }

    // public void MoveArmBaseDown (float distance, float speed, float tolerance, float maxTimePassed)
    // {
    //     Move(                    
    //         distance: distance,
    //         speed: speed,
    //         tolerance: tolerance,
    //         maxTimePassed: maxTimePassed,
    //         direction: -1 //going down
    //     );
    // }

    //actually send the arm parameters to the joint moving up/down and begin movement
    // public void Move(float distance = 0.25f, float speed = 3.0f, float tolerance = 1e-3f, float maxTimePassed = 5.0f, int direction = 1)
    // {
    //     //create a set of movement params for how we are about to move
    //     ArmMoveParams amp = new ArmMoveParams{
    //         distance = distance,
    //         speed = speed,
    //         tolerance = tolerance,
    //         maxTimePassed = maxTimePassed,
    //         direction = direction 
    //     };

    //     if(Mathf.Approximately(distance, 0.0f))
    //     {
    //         Debug.Log("Error! distance to move must be nonzero");
    //         return;
    //     }

    //     if(moveState == MoveState.Idle)
    //     {
    //         //set current arm move params to prep for movement in fixed update
    //         currentArmMoveParas = armMoveParams;

    //         //initialize the buffer to cache positions to check for later
    //         currentArmMoveParams.cachedPositions = new float[currentArmMoveParams.positionCacheSize];
            
    //         //snapshot the initial joint position to compare with later during movement
    //         currentArmMoveParams.initialJointPosition = myAB.jointPosition[0];

    //         //set if we are moving up or down based on sign of distance from input
    //         if(armMoveParams.direction < 0)
    //         {
    //             Debug.Log("setting lift state to move down");
    //             liftState = ArmLiftState.MovingDown;
    //         }

    //         else if(armMoveParams.direction > 0)
    //         {
    //             Debug.Log("setting lift state to move up");
    //             liftState = ArmLiftState.MovingUp;
    //         }
    // }


    public void OnMove(InputAction.CallbackContext context) 
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        move = context.ReadValue<float>();
        SetMoveState(MoveStateFromInput(move));
    }

    private void SetMoveState (MoveState state) 
    {
        moveState = state;
    }

    MoveState MoveStateFromInput (float input)
    {
        if (input > 0)
        {
            return MoveState.Forward;
        }
        else if (input < 0)
        {
            return MoveState.Backward;
        }
        else
        {
            return MoveState.Idle;
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if(controlMode != ABControlMode.Keyboard_Input) 
        {
            return;
        }

        rotate = context.ReadValue<float>();
        SetRotateState(RotateStateFromInput(rotate));
    }

    private void SetRotateState(RotateState state)
    {
        rotateState = state;
    }

    RotateState RotateStateFromInput (float input)
    {
        if (input > 0)
        {
            return RotateState.Positive;
        }
        else if (input < 0)
        {
            return RotateState.Negative;
        }
        else
        {
            return RotateState.Idle;
        }    
    }

    private void FixedUpdate() 
    {
        Move();
        Rotate();
        // baseFloorCollider.transform.eulerAngles = new Vector3(0, baseFloorCollider.transform.eulerAngles.y, 0);
    }

    private void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     StartCoroutine(MoveDistanceForward(1.0f));
        // }
    }

    void Move()
    {
        if(rotateState == RotateState.Idle && moveState != MoveState.Idle)
        {
            //find target velocity
            Vector3 currentVelocity = ab.velocity;

            Vector3 targetvelocity = new Vector3(0, 0, move);
            targetvelocity *= moveSpeed;

            //allign direction
            targetvelocity = transform.TransformDirection(targetvelocity);

            //calculate forces
            Vector3 velocityChange = (targetvelocity - currentVelocity); //this is F = mvt I think?
            //Debug.Log(Time.fixedDeltaTime);

            // ab.centerOfMass = Vector3.zero;
            // foreach (ArticulationBody abChild in abChildren) {
            //     abChild.centerOfMass = abChild.transform.InverseTransformPoint(ab.worldCenterOfMass);
            //     Debug.Log(abChild.gameObject.name + "'s new centerOfMass is (" + abChild.worldCenterOfMass.x + ", " + abChild.worldCenterOfMass.y + ", " + abChild.worldCenterOfMass.z + ")");
            // }

            ab.AddForce(velocityChange);
        }
    }

    void Rotate()
    {
        if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
        {
            float rotateAmount = rotate;
            //Debug.Log(rotateAmount);

            if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
            {
                //note 2021 unity adds a forceMode paramater to AddTorque so uhhhh
                ab.AddTorque(Vector3.up * rotateAmount * rotateSpeed);
            }
        }
    }
}
