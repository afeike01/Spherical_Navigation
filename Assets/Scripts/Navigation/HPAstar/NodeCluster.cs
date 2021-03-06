﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NodeCluster
{
    public int xVal=0;
    public int yVal = 0;
    public int zVal=0;
    public int clusterSize;
    public AbstractGrid mainAbstractGrid;

    public List<Node> nodeList = new List<Node>();
    public Dictionary<int,List<Node>> storedPathsDictionary = new Dictionary<int,List<Node>>();

    private int nodeCounter = 0;

    public Vector3 gridCoordinates;
    public Vector3 cubeCoordinates;
    public Vector3 sphereCoordinates;

    public Vector3 centerGridCoordinates;
    public Vector3 centerCubeCoordinates;
    public Vector3 centerSphereCoordinates;

    public NodeCluster(AbstractGrid newAbstractGrid, int newSize, Vector3 gridLocation, Vector3 cubeLocation, Vector3 sphereLocation)
    {

        clusterSize = newSize;
        mainAbstractGrid = newAbstractGrid;

        xVal = (int)gridLocation.x;
        yVal = (int)gridLocation.y;
        zVal = (int)gridLocation.z;

        gridCoordinates = gridLocation;
        cubeCoordinates = cubeLocation;
        sphereCoordinates = sphereLocation;

        Node centerNode = mainAbstractGrid.mainGrid.LookUpNode(gridLocation.x + clusterSize / 2, gridLocation.z + clusterSize / 2);
        centerGridCoordinates = centerNode.gridCoordinates;
        centerCubeCoordinates = SphereGrid.GridToCubeCoordinates(centerNode);
        centerSphereCoordinates = SphereGrid.CubeToSphereCoordinates(centerCubeCoordinates);
        

    }
    
    public void ManageAbstractNodes(Node refNode,bool add=true)
    { 
        if (refNode != null)
        {
            if (add&&!refNode.IsTemporary())
            {

                bool isValid = true;

                int minX = 0;// ((int)mainAbstractGrid.mainGrid.transform.position.x);
                int minZ = 0;// ((int)mainAbstractGrid.mainGrid.transform.position.z);
                int maxX = (mainAbstractGrid.mainGrid.gridSize - 1);// +((int)mainAbstractGrid.mainGrid.transform.position.x);
                int maxZ = (mainAbstractGrid.mainGrid.gridSize - 1);// +((int)mainAbstractGrid.mainGrid.transform.position.z);

                /*if (refNode.xVal == minX && refNode.zVal == minZ)
                    isValid = false;
                if (refNode.xVal == maxX && refNode.zVal == maxZ)
                    isValid = false;
                if (refNode.xVal == minX && refNode.zVal == maxZ)
                    isValid = false;
                if (refNode.xVal == maxX && refNode.zVal == minZ)
                    isValid = false;*/

                if (isValid)
                {
                    //Create a new Node
                    //Add to NodeList
                    Vector3 newLocation = refNode.gridCoordinates;
                    Node newNode = new Node(this.mainAbstractGrid.mainGrid, newLocation, NodeType.Abstract, nodeCounter);
                    nodeCounter++;
                    newNode.SetClusterParent(this);
                    nodeList.Add(newNode);
                    mainAbstractGrid.ManageAbstractNodeList(newNode);
                }
                
            }
            else if (add && refNode.IsTemporary())
            {
                //If an Inserted Node
                if (!nodeList.Contains(refNode))
                {
                    nodeList.Add(refNode);
                }
            }
            else
            {
                //If Remove
                nodeList.Remove(refNode);
                if(refNode.IsTemporary())
                    refNode = null;
            }
        }
        
    }
    public Vector3 GetLocation()
    {
        return new Vector3(xVal,yVal,zVal);
    }
    public int GetConnectionKey(Node startNode, Node endNode)
    {
        return startNode.nodeNum + (endNode.nodeNum * 1000);
    }
    public List<Node> GetStoredPath(Node startNode, Node endNode)
    {
        int connectionKey = GetConnectionKey(startNode,endNode);
        if (storedPathsDictionary.ContainsKey(connectionKey))
        {
            List<Node> newPath = storedPathsDictionary[connectionKey];
            return newPath;
        }
        else
            return null;
        
    }
    public void StoreConnectionValue(Node startNode, Node endNode)
    {
        

        Node refStartNode = mainAbstractGrid.mainGrid.LookUpNode(startNode.xVal, startNode.zVal);
        Node refEndNode = mainAbstractGrid.mainGrid.LookUpNode(endNode.xVal, endNode.zVal);

        if (refStartNode == null || refEndNode == null)
        {
            Debug.Log("Node is Null");
            return;
        }

        //This is questionable, consider revision
        float newVal = mainAbstractGrid.mainGrid.DoesPathExist(refStartNode, refEndNode);
        int connectionKey = GetConnectionKey(startNode, endNode);
        if (!storedPathsDictionary.ContainsKey(connectionKey))
        {
            if (newVal > 0)
            {
                startNode.AddNeighbor(endNode);
                endNode.AddNeighbor(startNode);
                StorePath(startNode, endNode);
            }
        }
        
    }
    public void StorePath(Node startNode, Node endNode)
    {
        if (!startNode.IsTemporary() && !endNode.IsTemporary())
        {
            Node refStartNode = mainAbstractGrid.mainGrid.LookUpNode(startNode.xVal, startNode.zVal);
            Node refEndNode = mainAbstractGrid.mainGrid.LookUpNode(endNode.xVal, endNode.zVal);

            List<Node> newPath = mainAbstractGrid.mainGrid.FindPath(refStartNode, refEndNode);
            int connectionKey = GetConnectionKey(startNode, endNode);
            if (!storedPathsDictionary.ContainsKey(connectionKey) && newPath != null)
            {
                storedPathsDictionary.Add(connectionKey, newPath); 
            }
        }
    }
    public void StoreRefreshedPath(Node startNode, Node endNode)
    {
        if (!startNode.IsTemporary() && !endNode.IsTemporary())
        {
            Node refStartNode = mainAbstractGrid.mainGrid.LookUpNode(startNode.xVal, startNode.zVal);
            Node refEndNode = mainAbstractGrid.mainGrid.LookUpNode(endNode.xVal, endNode.zVal);

            List<Node> newPath = mainAbstractGrid.mainGrid.FindPath(refStartNode, refEndNode);
            int connectionKey = GetConnectionKey(startNode, endNode);
            if (newPath != null&&storedPathsDictionary.ContainsKey(connectionKey))
            {
                storedPathsDictionary[connectionKey] = newPath;
            }
        }
    }
    public void RefreshPaths(Node newNode)
    {
        List<int> pathsToRefresh = new List<int>();
        foreach (int connectionKey in storedPathsDictionary.Keys)
        {
            List<Node> path = storedPathsDictionary[connectionKey] as List<Node>;
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i].xVal == newNode.xVal && path[i].zVal == newNode.zVal)
                {
                    pathsToRefresh.Add(connectionKey);
                }
            }
        }
        for (int i = 0; i < pathsToRefresh.Count; i++)
        {
            List<Node> path = storedPathsDictionary[pathsToRefresh[i]];
            StoreRefreshedPath(path[0], path[path.Count - 1]);
        }
    }
    public void SetAbstractConnections()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            Node startNode = nodeList[i];
            for (int j = 0; j < nodeList.Count; j++)
            {
                Node endNode = nodeList[j];
                StoreConnectionValue(startNode, endNode);
                
            }
        }
    }
}
