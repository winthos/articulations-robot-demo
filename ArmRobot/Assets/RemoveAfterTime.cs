using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAfterTime : MonoBehaviour
{
    public float timeTillRemove = 10f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, timeTillRemove);
    }

    void Update()
    {

    }
}
