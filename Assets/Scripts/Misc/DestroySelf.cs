using UnityEngine;
using System.Collections;

public class DestroySelf : MonoBehaviour 
{
    public float lifeTime = 1.5f;

	// Use this for initialization
	void Start () 
    {
        Destroy(this.gameObject, lifeTime);
	}
	
	
}
