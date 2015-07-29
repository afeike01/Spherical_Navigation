using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Testing : MonoBehaviour 
{
    private const int SIZE = 20;
    private const int STEP_SIZE = 1;
    private const float RADIUS = 5;

    private List<Vector3> cubeCoordinates = new List<Vector3>();
    private List<Vector3> sphereCoordinates = new List<Vector3>();

    public GameObject visualPrefab;
	// Use this for initialization
	void Start () 
    {
        GenerateCubePoints();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    private void GenerateCubePoints()
    {
        int minVal = -(SIZE / 2);
        int maxVal = SIZE / 2;

        //Front Face
        for (int i = minVal; i <= maxVal; i++)
        {
            for (int j = minVal; j <= maxVal; j++)
            {
                cubeCoordinates.Add(new Vector3(i, j, minVal));
            }
        }
        //Back Face
        for (int i = minVal; i <= maxVal; i++)
        {
            for (int j = minVal; j <= maxVal; j++)
            {
                cubeCoordinates.Add(new Vector3(i, j, maxVal));
            }
        }
        //Left Face
        for (int i = minVal; i <= maxVal; i++)
        {
            for (int j = minVal; j <= maxVal; j++)
            {
                cubeCoordinates.Add(new Vector3(minVal,i,j));
            }
        }
        //Right Face
        for (int i = minVal; i <= maxVal; i++)
        {
            for (int j = minVal; j <= maxVal; j++)
            {
                cubeCoordinates.Add(new Vector3(maxVal, i, j));
            }
        }
        //Top Face
        for (int i = minVal; i <= maxVal; i++)
        {
            for (int j = minVal; j <= maxVal; j++)
            {
                cubeCoordinates.Add(new Vector3(i, maxVal, j));
            }
        }
        //Bottom Face
        for (int i = minVal; i <= maxVal; i++)
        {
            for (int j = minVal; j <= maxVal; j++)
            {
                cubeCoordinates.Add(new Vector3(i, minVal, j));
            }
        }
        for (int i = 0; i < cubeCoordinates.Count; i++)
        {
            Vector3 newSphereCoordinate = CubeToSphereCoordinates(cubeCoordinates[i]);
            sphereCoordinates.Add(CubeToSphereCoordinates(cubeCoordinates[i]));
            GameObject newVisual = Instantiate(visualPrefab, sphereCoordinates[i], Quaternion.identity) as GameObject;
        }

    }
    private Vector3 CubeToSphereCoordinates(Vector3 newLocation)
    { 
        Vector3 newPoint = newLocation.normalized*RADIUS;
        return newPoint;
    }
}
