﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class AbstractGrid 
{
    public Grid mainGrid;
    public int abstractGridSize;
    public int clusterSize;

    public Dictionary<Vector3, NodeCluster> nodeClusterDictionary = new Dictionary<Vector3, NodeCluster>();
    public Dictionary<Vector3, Node> nodeDictionary = new Dictionary<Vector3, Node>();
    private BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();
    private int nodeClusterCounter = 0;

    private float gDist = 0;
    public float gDistInc = .01f;

    public List<Node> connectionNodes = new List<Node>();

    public AbstractGrid(Grid newGrid, int cSize)
    {
        mainGrid = newGrid;
        clusterSize = cSize;
        abstractGridSize = mainGrid.gridSize / clusterSize;

         

        int newX = 0;// (int)mainGrid.transform.position.x;
        for (int i = 0; i < abstractGridSize; ++i)
        {
            SpawnX(newX);
            newX+=clusterSize;
        }
        
        SetClusterParents(mainGrid.initNodeDictionary);
        CreateNodeClusterEntrances();
        SetAllClusterEntranceConnections();
        AssignAbstractNodeNeighbors();
        SetConnectionNodes();
        
    }
    private void SpawnX(int newX)
    {

        int tempZ = 0;// (int)mainGrid.transform.position.z;
        Vector3 newLocation = new Vector3(newX, 0, tempZ);
        Node newNode = mainGrid.initNodeDictionary[newLocation];
        NodeCluster newNodeCluster = new NodeCluster(this, clusterSize, newNode.gridCoordinates,newNode.cubeCoordinates,newNode.sphereCoordinates);
        nodeClusterDictionary.Add(newLocation, newNodeCluster);
        nodeClusterCounter++;

        int newZ = tempZ + clusterSize;
        for (int i = 0; i < abstractGridSize - 1; ++i)
        {
            SpawnZ(newX, newZ);
            newZ += clusterSize;
        }
    }
    private void SpawnZ(int newX, int newZ)
    {
        Vector3 newLocation = new Vector3(newX, 0, newZ);
        Node newNode = mainGrid.initNodeDictionary[newLocation];
        NodeCluster newNodeCluster = new NodeCluster(this, clusterSize, newNode.gridCoordinates, newNode.cubeCoordinates, newNode.sphereCoordinates);
        nodeClusterDictionary.Add(newLocation, newNodeCluster);
        nodeClusterCounter++;
    }
    public void SetClusterParents(Dictionary<Vector3,Node> nodeDictionary)
    {

        foreach(Node newNode in nodeDictionary.Values)
        {
            int minX = 0;// (int)mainGrid.transform.position.x;
            int minZ = 0;// (int)mainGrid.transform.position.z;

            Vector2 currentMin = new Vector2(minX, minZ);
            Vector2 currentMax = new Vector2(minX + (clusterSize - 1), minZ + (clusterSize - 1));

            bool clusterFound = false;
            for (int i = 0; i < mainGrid.gridSize / clusterSize; i++)
            {
                currentMin.x = minX + (clusterSize * i);
                currentMax.x = currentMin.x + (clusterSize - 1);

                for (int j = 0; j < mainGrid.gridSize / clusterSize; j++)
                {
                    currentMin.y = minZ + (clusterSize * j);
                    currentMax.y = currentMin.y + (clusterSize - 1);

                    if (newNode.xVal >= currentMin.x && newNode.xVal <= currentMax.x && newNode.zVal >= currentMin.y && newNode.zVal <= currentMax.y)
                    {
                        clusterFound = true;
                        break;
                    }

                }
                if (clusterFound)
                    break;
            }
            if (clusterFound)
            {
                NodeCluster newCluster = LookUpNodeCluster((int)currentMin.x, (int)currentMin.y);
                newNode.SetClusterParent(newCluster);
            }
        }
    }
    public void CreateNodeClusterEntrances()
    {
        //Cluster Parents must be assigned!!!
        //This will need REVAMPED when some nodes are unavailable, currently does not account for obstacles
        
        foreach(NodeCluster newCluster in nodeClusterDictionary.Values)
        {
            int minX = newCluster.xVal;
            int minZ = newCluster.zVal;
            int maxX = minX + clusterSize-1;
            int maxZ = minZ+clusterSize-1;

            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(minX, minZ));
            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(maxX, maxZ));
            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(minX, maxZ));
            newCluster.ManageAbstractNodes(mainGrid.LookUpNode(maxX, minZ));
        }

    }
    
    public void SetAllClusterEntranceConnections()
    {
        foreach (NodeCluster newCluster in nodeClusterDictionary.Values)
        {
            newCluster.SetAbstractConnections();
        }
    }
    public void AssignAbstractNodeNeighbors()
    {

        foreach(Node newNode in nodeDictionary.Values)
        {
            Node tempNode = null;

            tempNode = LookUpAbstractNode((int)newNode.xVal + 1, (int)newNode.zVal);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode((int)newNode.xVal - 1, (int)newNode.zVal);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode((int)newNode.xVal, (int)newNode.zVal + 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode((int)newNode.xVal, (int)newNode.zVal - 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode((int)newNode.xVal + 1, (int)newNode.zVal + 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode((int)newNode.xVal - 1, (int)newNode.zVal - 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode((int)newNode.xVal - 1, (int)newNode.zVal + 1);
            newNode.AddNeighbor(tempNode);

            tempNode = LookUpAbstractNode((int)newNode.xVal + 1, (int)newNode.zVal - 1);
            newNode.AddNeighbor(tempNode);

        }
    }
    public static Vector3 GetNodeClusterKey(int newX, int newZ)
    {
        return new Vector3(newX, 0, newZ);
        //return newX * 1000 + newZ;
    }
    public NodeCluster LookUpNodeCluster(int newX, int newZ)
    {
        Vector3 nodeClusterKey = GetNodeClusterKey(newX, newZ);
        //int nodeClusterKey = GetNodeClusterKey(newX, newZ);
        if (nodeClusterDictionary.ContainsKey(nodeClusterKey))
        {
            return nodeClusterDictionary[nodeClusterKey];
        }
        else
        {
            return null;
        }
    }
    public Vector3 GetAbstractNodeKey(Node newNode)
    {
        return newNode.gridCoordinates;
        //return newNode.xVal * 1000 + newNode.zVal;
    }
    public Node LookUpAbstractNode(int newX, int newZ)
    {
        Vector3 nodeKey = new Vector3(newX, 0, newZ);
        //int nodeKey = newX * 1000 + newZ;
        if (nodeDictionary.ContainsKey(nodeKey))
        {
            return nodeDictionary[nodeKey];
        }
        else
        {
            return null;
        }
    }
    public void ManageAbstractNodeList(Node newNode,bool add=true)
    {
        Vector3 nodeKey = GetAbstractNodeKey(newNode);
        if (add && newNode != null && !nodeDictionary.ContainsKey(nodeKey))
        {
            nodeDictionary.Add(nodeKey, newNode);
        }
        else
        {
            if (nodeDictionary.ContainsKey(nodeKey))
            {
                nodeDictionary.Remove(nodeKey);
            }
        }
    }
    
    public List<Node> FindAbstractPath(Node sNode, Node eNode)
    {


        bool startNodeInserted = (LookUpAbstractNode((int)sNode.xVal, (int)sNode.zVal) == null);
        bool endNodeInserted = (LookUpAbstractNode((int)eNode.xVal, (int)eNode.zVal) == null);

        Node startNode = startNodeInserted ? InsertNode(sNode) : LookUpAbstractNode((int)sNode.xVal, (int)sNode.zVal);
        Node endNode = endNodeInserted ? InsertNode(eNode) : LookUpAbstractNode((int)eNode.xVal, (int)eNode.zVal);


        frontierHeap.Add(startNode);
        gDist = 0;
        while (frontierHeap.Count > 0)
        {

            Node currentNode = frontierHeap.Remove();
            if (currentNode.visited)
                break;
            currentNode.ToggleVisited(true);

            if (currentNode == endNode)
                break;
            gDist += gDistInc;
            for (int i = 0; i < currentNode.neighbors.Count; ++i)
            {
                if (currentNode.neighbors[i].visited != true && currentNode.neighbors[i].available)
                {
                    currentNode.neighbors[i].AssignPreviouseNode(currentNode);
                    currentNode.neighbors[i].SetG(gDist);
                    currentNode.neighbors[i].SetH(endNode);
                    currentNode.neighbors[i].SetF();
                    frontierHeap.Add(currentNode.neighbors[i]);
                }
            }

        }
        //===================================================
        //Back track through nodes
        //===================================================
        bool pathExists = endNode.visited;
        if (pathExists)
        {
            Node curNode = endNode;
            List<Node> newPath = new List<Node>();

            while (curNode != startNode)
            {
                newPath.Add(curNode);
                curNode = curNode.previouseNode;
            }
            newPath.Add(curNode);
            newPath.Reverse();

            if (startNodeInserted)
            {
                
                RemoveNode(startNode);
            }

            if (endNodeInserted)
            {
                
                RemoveNode(endNode);
            }
            ResetAbstractGrid();

            return newPath;
        }
        else
        {
            
            if (startNodeInserted)
            {
                
                RemoveNode(startNode);
            }

            if (endNodeInserted)
            {
                
                RemoveNode(endNode);
            }
                
            ResetAbstractGrid();

            return null;
            
        }

    }

    public List<Node> FindMultiAbstractGridPath(Node sNode, Node eNode)
    {

        Grid startNodeGrid = sNode.gridParent;
        Grid endNodeGrid = eNode.gridParent;

        bool startNodeInserted = (startNodeGrid.abstractGrid.LookUpAbstractNode((int)sNode.xVal, (int)sNode.zVal) == null);
        bool endNodeInserted = (endNodeGrid.abstractGrid.LookUpAbstractNode((int)eNode.xVal, (int)eNode.zVal) == null);

        Node startNode = startNodeInserted ? startNodeGrid.abstractGrid.InsertNode(sNode) : startNodeGrid.abstractGrid.LookUpAbstractNode((int)sNode.xVal, (int)sNode.zVal);
        Node endNode = endNodeInserted ? endNodeGrid.abstractGrid.InsertNode(eNode) : endNodeGrid.abstractGrid.LookUpAbstractNode((int)eNode.xVal, (int)eNode.zVal);

        frontierHeap.Add(startNode);
        gDist = 0;

        while (frontierHeap.Count > 0)
        {

            Node currentNode = frontierHeap.Remove();
            if (currentNode.visited)
                break;
            currentNode.ToggleVisited(true);

            if (currentNode == endNode)
                break;
            gDist += gDistInc;
            for (int i = 0; i < currentNode.neighbors.Count; ++i)
            {
                if (currentNode.neighbors[i].visited != true && currentNode.neighbors[i].available)
                {
                    currentNode.neighbors[i].AssignPreviouseNode(currentNode);
                    currentNode.neighbors[i].SetG(gDist);
                    currentNode.neighbors[i].SetH(endNode);
                    currentNode.neighbors[i].SetF();
                    frontierHeap.Add(currentNode.neighbors[i]);
                }
            }

        }
        //===================================================
        //Back track through nodes
        //===================================================
        bool pathExists = endNode.visited;
        if (pathExists)
        {
            Node curNode = endNode;
            List<Node> newPath = new List<Node>();
            //Debug.Log("PathSucceeded: " + endNode.visited);

            while (curNode != startNode)
            {
                //pathCost += curNode.f;
                //Node newNode = mainGrid.LookUpNode(curNode.xVal, curNode.zVal);
                newPath.Add(curNode);
                curNode = curNode.previouseNode;
            }

            //Node lastNode = mainGrid.LookUpNode(curNode.xVal, curNode.zVal);
            newPath.Add(curNode);
            newPath.Reverse();

            if (startNodeInserted)
                startNodeGrid.abstractGrid.RemoveNode(startNode);
            if (endNodeInserted)
                endNodeGrid.abstractGrid.RemoveNode(endNode);
            startNodeGrid.abstractGrid.ResetAbstractGrid();
            endNodeGrid.abstractGrid.ResetAbstractGrid();

            return newPath;
        }
        else
        {
            Debug.Log("Abstract Path is NULL");

            if (startNodeInserted)
                startNodeGrid.abstractGrid.RemoveNode(startNode);
            if (endNodeInserted)
                endNodeGrid.abstractGrid.RemoveNode(endNode);
            startNodeGrid.abstractGrid.ResetAbstractGrid();
            endNodeGrid.abstractGrid.ResetAbstractGrid();

            return null;

        }

    }
    private void ResetAbstractGrid()
    {
        foreach (Node newNode in nodeDictionary.Values)
        {
            newNode.Reset();
        }
        frontierHeap.Clear();

    }
    public NodeCluster GetNodeClusterFromLocation(int xVal, int zVal)
    {
        foreach(NodeCluster newCluster in nodeClusterDictionary.Values)
        {
            int minX = newCluster.xVal;
            int minZ = newCluster.zVal;
            int maxX = newCluster.xVal + clusterSize;
            int maxZ = newCluster.zVal + clusterSize;

            if (xVal >= minX && xVal <= maxX && zVal >= minZ && zVal <= maxZ)
                return newCluster;
        }
        return null;
    }
    /*
    * =============================================================
    *                      Manages Inserted Nodes
    * =============================================================
    */
    public Node InsertNode(Node refNode)
    {

        NodeCluster newCluster = refNode.clusterParent != null ? refNode.clusterParent : GetNodeClusterFromLocation((int)refNode.xVal, (int)refNode.zVal);
        Node newNode = new Node(this.mainGrid,refNode.gridCoordinates, NodeType.Temporary,-1);
        newNode.SetClusterParent(newCluster);

        ManageAbstractNodeList(newNode);
        newCluster.ManageAbstractNodes(newNode);
        newCluster.SetAbstractConnections();
        
        return newNode;


    }
    public void RemoveNode(Node newNode)
    {
        //Only Remove if Node is NOT ABSTRACT
        if (newNode.IsTemporary())
        {
            NodeCluster newCluster = newNode.clusterParent;
            ManageAbstractNodeList(newNode, false);
            
            for (int i = 0; i < newCluster.nodeList.Count; i++)
            {
                Node removeNode=null;
                for (int j = 0; j < newCluster.nodeList[i].neighbors.Count; j++)
                {
                    if (newCluster.nodeList[i].neighbors[j] == newNode)
                        removeNode = newNode;
                }
                if (removeNode != null)
                    newCluster.nodeList[i].neighbors.Remove(removeNode);
            }
            newCluster.ManageAbstractNodes(newNode, false);
            newCluster.SetAbstractConnections();
        }
        
    }
    public void SetConnectionNodes()
    {
        int minVal = 0;
        int maxVal = mainGrid.gridSize-1;

        foreach (Node n in nodeDictionary.Values)
        {
            if (n.xVal == minVal || n.zVal == minVal || n.xVal == maxVal || n.zVal == maxVal)
            {
                connectionNodes.Add(n);
            }
        }
        
    }
}
