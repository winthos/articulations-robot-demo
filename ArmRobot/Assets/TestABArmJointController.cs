using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmLiftState { Fixed = 0, MovingDown = 1, MovingUp = -1 };


public class TestABArmJointController : MonoBehaviour
{
    //default move state to fixed
    public ArmLiftState liftState = ArmLiftState.Fixed;
    public float speed = 1.0f;
    public ArticulationBody myAB;

    void Start() 
    {
        myAB = this.GetComponent<ArticulationBody>();
        //Debug.Log(myAB.linearLockX);
    }    

    private void FixedUpdate()
    {
        if (liftState != ArmLiftState.Fixed)
        {
            //get jointPosition along y axis
            float yDrivePostion = myAB.jointPosition[0];
            //Debug.Log(xDrivePostion);

            //increment this y position
            float targetPosition = yDrivePostion + -(float)liftState * Time.fixedDeltaTime * speed;

            //set joint Drive to new position
            var drive = myAB.yDrive;
            drive.target = targetPosition;
            myAB.yDrive = drive;
        }
    }
}
