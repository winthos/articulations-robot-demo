using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RandomExtensions;

public enum MoveState {Idle = 0, Backward = -1, Forward = 1};

public enum RotateState {Idle = 0, Negative = -1, Positive = 1};

public class TestABController : MonoBehaviour
{
    //use this to set global random object
    protected static System.Random systemRandom = new System.Random();

    public bool applyActionNoise = false;
    public float movementGaussian = 0.1f;
    public float rotateGaussian = 0.1f;

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
            //prep to apply noise to forward direction if flagged to do so            
            if(applyActionNoise == true)
            {
                // Add noise stuff here when time comes
            }

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
            float rotateAmount;

            if(applyActionNoise)
            {
                float rotRandom = Random.Range(-rotateGaussian, rotateGaussian);
                rotateAmount = rotate + rotRandom;
            }

            else
            {
                rotateAmount = rotate;
            }

            //Debug.Log(rotateAmount);

            if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
            {
                //note 2021 unity adds a forceMode paramater to AddTorque so uhhhh
                ab.AddTorque(Vector3.up * rotateAmount * rotateSpeed);
            }
        }
    }
}
