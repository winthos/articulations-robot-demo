using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmLiftState { Fixed = 0, MovingDown = -1, MovingUp = 1 };
public enum ArmMoveState {Fixed = 0, MovingBackward = -1, MovingForward = 1};
public enum JointAxisType {X, Y, Z};
public class TestABArmJointController : MonoBehaviour
{
    public JointAxisType jointAxisType = JointAxisType.Y;
    public ArmLiftState liftState = ArmLiftState.Fixed;
    public ArmMoveState moveState = ArmMoveState.Fixed;
    public float speed = 1.0f;
    public ArticulationBody myAB;

    void Start() 
    {
        myAB = this.GetComponent<ArticulationBody>();
        //Debug.Log(myAB.linearLockX);
    }    

    private void FixedUpdate()
    {
        if(jointAxisType == JointAxisType.Y) {
            if (liftState != ArmLiftState.Fixed)
            {
                //get jointPosition along y axis
                float yDrivePostion = myAB.jointPosition[0];
                //Debug.Log(xDrivePostion);

                //increment this y position
                float targetPosition = yDrivePostion + (float)liftState * Time.fixedDeltaTime * speed;

                //set joint Drive to new position
                var drive = myAB.yDrive;
                drive.target = targetPosition;
                myAB.yDrive = drive;
            }
        }

        else if(jointAxisType == JointAxisType.Z) {
            if(moveState != ArmMoveState.Fixed) 
            {
                //get jointPosition along y axis
                float zDrivePostion = myAB.jointPosition[0];
                //Debug.Log(xDrivePostion);

                //increment this y position
                float targetPosition = zDrivePostion + (float)moveState * Time.fixedDeltaTime * speed;

                //set joint Drive to new position
                var drive = myAB.zDrive;
                drive.target = targetPosition;
                myAB.zDrive = drive;
            }
        }
    }
}
