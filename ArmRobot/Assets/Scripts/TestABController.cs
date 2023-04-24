using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MoveState {Idle = 0, Backward = -1, Forward = 1};

public enum RotateState {Idle = 0, Negative = -1, Positive = 1};

public class TestABController : MonoBehaviour
{
    public ArticulationBody ab;
    public List<ArticulationBody> abChildren = new List<ArticulationBody>();

    [Header("Speed for movement and rotation")]
    public float moveSpeed;
    public float rotateSpeed;
    private float move;
    private float rotate;

    [Header("Colliders used when moving or rotating")]
    public GameObject MoveColliders;
    public GameObject RotateColliders;

    [Header("Debug Move and Rotate states")]
    public MoveState moveState = MoveState.Idle;
    public RotateState rotateState = RotateState.Idle;
    
    public void Start()
    {
        
        //print(ab.centerOfMass);
        
        Debug.Log(ab.gameObject.name + "'s old centerOfMass is (" + ab.centerOfMass.x + ", " + ab.centerOfMass.y + ", " + ab.centerOfMass.z + ")");
        // ShiftChildCollidersToAB(ab, ab.transform);
        
        foreach (ArticulationBody abChild in abChildren) {
            // ShiftChildCollidersToAB(abChild, abChild.transform);
            abChild.centerOfMass = abChild.transform.InverseTransformPoint(ab.worldCenterOfMass);
            Debug.Log(abChild.gameObject.name + "'s new centerOfMass is (" + abChild.centerOfMass.x + ", " + abChild.centerOfMass.y + ", " + abChild.centerOfMass.z + ")");
        }

        ab.centerOfMass = Vector3.zero;
        Debug.Log(ab.gameObject.name + "'s new centerOfMass is (" + ab.centerOfMass.x + ", " + ab.centerOfMass.y + ", " + ab.centerOfMass.z + ")");

        ab.TeleportRoot(new Vector3(3.466904f, -0.01618071f, 30f), Quaternion.Euler(0, 90f, 0));
    }

    public void OnMove(InputAction.CallbackContext context) 
    {
        move = context.ReadValue<float>();
        Debug.Log(move);
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotate = context.ReadValue<float>();
        Debug.Log(rotate);
    }

    private void FixedUpdate() 
    {
        Move();
        //turn
        ab.AddTorque(Vector3.up * rotate * rotateSpeed);
    }

    void LateUpdate() 
    {
        
    }

    void Move()
    {
        //find target velocity
        Vector3 currentVelocity = ab.velocity;
        Vector3 targetvelocity = new Vector3(0, 0, move);
        targetvelocity *= moveSpeed;

        //align direction
        targetvelocity = transform.TransformDirection(targetvelocity);

        //calculate forces
        Vector3 velocityChange = (targetvelocity - currentVelocity);
        if(rotateState == RotateState.Idle && moveState != MoveState.Idle)
        {
            // Vector3 forcePosition = new Vector3();

            //prep to apply noise to forward direction if flagged to do so
            GameObject targetObject = null;
            
            if(moveState == MoveState.Forward)
            {
                // forcePosition = forceTarget.transform.position;
                // targetObject = forceTarget;
            }

            else if(moveState == MoveState.Backward)
            {
                // forcePosition = forceTargetBack.transform.position;
                // targetObject = forceTargetBack;
            }
            
            // if(applyActionNoise && targetObject != null)
            // {
            //     // float dirRandom = Random.Range(-movementGaussian, movementGaussian);
            //     // targetObject.transform.Rotate(0, dirRandom, 0);
            // }

            targetvelocity = new Vector3(0, 0, move);
            targetvelocity *= moveSpeed;

            //allign direction
            targetvelocity = transform.TransformDirection(targetvelocity);

            //calculate forces
            Debug.Log(Time.fixedDeltaTime);

            ab.centerOfMass = Vector3.zero;

            ab.AddForce(velocityChange);
        }
    }

    void Rotate()
    {
        if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
        {
            // float rotateAmount;

            // if(applyActionNoise)
            // {
            //     float rotRandom = Random.Range(-rotateGaussian, rotateGaussian);
            //     rotateAmount = rotate + rotRandom;
            // }

            // else
            // {
            //     rotateAmount = rotate;
            // }

            //Debug.Log(rotateAmount);

            if(rotateState != RotateState.Idle && moveState == MoveState.Idle)
            {

                ab.centerOfMass = Vector3.zero;
                foreach (ArticulationBody abChild in abChildren) {
                    abChild.centerOfMass = abChild.transform.InverseTransformPoint(ab.worldCenterOfMass);
                    Debug.Log(abChild.gameObject.name + "'s new centerOfMass is (" + abChild.centerOfMass.x + ", " + abChild.centerOfMass.y + ", " + abChild.centerOfMass.z + ")");
                }
                
                //note 2021 unity adds a forceMode paramater to AddTorque so uhhhh
                ab.AddTorque(Vector3.up * rotate * rotateSpeed);
            }
        }
    }
    public void ShiftChildCollidersToAB(ArticulationBody ab, Transform parent)
    {
        Transform child;
        int children = parent.childCount;
        Debug.Log(children);
        for (int i = 0; i < children; i++) {
            child = parent.GetChild(i);
            if (child.GetComponents<Collider>() != null && child.GetComponent<ArticulationBody>() == null) {
                foreach (Component original in child.GetComponents<Collider>()) {
                    System.Type type = original.GetType();
                    Debug.Log(original + " is type " + type);
                    Component copy = ab.gameObject.AddComponent(type);

                    // Copied fields can be restricted with BindingFlags
                    System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default;
                    System.Reflection.PropertyInfo[] properties = type.GetProperties(bindingFlags);
                    Debug.Log("LALALA " + properties.Length);
                    foreach (System.Reflection.PropertyInfo property in properties)
                    {
                        if (property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0 && property.SetMethod != null)
                        {   
                            // offset center to realign it to AB GameObject
                            if (property.Name == "center") {
                                Vector3 newCenter = (Vector3)property.GetValue(original) + ab.transform.InverseTransformPoint(child.position);
                                property.SetValue(copy, newCenter);
                            } else {
                                property.SetValue(copy, property.GetValue(original));
                            }
                        }
                    }

                    //original.GetComponent<Center>() = 

                    // DestroyImmediate(sourceCollider);
                }

                if (child.childCount != 0) {
                    ShiftChildCollidersToAB(ab, child);
                }
            }
        }
            
        return;
    }

    public void UpdateCollidersForMovement()
    {
        if(MoveColliders == null)
        {
            return;
        }

        if(RotateColliders == null)
        {
            return;
        }

        //moving forward/backward
        if((moveState == MoveState.Forward || moveState == MoveState.Backward) && rotateState == RotateState.Idle)
        {
            if(MoveColliders.activeSelf == false)
            MoveColliders.SetActive(true);

            if(RotateColliders.activeSelf == true)
            RotateColliders.SetActive(false);
        }

        //rotating
        else if ((rotateState == RotateState.Positive || rotateState == RotateState.Negative) && moveState == MoveState.Idle)
        {
            if(MoveColliders.activeSelf == true)
            MoveColliders.SetActive(false);

            if(RotateColliders.activeSelf == false)
            RotateColliders.SetActive(true);
        }
    }

}
