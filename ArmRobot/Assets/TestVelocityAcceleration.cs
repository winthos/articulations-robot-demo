using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestVelocityAcceleration : MonoBehaviour
{
    public float distance = 10f;
    public float maxSpeed = 1f;
    public float acceleration = 1f;
    private float accelerationDistance, accelerationTime;
    private Vector3 initialPosition, finalPosition, currentPosition;
    private float timePassed = 0.0f;
    public ArticulationBody myAB;

    [SerializeField]
    public MoveState agentState = MoveState.Idle;
    public RotateState rotateState = RotateState.Idle;

    // Start is called before the first frame update
    void Start()
    {
        myAB.centerOfMass = Vector3.zero;
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

        initialPosition = myAB.transform.position;
        finalPosition = myAB.transform.TransformPoint(Vector3.forward * distance);

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
            agentState = MoveState.Forward;
        }

        else if(direction < 0)
        {
            Debug.Log("setting agent state to move backward");
            agentState = MoveState.Backward;
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
        currentPosition = myAB.transform.position;
        //if we are moving forward or backward actively
        if(agentState != MoveState.Idle)
        {
            Vector3 currentPosition = myAB.transform.position;
            // Debug.Log($"position of agent: {currentPosition}");
            
            Vector3 forceDirection = new Vector3(0,0,acceleration);
            
            if (finalPosition.magnitude - currentPosition.magnitude < 1e-3f) {
                myAB.AddForce(myAB.mass * Vector3.back * myAB.velocity.magnitude * Time.fixedDeltaTime);
                agentState = MoveState.Idle;
                Debug.Log("STOP!");
            } 

            // Apply acceleration over acceleration-time
            if (timePassed < accelerationTime) {
                myAB.AddForce(myAB.mass * forceDirection);
                Debug.Log("Accelerating!");
            }

            if (accelerationDistance >= (finalPosition - currentPosition).magnitude) {
                myAB.AddForce(myAB.mass * -forceDirection);
                Debug.Log("Decelerating!");
            }

            timePassed += Time.fixedDeltaTime;
        }
    }
}
