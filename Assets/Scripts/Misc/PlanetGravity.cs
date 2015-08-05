using UnityEngine;
using System.Collections;

public class PlanetGravity : MonoBehaviour 
{
    public GameObject planet;
    public float gravitationalAcceleration = 6f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void FixedUpdate()
    {
        Vector3 offset = planet.transform.position - transform.position;
        float magSqr = offset.sqrMagnitude;
        if (magSqr > 0.0001f)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce((gravitationalAcceleration * offset.normalized / magSqr) * rb.mass);
        }
        //GetComponent<Rigidbody>().velocity += gravitationalAcceleration * Time.fixedTime * (planet.transform.position - transform.position);
    }
}
