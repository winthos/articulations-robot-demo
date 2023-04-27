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
    
    public float distance = 10f;
    public float maxSpeed = 1f;
    public float acceleration = 1f;
    private float accelerationDistance, accelerationTime;
    private Vector3 initialPosition, finalPosition, currentPosition;
    private float timePassed = 0.0f;

    // Start is called before the first frame update
    public void Start()
    {
        print(ab.centerOfMass);

        //print(ab.centerOfMass);
        
        // Debug.Log(ab.gameObject.name + "'s old centerOfMass is (" + ab.centerOfMass.x + ", " + ab.centerOfMass.y + ", " + ab.centerOfMass.z + ")");
        
        ab.centerOfMass = Vector3.zero + new Vector3(0,0,0);
        // Debug.Log(ab.gameObject.name + "'s new centerOfMass is (" + ab.centerOfMass.x + ", " + ab.centerOfMass.y + ", " + ab.centerOfMass.z + ")");

        foreach (ArticulationBody abChild in abChildren) {
            abChild.centerOfMass = abChild.transform.InverseTransformPoint(ab.worldCenterOfMass);
            // Debug.Log(abChild.gameObject.name + "'s new centerOfMass is (" + abChild.worldCenterOfMass.x + ", " + abChild.worldCenterOfMass.y + ", " + abChild.worldCenterOfMass.z + ")");
        }
        // ab.TeleportRoot(new Vector3(3.466904f, 0f, 30f), Quaternion.Euler(0, 90f, 0));
    }

    //Server Action format to move the base of the arm up
    public void MoveAgentForward (float distance, float maxSpeed, float acceleration)
    {
        MoveAgent(                    
            distance: distance,
            maxSpeed: maxSpeed,
            acceleration: acceleration,
            direction: 1 //going up
        );
    }

    //server action format to move the base of the arm down
    public void MoveAgentBackward (float distance, float maxSpeed, float acceleration)
    {
        MoveAgent(
            distance: distance,
            maxSpeed: maxSpeed,
            acceleration: acceleration,
            direction: -1 //going down
        );
    }

    //actually send the arm parameters to the joint moving up/down and begin movement
    public void MoveAgent(float distance = 10.0f, float maxSpeed = 1.0f, float acceleration = 1.0f, float direction = 1)
    {
        if(Mathf.Approximately(distance, 0.0f))
        {
            Debug.Log("Error! distance to move must be nonzero");
            return;
        }

        initialPosition = ab.transform.position;
        finalPosition = ab.transform.TransformPoint(Vector3.forward * distance);

        // determine if agent can even accelerate to max velocity and decelerate to 0 before reaching target position
        accelerationDistance = Mathf.Pow(maxSpeed,2) / (2 * acceleration);

        if (2 * accelerationDistance > distance) {
            maxSpeed = Mathf.Sqrt(distance * acceleration);
        }

        accelerationTime = maxSpeed / acceleration;



        //set if we are moving up or down based on sign of distance from input
        if(direction > 0)
        {
            Debug.Log("setting agent state to move forward");
            moveState = MoveState.Forward;
        }

        else if(direction < 0)
        {
            Debug.Log("setting agent state to move backward");
            moveState = MoveState.Backward;
        }
    }

    //use R and F keys to move agent forward and backward
    public void OnMoveAgent(InputAction.CallbackContext context)
    {
        if(context.started == true)
        {
            if(MoveStateFromInput(context.ReadValue<float>()) == MoveState.Forward)
            {
                //these parameters here act as if a researcher has put them in as an action
                MoveAgentForward(
                    distance: 10f,
                    maxSpeed: 2f,
                    acceleration: 1f
                );
            }

            else if(MoveStateFromInput(context.ReadValue<float>()) == MoveState.Backward)
            {
                //these parameters here act as if a researcher has put them in as an action
                MoveAgentBackward(
                    distance: 10f,
                    maxSpeed: 2f,
                    acceleration: 1f
                );
            }
        }
    }

    //reads input from the Player Input component to agent up and down along its local Z axis
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

    void FixedUpdate()
    {
        ControlAgentFromAction();
    }

    public void ControlAgentFromAction()
    {
        currentPosition = ab.transform.position;
        //if we are moving forward or backward actively
        if(moveState != MoveState.Idle)
        {
            Vector3 currentPosition = ab.transform.position;
            // Debug.Log($"position of agent: {currentPosition}");
            
            Vector3 forceDirection = new Vector3(0,0,acceleration);
            
            if (finalPosition.magnitude - currentPosition.magnitude < 1e-3f) {
                ab.AddForce(ab.mass * Vector3.back * ab.velocity.magnitude * Time.fixedDeltaTime);
                moveState = MoveState.Idle;
                Debug.Log("STOP!");
            } 

            // Apply acceleration over acceleration-time
            if (timePassed < accelerationTime) {
                ab.AddForce(ab.mass * forceDirection);
                Debug.Log("Accelerating!");
            }

            if (accelerationDistance >= (finalPosition - currentPosition).magnitude) {
                ab.AddForce(ab.mass * -forceDirection);
                Debug.Log("Decelerating!");
            }

            timePassed += Time.fixedDeltaTime;
        }
    }

    // // public void MoveForward (float distance, float speed, float tolerance, float maxTimePassed)
    // // {
    // //     Move(                    
    // //         distance: distance,
    // //         speed: speed,
    // //         tolerance: tolerance,
    // //         maxTimePassed: maxTimePassed,
    // //         direction: 1 //going up
    // //     );
    // // }

    // // public void MoveArmBaseDown (float distance, float speed, float tolerance, float maxTimePassed)
    // // {
    // //     Move(                    
    // //         distance: distance,
    // //         speed: speed,
    // //         tolerance: tolerance,
    // //         maxTimePassed: maxTimePassed,
    // //         direction: -1 //going down
    // //     );
    // // }

    // //actually send the arm parameters to the joint moving up/down and begin movement
    // // public void Move(float distance = 0.25f, float speed = 3.0f, float tolerance = 1e-3f, float maxTimePassed = 5.0f, int direction = 1)
    // // {
    // //     //create a set of movement params for how we are about to move
    // //     ArmMoveParams amp = new ArmMoveParams{
    // //         distance = distance,
    // //         speed = speed,
    // //         tolerance = tolerance,
    // //         maxTimePassed = maxTimePassed,
    // //         direction = direction 
    // //     };

    // //     if(Mathf.Approximately(distance, 0.0f))
    // //     {
    // //         Debug.Log("Error! distance to move must be nonzero");
    // //         return;
    // //     }

    // //     if(moveState == MoveState.Idle)
    // //     {
    // //         //set current arm move params to prep for movement in fixed update
    // //         currentArmMoveParas = armMoveParams;

    // //         //initialize the buffer to cache positions to check for later
    // //         currentArmMoveParams.cachedPositions = new float[currentArmMoveParams.positionCacheSize];
            
    // //         //snapshot the initial joint position to compare with later during movement
    // //         currentArmMoveParams.initialJointPosition = ab.jointPosition[0];

    // //         //set if we are moving up or down based on sign of distance from input
    // //         if(armMoveParams.direction < 0)
    // //         {
    // //             Debug.Log("setting lift state to move down");
    // //             liftState = ArmLiftState.MovingDown;
    // //         }

    // //         else if(armMoveParams.direction > 0)
    // //         {
    // //             Debug.Log("setting lift state to move up");
    // //             liftState = ArmLiftState.MovingUp;
    // //         }
    // // }


    // public void OnMove(InputAction.CallbackContext context) 
    // {
    //     if(controlMode != ABControlMode.Keyboard_Input) 
    //     {
    //         return;
    //     }

    //     move = context.ReadValue<float>();
    //     SetMoveState(MoveStateFromInput(move));
    // }

    // private void SetMoveState (MoveState state) 
    // {
    //     moveState = state;
    // }

    // MoveState MoveStateFromInput (float input)
    // {
    //     if (input > 0)
    //     {
    //         return MoveState.Forward;
    //     }
    //     else if (input < 0)
    //     {
    //         return MoveState.Backward;
    //     }
    //     else
    //     {
    //         return MoveState.Idle;
    //     }
    // }

    // public void OnRotate(InputAction.CallbackContext context)
    // {
    //     if(controlMode != ABControlMode.Keyboard_Input) 
    //     {
    //         return;
    //     }

    //     rotate = context.ReadValue<float>();
    //     SetRotateState(RotateStateFromInput(rotate));
    // }

    // private void SetRotateState(RotateState state)
    // {
    //     rotateState = state;
    // }

    // RotateState RotateStateFromInput (float input)
    // {
    //     if (input > 0)
    //     {
    //         return RotateState.Positive;
    //     }
    //     else if (input < 0)
    //     {
    //         return RotateState.Negative;
    //     }
    //     else
    //     {
    //         return RotateState.Idle;
    //     }    
    // }

    // private void FixedUpdate() 
    // {
    //     Move();
    //     Rotate();
    //     // baseFloorCollider.transform.eulerAngles = new Vector3(0, baseFloorCollider.transform.eulerAngles.y, 0);
    // }

    // private void Update()
    // {
    //     // if(Input.GetKeyDown(KeyCode.Space))
    //     // {
    //     //     StartCoroutine(MoveDistanceForward(1.0f));
    //     // }
    // }

    // void Move()
    // {
    //     if(rotateState == RotateState.Idle && moveState != MoveState.Idle)
    //     {
    //         //find target velocity
    //         Vector3 currentVelocity = ab.velocity;

    //         Vector3 targetvelocity = new Vector3(0, 0, move);
    //         targetvelocity *= moveSpeed;

    //         //allign direction
    //         targetvelocity = transform.TransformDirection(targetvelocity);

    //         //calculate forces
    //         Vector3 velocityChange = (targetvelocity - currentVelocity); //this is F = mvt I think?
    //         //Debug.Log(Time.fixedDeltaTime);

    //         // ab.centerOfMass = Vector3.zero;
    //         // foreach (ArticulationBody abChild in abChildren) {
    //         //     abChild.centerOfMass = abChild.transform.InverseTransformPoint(ab.worldCenterOfMass);
    //         //     Debug.Log(abChild.gameObject.name + "'s new centerOfMass is (" + abChild.worldCenterOfMass.x + ", " + abChild.worldCenterOfMass.y + ", " + abChild.worldCenterOfMass.z + ")");
    //         // }

    //         ab.AddForce(velocityChange);
    //     }
    // }

    // void Rotate()
    // {
    //     if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
    //     {
    //         float rotateAmount = rotate;
    //         //Debug.Log(rotateAmount);

    //         if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
    //         {
    //             //note 2021 unity adds a forceMode paramater to AddTorque so uhhhh
    //             ab.AddTorque(Vector3.up * rotateAmount * rotateSpeed);
    //         }
    //     }
    // }
}
