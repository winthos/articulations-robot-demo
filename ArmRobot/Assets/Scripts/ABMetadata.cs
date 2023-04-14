using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABMetadata : MonoBehaviour
{

    ArticulationBody articulationBody;
    float stiffness, damping;
    Vector3 drivePosition, targetPosition, driveVelocity, targetVelocity;

    //List<float> driveForces = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        articulationBody = this.GetComponent<ArticulationBody>();
        stiffness = articulationBody.xDrive.stiffness;
        damping = articulationBody.xDrive.damping;
        //drivePosition = new Vector3(articulationBody.jointPosition, articulationBody.jointPosition, articulationBody.jointPosition);;

    }

    // Update is called once per frame
    void Update()
    {
        // Read the joint force from the ArticulationBody
        // articulationBody.GetJointForces(driveForces);
        // Access the individual force components
        // float jointForceX = jointForce.x;
        // float jointForceY = jointForce.y;
        // float jointForceZ = jointForce.z;

        // Print the joint force components
        Debug.Log("Joint Force X: " + articulationBody.jointForce[0]);
        // Debug.Log("Joint Force Y: " + jointForceY);
        // Debug.Log("Joint Force Z: " + jointForceZ);
    }
}
