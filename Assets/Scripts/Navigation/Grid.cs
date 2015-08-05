using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public enum Orientation : int
{
    Top = 0,
    Bottom = 1,
    Right = 2,
    Left = 3,
    Front = 4,
    Back = 5,
}
public class Grid// : MonoBehaviour 
{
    private BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();
    private float gDist = 0;
    public float gDistInc = .01f;

    public Dictionary<Vector3, Node> initNodeDictionary = new Dictionary<Vector3, Node>();
    private int nodeCounter = 0;

    public int gridSize = 20;
    public Orientation gridOrientation;

    public int clusterSize = 10;
    public AbstractGrid abstractGrid;

    public Grid(Orientation newOrientation, int newSize)
    {
        gridOrientation = newOrientation;
        gridSize = newSize;
        InitializeGrid();
    }
    private void InitializeGrid()
    {
        CreateNodes();
        
        AssignAllNeighboors();
        
        abstractGrid = CreateAbstractGrid();
        
    }
    
    
    private AbstractGrid CreateAbstractGrid()
    {
        return new AbstractGrid(this, clusterSize);
    }
    private void CreateNodes()
    {
        int newX = 0;//-gridSize / 2;//(int)transform.position.x;
        for (int i = 0; i < gridSize; ++i)
        {
            SpawnX(newX);
            newX++;
        }
    }
    private void SpawnX(int newX)
    {
        int tempZ = 0;//-gridSize / 2;//(int)transform.position.z;
        Vector3 newLocation = new Vector3(newX,0,tempZ);
        Node newNode = new Node(this,newLocation, NodeType.Normal,nodeCounter);
        initNodeDictionary.Add(newLocation, newNode);//nodeDictionary.Add(nodeKey, newNode);

        

        nodeCounter++;


        int newZ = tempZ + 1;
        
        for (int i = 0; i < gridSize - 1; ++i)
        {
            SpawnZ(newX, newZ);
            newZ++;
        }
    }
    private void SpawnZ(int newX, int newZ)
    {
        
        Vector3 newLocation = new Vector3(newX, 0, newZ);
        Node newNode = new Node(this, newLocation, NodeType.Normal, nodeCounter);
        initNodeDictionary.Add(newLocation, newNode);//nodeDictionary.Add(nodeKey, newNode);

        

        nodeCounter++;
    }
    public static Vector3 GetNodeKey(Node newNode)
    {
        return newNode.gridCoordinates;

        //return newNode.xVal * 1000 + newNode.zVal;
    }
    public static Vector3 GetNodeKey(int newX, int newZ)
    {
        return new Vector3(newX, 0, newZ);
        //return newX * 1000 + newZ;
    }
    public Node LookUpNode(int newX, int newZ)
    {
        Vector3 newLocation = GetNodeKey(newX, newZ);
        //int nodeKey = GetNodeKey(newX, newZ);
        if (initNodeDictionary.ContainsKey(newLocation))
        {
            return initNodeDictionary[newLocation];
        }
        else
        {
            return null;
        }
    }
    public Node LookUpInitNode(Vector3 newLocation)
    {
        if (initNodeDictionary.ContainsKey(newLocation))
        {
            return initNodeDictionary[newLocation];
        }
        else
            return null;
    }
    public Node LookUpNode(float newX, float newZ)
    {

        Vector3 nodeKey = new Vector3(newX, 0, newZ);
        //int nodeKey = GetNodeKey((int)newX, (int)newZ);
        if (initNodeDictionary.ContainsKey(nodeKey))
        {
            return initNodeDictionary[nodeKey];
        }
        else
        {
            return null;
        }
    }
    
    private void AssignAllNeighboors()
    {

        foreach(Node newNode in initNodeDictionary.Values)
        {
            
            Node tempNode = null;
            int cSize = clusterSize;
            Vector3 nodeLocation = new Vector3(0, 0, 0);

            nodeLocation.x = newNode.xVal + 1;
            nodeLocation.z = newNode.zVal;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal + 1, newNode.zVal);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);

            nodeLocation.x = newNode.xVal - 1;
            nodeLocation.z = newNode.zVal;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal - 1, newNode.zVal);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);

            nodeLocation.x = newNode.xVal;
            nodeLocation.z = newNode.zVal+1;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);

            nodeLocation.x = newNode.xVal;
            nodeLocation.z = newNode.zVal-1;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);

            nodeLocation.x = newNode.xVal + 1;
            nodeLocation.z = newNode.zVal+1;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal + 1, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);

            nodeLocation.x = newNode.xVal - 1;
            nodeLocation.z = newNode.zVal-1;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal - 1, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);

            nodeLocation.x = newNode.xVal - 1;
            nodeLocation.z = newNode.zVal+1;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal - 1, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);

            nodeLocation.x = newNode.xVal + 1;
            nodeLocation.z = newNode.zVal-1;
            tempNode = LookUpInitNode(nodeLocation);//(newNode.xVal + 1, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
        }
        
    }
    /*
     * AreNodesWithinSameCluster(Node, Node) is used to assign neighbors correctly 
     * before the NodeClusters of the AbstractGrid are created.
     */
    private bool AreNodesWithinSameCluster(Node newNode, Node checkNode)
    {
        
        if (newNode == null || checkNode == null)
            return false;

        int minX = 0;//-gridSize / 2;//(int)transform.position.x;
        int minZ = 0;// -gridSize / 2;// (int)transform.position.z;
        

        Vector2 currentMin = new Vector2(minX,minZ);
        Vector2 currentMax = new Vector2(minX+(clusterSize-1),minZ+(clusterSize-1));

        bool clusterFound = false;
        for (int i = 0; i < gridSize / clusterSize; i++)
        {
            currentMin.x = minX + (clusterSize * i);
            currentMax.x = currentMin.x + (clusterSize - 1);
            

            for (int j = 0; j < gridSize / clusterSize; j++)
            {
                currentMin.y = minZ+(clusterSize*j);
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
            if (checkNode.xVal >= currentMin.x && checkNode.xVal <= currentMax.x && checkNode.zVal >= currentMin.y && checkNode.zVal <= currentMax.y)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
        
    }
    public List<Node> FindComplexPath(Node startNode, Node endNode)
    {
        if (startNode.available != true || endNode.available != true)
        {
            return null;
        }

        List<Node> newAbstractPath;
        if (startNode.gridParent != endNode.gridParent)
            newAbstractPath = startNode.gridParent.abstractGrid.FindMultiAbstractGridPath(startNode, endNode);
        else
            newAbstractPath = startNode.gridParent.abstractGrid.FindAbstractPath(startNode, endNode);
        
        List<Node> tempList = new List<Node>();
        List<Node> outList = new List<Node>();

        if (newAbstractPath == null)
        {
            //Debug.Log("Abstract Path is Null");
            return null;
        }

        for (int i = 0; i < newAbstractPath.Count-1; i++)
        {
            Node sNode = newAbstractPath[i];
            Node eNode = newAbstractPath[i + 1];

            if (sNode.clusterParent == eNode.clusterParent)
            {
                NodeCluster newCluster = sNode.clusterParent;
                //Nodes are NOT Temporary, Use Precomputed Path
                if (!sNode.IsTemporary()&&!eNode.IsTemporary())
                {
                    
                    List<Node> storedPath = sNode.clusterParent.GetStoredPath(sNode, eNode);
                    if (storedPath==null)
                    {
                        Debug.Log("Stored Path is NULL");
                        Debug.Log("Start Node:(" + sNode.xVal + "," + sNode.zVal + ") End Node:(" + eNode.xVal + "," + eNode.zVal + ")");
                        return null;
                    }
                    
                    bool direction = (sNode.xVal == storedPath[0].xVal && sNode.zVal == storedPath[0].zVal);
                    if (!direction)
                    {
                        storedPath.Reverse();
                    }
                        
                    if (outList.Count > 0)
                    {
                        if (outList[outList.Count - 1] == sNode)
                            outList.RemoveAt(outList.Count - 1);
                    }
                    outList.AddRange(storedPath);
                }
                //At least one of the Nodes have been Inserted.  Compute a new path
                else
                {
                    Node s = sNode.gridParent.LookUpNode(sNode.xVal, sNode.zVal);
                    Node e = eNode.gridParent.LookUpNode(eNode.xVal, eNode.zVal);
                    tempList = sNode.gridParent.FindPath(s, e);
                    outList.AddRange(tempList);
                    
                    tempList.Clear();
                }
                

            }
            else
            {
                if ((i + 2) < newAbstractPath.Count)
                {
                    if (newAbstractPath[i + 2].IsAbstract())
                        outList.Add(eNode);
                }
                
            }
        }
        return outList;
        
    }
   
    public List<Node> FindPath(Node startNode, Node endNode, bool countVisitedNodes=false)
    {
        
        frontierHeap.Add(startNode);
        gDist = 0;


        while (frontierHeap.Count > 0)
        {

            Node currentNode = frontierHeap.Remove();
            
            currentNode.ToggleVisited(true);


            if (currentNode == endNode)
                break;
            gDist += gDistInc;
            for (int i = 0; i < currentNode.neighbors.Count; ++i)
            {
                if (currentNode.neighbors[i].visited != true && currentNode.neighbors[i].available)
                {
                    if (currentNode.clusterParent == currentNode.neighbors[i].clusterParent)
                    {
                        currentNode.neighbors[i].AssignPreviouseNode(currentNode);
                        currentNode.neighbors[i].SetG(gDist);
                        currentNode.neighbors[i].SetH(endNode);
                        currentNode.neighbors[i].SetF();
                        frontierHeap.Add(currentNode.neighbors[i]);

                        if (countVisitedNodes)
                        {
                            //visitCounter++;
                        }
                            
                    }   
                }
            }
        }
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

            ResetGrid();
            return newPath;
        }
        else
        {
            ResetGrid();
            return null;
        }
        
    }
    public float DoesPathExist(Node startNode, Node endNode)
    {
        float currentPathCost = 0;

        frontierHeap.Add(startNode);
        gDist = 0;

        while (frontierHeap.Count > 0)
        {
            Node currentNode = frontierHeap.Remove();
            
            currentNode.ToggleVisited(true);
            if (currentNode == endNode)
            {
                break;
            }
                
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

        bool pathExists = endNode.visited;


        if (pathExists)
        {
            Node curNode = endNode;

            while (curNode != startNode)
            {
                currentPathCost += curNode.f;
                curNode = curNode.previouseNode;
            }
        }

        ResetGrid();
        return (pathExists) ? currentPathCost : -1;
    }
    private void ResetGrid()
    {

        foreach(Node newNode in initNodeDictionary.Values)
        {
            newNode.Reset();
        }
        frontierHeap.Clear();
        
    }
    

    /*public bool ToggleNodeAvailability(Node newNode, bool availability = false)
    {
        if (newNode != null)
        {
            newNode.ToggleAvailable(availability);
            return true;
        }
        else
            return false;
            
    }
    public bool ToggleNodeAvailability(int xVal, int zVal, bool availability = false)
    {
        Node newNode = LookUpNode(xVal, zVal);
        if (newNode != null)
        {
            newNode.ToggleAvailable(availability);
            return true;
        }
        else
            return false;
    }*/
    /*
     * Connection Nodes in the Grid Class are Abstract Nodes
     */
    
}
