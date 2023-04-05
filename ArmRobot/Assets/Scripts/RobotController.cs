using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RobotController : MonoBehaviour
{
    public ArticulationBody myAB;

    [System.Serializable]
    public struct Joint
    {
        public string inputAxis;
        public GameObject robotPart;
    }
    public Joint[] joints;

    void Start() 
    {
        myAB = this.GetComponent<ArticulationBody>();
        //Debug.Log(myAB.linearLockX);
    }

    void Update()
    {
        // manual input
        if(Input.GetKeyDown(KeyCode.Space)) {
            this.GetComponent<ArticulationBody>().AddForce(new Vector3(0, 1000, 0));
        }
    }

    // CONTROL

    public void StopAllJointRotations()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject robotPart = joints[i].robotPart;
            UpdateRotationState(RotationDirection.None, robotPart);
        }
    }

    public void RotateJoint(int jointIndex, RotationDirection direction)
    {
        StopAllJointRotations();
        Joint joint = joints[jointIndex];
        UpdateRotationState(direction, joint.robotPart);
    }

    // HELPERS

    static void UpdateRotationState(RotationDirection direction, GameObject robotPart)
    {
        ArticulationJointController jointController = robotPart.GetComponent<ArticulationJointController>();
        jointController.rotationState = direction;
    }



}
