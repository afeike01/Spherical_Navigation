using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridAgent : MonoBehaviour 
{

    public float movementSpeed = 5f;
    public float rotationSpeed = 25f;
    public bool okToMove = true;
    public List<Node> agentPath=new List<Node>();
    public Node currentNode;
    public Node lastNode;
    public SphereGrid navGrid;

    public float distFromNextNode;
    public float switchNodeDist = .5f;

    
    //private Unit unitComponent;

    private int surroundingNodeIndex = 0;
	// Use this for initialization
	void Start () 
    {
        Initialize();
	}
	
	
	void Update () 
    {
        UpdateMovement();   
	}
    
    private void Initialize()
    {
        //GameObject newFollowAgent = Instantiate(followAgentPrefab, transform.position, transform.rotation) as GameObject;
        //unitComponent = transform.root.GetComponentInChildren<Unit>();
        
        
    }
    private void UpdateMovement()
    {
        if (okToMove && agentPath.Count > 0)
        {
            currentNode = agentPath[0];
            distFromNextNode = Vector3.Distance(transform.position, currentNode.GetLocation());
            transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);

            Vector3 newPosition = currentNode.GetLocation() - transform.position;
            Quaternion newRotation = Quaternion.LookRotation(newPosition);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);

            if (distFromNextNode < switchNodeDist)
            {
                lastNode = agentPath[0];
                agentPath.RemoveAt(0);
                if (agentPath.Count < 1)
                {
                    ToggleOkToMove(false);
                }
            }
        }
    }
    
    private void ExecuteMove()
    {
        ToggleOkToMove(true);
        
    }
    /*public void GetPath(float newX, float newZ)
    {
        int xVal = (int)newX;
        int zVal = (int)newZ;
        agentPath.Clear();
        if (currentNode == null)
        {
            SetCurrentNode();
        }
        
        //Grid.FindPathImproved(navGrid, currentNode.xVal, currentNode.zVal, xVal, zVal, out agentPath);
        ExecuteMove();
    }
    public void GetPath(int xVal, int zVal)
    {
        agentPath.Clear();
        if(currentNode==null)
        {
            SetCurrentNode();
        }
        
        Grid.FindPathImproved(navGrid, currentNode.xVal, currentNode.zVal, xVal, zVal, out agentPath);
        ExecuteMove();
    }*/
    public void GetPath(Node newNode)
    {
        agentPath.Clear();
        
        /*Debug.Log("Current Location: ("+currentNode.xVal + ", " + currentNode.zVal+") Destination: ("+newNode.xVal+", "+newNode.zVal+")");
        return;*/
        //Grid.FindPathImproved(navGrid, currentNode.xVal, currentNode.zVal, newNode.xVal, newNode.zVal, out agentPath);
        ExecuteMove();
    }
    public void GetPath(List<Node> newPath)
    {
        agentPath.Clear();
        agentPath = newPath;
        ExecuteMove();
    }
    public void SetCurrentNode(Node newNode)
    {
        currentNode = newNode;
    }
    public void SetNavigationGrid(SphereGrid newGrid)
    {
        navGrid = newGrid;
    }
    public bool GetPathStatus()
    {
        if (agentPath.Count > 0)
            return true;
        else
            return false;
    }
    public void ToggleOkToMove(bool newVal)
    {
        okToMove = newVal;
    }
    /*public Node GetSurroundingNode()
    {
        if (currentNode == null)
        {
            SetCurrentNode();
        }
        if (currentNode != null)
        {
            Node newNode = null;
            int counter = 0;
            while (true)
            {
                surroundingNodeIndex = (currentNode.neighboors.Count > surroundingNodeIndex) ? surroundingNodeIndex : 0;
                newNode = currentNode.neighboors[surroundingNodeIndex];
                if (newNode != null && newNode.available)
                {
                    surroundingNodeIndex++;
                    break;
                }
                if (counter >= GameManager.MAX_NUM_LOOPS)
                    break;
                counter++;
                surroundingNodeIndex++;
            }
            if (newNode != null && newNode.available)
                return newNode;
            else
            {
                transform.root.GetComponent<Unit>().gameManager.DisplayDebugMessage("Unit GetSurroundingNode() ERROR!", Color.red);
                return null;
            }
        }
        else
        {
            transform.root.GetComponent<Unit>().gameManager.DisplayDebugMessage("Unit GetSurroundingNode() has returned null!", Color.yellow);
            return null;
        }
            
    }*/
}
