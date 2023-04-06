using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public bool shouldSpawn;
    public GameObject spawnedObject;
    public Transform spawnPosition;
    public float force = 100;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(spawn());
    }

    IEnumerator spawn() 
    {
        while (shouldSpawn)
        {
            GameObject spawned = GameObject.Instantiate(spawnedObject, spawnPosition.transform.position, spawnPosition.rotation);
            spawned.GetComponent<Rigidbody>().AddForce(spawned.transform.forward * force, ForceMode.Impulse);
            yield return new WaitForSeconds(1.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
