using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum NodeType : int
{
    Temporary =-1,
    Normal =0,
    Abstract = 1,
}
public class Node : IComparable<Node>
{
    public float xVal = 0;
    public float yVal = 0;
    public float zVal = 0;
    public bool available = true;

    public int nodeNum;

    public List<Node> neighbors = new List<Node>();
    public float f = 0;
    public float g = 0;
    public float h = 0;
    public Node previouseNode;
    public bool visited = false;

    public Grid gridParent;
    public NodeCluster clusterParent;

    public List<Node> nodesConnectingTo;

    public NodeType level;

    private bool isTemporary = false;

    public Vector3 gridCoordinates;
    public Vector3 cubeCoordinates;
    public Vector3 sphereCoordinates;

    public Node(Grid newGrid, Vector3 location, NodeType nodeLevel, int num)
    {
        this.gridParent = newGrid;
        this.level = nodeLevel;
        this.xVal = location.x;
        this.yVal = location.y;
        this.zVal = location.z;
        this.nodeNum = num;

        gridCoordinates = location;
        cubeCoordinates = SphereGrid.GridToCubeCoordinates(this);
        sphereCoordinates = SphereGrid.CubeToSphereCoordinates(cubeCoordinates);
    }
    public int CompareTo(Node newNode)
    {
        if (this.f > newNode.f)
            return 1;
        if (this.f < newNode.f)
            return -1;
        else
            return 0;
    }
    public void AddNeighbor(Node newNeighbor)
    {
        if(newNeighbor!=null&&!neighbors.Contains(newNeighbor))
        {
            neighbors.Add(newNeighbor);
        }
    }
    
    public Vector3 GetLocation()
    {
        return new Vector3(xVal, yVal, zVal);
    }
    public void SetLocation(Vector3 newLocation)
    {
        xVal = newLocation.x;
        yVal = newLocation.y;
        zVal = newLocation.z;
    }
    public void ToggleVisited(bool newVal)
    {
        visited = newVal;
    }
    public void ToggleAvailable(bool newVal)
    {
        available = newVal;
        
    }
    public void SetF()
    {
        f = g + h;
    }
    public void SetG(float newG)
    {
        g = newG;
    }
    public void SetH(Node goalNode)
    {
        //TESTING
        Vector3 start = cubeCoordinates;//new Vector3(xVal, yVal, zVal);
        Vector3 end = goalNode.cubeCoordinates;// new Vector3(goalNode.xVal, goalNode.yVal, goalNode.zVal);
        h = Vector3.Distance(start, end);
    }
    public void Reset()
    {
        ToggleVisited(false);
        f = 0;
        g = 0;
        h = 0;
    }
    public void AssignPreviouseNode(Node newNode)
    {
        previouseNode = newNode;
    }
    public bool IsAbstract()
    {
        return level == NodeType.Abstract;
    }
    public bool IsTemporary()
    {
        return level == NodeType.Temporary;
    }
    //Used for Nodes in the ConnectionGrid
    public void SetClusterParent(NodeCluster newNodeCluster)
    {
        clusterParent = newNodeCluster;
    }
    public void ManageNodesConnectingTo(Node newNode)
    {
        if (nodesConnectingTo == null)
        {
            nodesConnectingTo = new List<Node>();
        }
        if (!nodesConnectingTo.Contains(newNode))
        {
            nodesConnectingTo.Add(newNode);
        }
    }
    /*public void SetFaceParent(SphereFace newFace)
    {
        faceParent = newFace;
    }*/
    
}
