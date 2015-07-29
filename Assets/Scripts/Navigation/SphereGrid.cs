﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class SphereGrid : MonoBehaviour 
{
    private BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();

    private const float spacing = 1;
    public int nodeCounter = 0;

    private float gDist = 0;
    public float gDistInc = 1f;

    private Dictionary<Vector3, Node> tempNodeDictionary = new Dictionary<Vector3, Node>();
    public Dictionary<Vector3, Node> nodeDictionary = new Dictionary<Vector3, Node>();

    public int gridSize = 30;
    public float[] heightMap;

    List<Node> mainPath = new List<Node>();

    public GameObject nodeVisual;


    private const int SIZE = 20;
    private const float RADIUS = 10;

    private List<Vector3> cubeCoordinates = new List<Vector3>();
    private List<Vector3> sphereCoordinates = new List<Vector3>();

    public SphereCollider planetCollider;

    public Dictionary<FaceType, SphereFace> sphereFaceDictionary = new Dictionary<FaceType, SphereFace>();
    public List<Node> cornerNodes = new List<Node>();

    private Node startNode;
    private Node endNode;
    private GridAgent agent;
    public GameObject agentPrefab;

	void Start () 
    {
        InitializeGrid();   
	}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Node startNode = LookUpNode(new Vector3(0,5,0));
            Node endNode = LookUpNode(new Vector3(5,0,0));
            List<Node> newPath = FindPath(startNode, endNode);
            VisualizePath(newPath);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == planetCollider)
                {
                    Node newNode = GetClosestNode(hit.point);
                    if (agent == null)
                    {
                        if (startNode == null)
                        {
                            startNode = newNode;
                        }
                        else
                        {
                            endNode = newNode;
                        }
                        if (startNode != null && endNode != null)
                        {

                            GameObject newAgentPrefab = Instantiate(agentPrefab, startNode.GetLocation(), Quaternion.identity) as GameObject;
                            agent = newAgentPrefab.GetComponent<GridAgent>();
                            agent.SetCurrentNode(startNode);
                            agent.SetNavigationGrid(this);

                            mainPath = FindPath(startNode, endNode);
                            agent.GetPath(mainPath);

                            //VisualizePath(mainPath);
                        }
                    }
                    else
                    {
                        startNode = agent.currentNode;
                        endNode = newNode;
                        mainPath.Clear();
                        mainPath = FindPath(startNode, endNode);
                        agent.GetPath(mainPath);
                    }
                }
            }
        }
        
    }
    private void InitializeGrid()
    {
        CreateHeightMap();
        GenerateCubePoints();
        AssignAllNeighboors();
        CreateSphereFaces();
        SetNodeFaceParents();
        GenerateSpherePoints();

        
        
        //Currently Hard Coded
        //IN PROGRESS
        /*
         * To Do:
         *  Mark off CornerNodes
         *  Use to determine which face a Node is within
         *  Search within Faces Nodes to determine the closest Node to the hit.point
         *  This is an inefficient way to handle this, but will get the job done for the time being
         */ 
        

        /*int newX = (int)transform.position.x;
        for (int i = 0; i < gridSize; ++i)
        {
            SpawnX(newX);
            newX++;
        }*/
        
        //abstractGrid = CreateAbstractGrid();

        /*for (int i = 0; i < connectors.Length; i++)
        {
            if(connectors[i]!=null)
                connectors[i].SetAbstractGrid(this);
        }

        connectionGrid.ManageGridList(this);*/


    }
    private void SpawnNodeVisual(Vector3 newLocation)
    {
        GameObject newVisual = Instantiate(nodeVisual, newLocation, Quaternion.identity) as GameObject;
    }
    private void VisualizePath(List<Node> newPath)
    {
        for (int i = 0; i < newPath.Count; i++)
        {
            GameObject newVisual = Instantiate(nodeVisual, newPath[i].GetLocation(), Quaternion.identity) as GameObject;
        }
    }
    /*private void SetCornerNodes()
    {
        cornerNodes.Add(LookUpNode(new Vector3(5.8f, 5.8f, 5.8f)));
        cornerNodes.Add(LookUpNode(new Vector3(-5.8f, 5.8f, 5.8f)));
        cornerNodes.Add(LookUpNode(new Vector3(5.8f, -5.8f, 5.8f)));
        cornerNodes.Add(LookUpNode(new Vector3(-5.8f, -5.8f, 5.8f)));

        cornerNodes.Add(LookUpNode(new Vector3(-5.8f, -5.8f, -5.8f)));
        cornerNodes.Add(LookUpNode(new Vector3(5.8f, 5.8f, -5.8f)));
        cornerNodes.Add(LookUpNode(new Vector3(-5.8f, 5.8f, -5.8f)));
        cornerNodes.Add(LookUpNode(new Vector3(5.8f, -5.8f, -5.8f)));
    }*/
    private void CreateSphereFaces()
    {
        FaceType[] type = { FaceType.Top,FaceType.Bottom,FaceType.Right,FaceType.Left,FaceType.Front,FaceType.Back };
        for (int i = 0; i < type.Length; i++)
        {
            SphereFace newFace = new SphereFace(type[i]);
            sphereFaceDictionary.Add(type[i], newFace);
        }
    }
    private void SetNodeFaceParents()
    {
        foreach (Node n in tempNodeDictionary.Values)
        {
            int maxVal = SIZE/2;
            if (n.GetLocation().y >= maxVal)
                sphereFaceDictionary[FaceType.Top].ManageNodeList(n);
            if(n.GetLocation().y<=-maxVal)
                sphereFaceDictionary[FaceType.Bottom].ManageNodeList(n);
            if (n.GetLocation().x >= maxVal)
                sphereFaceDictionary[FaceType.Right].ManageNodeList(n);
            if (n.GetLocation().x <= -maxVal)
                sphereFaceDictionary[FaceType.Left].ManageNodeList(n);
            if (n.GetLocation().z >= maxVal)
                sphereFaceDictionary[FaceType.Front].ManageNodeList(n);
            if (n.GetLocation().z <= -maxVal)
                sphereFaceDictionary[FaceType.Back].ManageNodeList(n);

        }
    }
    private SphereFace GetFace(Node newNode)
    {
        Vector3 newLocation = newNode.GetLocation();

        if (newLocation.y >= 5.8f)
        {
            //TopFace
            return sphereFaceDictionary[FaceType.Top];
        }
        if (newLocation.y <= -5.8f)
        {
            //BottomFace
            return sphereFaceDictionary[FaceType.Bottom];
        }
        if (newLocation.x >= 5.8f)
        {
            //RightFace
            return sphereFaceDictionary[FaceType.Right];
        }
        if (newLocation.x <= -5.8f)
        {
            //LeftFace
            return sphereFaceDictionary[FaceType.Left];
        }
        if (newLocation.z >= 5.8f)
        {
            //FrontFace
            return sphereFaceDictionary[FaceType.Front];
        }
        if (newLocation.z <= -5.8f)
        {
            //BackFace
            return sphereFaceDictionary[FaceType.Back];
        }
        else
            return null;

    }
    private Node GetClosestNode(Vector3 newLocation)
    {
        SphereFace newFace = GetFace(newLocation);
        float closestDist = Vector3.Distance(newLocation, newFace.nodeList[0].GetLocation());
        Node closestNode = newFace.nodeList[0];
        for (int i = 0; i < newFace.nodeList.Count; i++)
        {
            float tempDist = Vector3.Distance(newLocation, newFace.nodeList[i].GetLocation());
            if (tempDist < closestDist)
            {
                closestDist = tempDist;
                closestNode = newFace.nodeList[i];
            }
        }
        return closestNode;
    }
    private SphereFace GetFace(Vector3 newLocation)
    {

        if (newLocation.y >= 5.8f)
        {
            //TopFace
            return sphereFaceDictionary[FaceType.Top];
        }
        else if (newLocation.y <= -5.8f)
        {
            //BottomFace
            return sphereFaceDictionary[FaceType.Bottom];
        }
        else if (newLocation.x >= 5.8f)
        {
            //RightFace
            return sphereFaceDictionary[FaceType.Right];
        }
        else if (newLocation.x <= -5.8f)
        {
            //LeftFace
            return sphereFaceDictionary[FaceType.Left];
        }
        else if (newLocation.z >= 5.8f)
        {
            //FrontFace
            return sphereFaceDictionary[FaceType.Front];
        }
        else if (newLocation.z <= -5.8f)
        {
            //BackFace
            return sphereFaceDictionary[FaceType.Back];
        }
        else
            return null;

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
                cubeCoordinates.Add(new Vector3(minVal, i, j));
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
            if (!tempNodeDictionary.ContainsKey(cubeCoordinates[i]))
            {
                
                Node newNode = new Node(this, cubeCoordinates[i], NodeType.Normal, nodeCounter);
                tempNodeDictionary.Add(cubeCoordinates[i], newNode);
                nodeCounter++;
                //SpawnNodeVisual(newNode.GetLocation());
            }
        }
    }
    private void GenerateSpherePoints()
    {
        foreach (Node n in tempNodeDictionary.Values)
        {
            Vector3 newLocation = CubeToSphereCoordinates(n.GetLocation());
            newLocation.x = Round(newLocation.x, 1);
            newLocation.y = Round(newLocation.y, 1);
            newLocation.z = Round(newLocation.z, 1);

            n.SetLocation(newLocation);
            if(!nodeDictionary.ContainsKey(newLocation))
                nodeDictionary.Add(newLocation, n);
            //SpawnNodeVisual(n.GetLocation());
        }
        tempNodeDictionary.Clear();
    }
    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
    private Vector3 CubeToSphereCoordinates(Vector3 newLocation)
    {
        Vector3 newPoint = newLocation.normalized*RADIUS;
        return newPoint;
    }
    private void AssignAllNeighboors()
    {
        foreach (Node n in tempNodeDictionary.Values)
        {
            float x = n.xVal;
            float y = n.yVal;
            float z = n.zVal;

            Vector3[] newLocation = { new Vector3(x + 1, y, z), new Vector3(x - 1, y, z), new Vector3(x, y, z + 1), new Vector3(x, y, z - 1), new Vector3(x + 1, y, z + 1), new Vector3(x - 1, y, z - 1), new Vector3(x - 1, y, z + 1), new Vector3(x + 1, y, z - 1), 
                                    new Vector3(x + 1, y+1, z), new Vector3(x - 1, y+1, z), new Vector3(x, y+1, z + 1), new Vector3(x, y+1, z - 1), new Vector3(x + 1, y+1, z + 1), new Vector3(x - 1, y+1, z - 1), new Vector3(x - 1, y+1, z + 1), new Vector3(x + 1, y+1, z - 1),new Vector3(x,y+1,z),
                                    new Vector3(x + 1, y-1, z), new Vector3(x - 1, y-1, z), new Vector3(x, y-1, z + 1), new Vector3(x, y-1, z - 1), new Vector3(x + 1, y-1, z + 1), new Vector3(x - 1, y-1, z - 1), new Vector3(x - 1, y-1, z + 1), new Vector3(x + 1, y-1, z - 1),new Vector3(x,y-1,z)};

            for (int i = 0; i < newLocation.Length; i++)
            {
                Node newNode = LookUpTempNode(newLocation[i]);
                if (newNode != null)
                {
                    n.AddNeighbor(newNode);
                }
            }
        }
    }
    /*private AbstractGrid CreateAbstractGrid()
    {
        return new AbstractGrid(this, clusterSize);
    }*/

    /*public void SpawnNodeClusterVisual(params NodeCluster[] newCluster)
    {
        for(int i =0;i<newCluster.Length;i++)
        {
            GameObject newVisual = Instantiate(nodeClusterVisual, newCluster[i].GetLocation(), Quaternion.identity) as GameObject;
        }
    }*/
    /*public static void VisualizePath(List<Node> path)
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (i == 0 || i == path.Count - 1)
                    SpawnDebugLocationVisual(path[i]);
                else
                    SpawnNodeVisual(path[i]);
            }
        }
        else
        {
            Debug.Log("Path is NULL");
        }
    }*/
    
    /*public static void SpawnNodeVisual(params Node[] nodes)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null&&nodes[i].available)
            {
                GameObject newVisual;
                if (!nodes[i].IsAbstract())
                    newVisual = Instantiate(nodes[i].gridParent.nodeVisual, nodes[i].GetLocation(), Quaternion.identity) as GameObject;
                else
                    newVisual = Instantiate(nodes[i].gridParent.nodeEntranceVisual, nodes[i].GetLocation(), Quaternion.identity) as GameObject;
            }
            else if (nodes[i] != null && !nodes[i].available)
            {
                GameObject newVisual = Instantiate(nodes[i].gridParent.nodeUnavailableVisual, nodes[i].GetLocation(), Quaternion.identity) as GameObject;
            }
        }
    }
    public static void SpawnDebugLocationVisual(Node newNode)
    {
        GameObject newVisual = Instantiate(newNode.gridParent.debugLocationVisual, newNode.GetLocation(), Quaternion.identity) as GameObject;
    }*/
    /*private void SpawnX(int newX)
    {
        int tempZ = (int)transform.position.z;
        Node newNode = new Node(this,newX, heightMap[nodeCounter], tempZ, NodeType.Normal,nodeCounter);
        nodeList.Add(newNode);
        int nodeKey = GetNodeKey(newNode);
        nodeHash.Add(nodeKey, nodeCounter);
        nodeCounter++;

        int newZ = tempZ + 1;
        for (int i = 0; i < gridSize - 1; ++i)
        {
            SpawnZ(newX, newZ);
            newZ++;
        }
    }*/
    /*private void SpawnZ(int newX, int newZ)
    {
        Node newNode = new Node(this, newX, heightMap[nodeCounter], newZ, NodeType.Normal, nodeCounter);
        nodeList.Add(newNode);
        int nodeKey = GetNodeKey(newNode);
        nodeHash.Add(nodeKey, nodeCounter);
        nodeCounter++;
    }*/
    public static Vector3 GetNodeKey(Node newNode)
    {
        return newNode.GetLocation();
    }
    public Node LookUpTempNode(Vector3 newLocation)
    {
        if (tempNodeDictionary.ContainsKey(newLocation))
        {
            return tempNodeDictionary[newLocation];
        }
        else
            return null;
    }
    public Node LookUpNode(Vector3 newLocation)
    {
        if (nodeDictionary.ContainsKey(newLocation))
        {
            return nodeDictionary[newLocation];
        }
        else
        {
            return null;
        }
    }
    
    
    /*private void AssignAllNeighboors()
    {
        for (int i = 0; i < nodeList.Count; ++i)
        {
            Node newNode = nodeList[i];
            Node tempNode = null;
            int cSize = clusterSize;

            tempNode = LookUpNode(newNode.xVal + 1, newNode.zVal);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal + 1, newNode.zVal));

            tempNode = LookUpNode(newNode.xVal - 1, newNode.zVal);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal - 1, newNode.zVal));

            tempNode = LookUpNode(newNode.xVal, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal, newNode.zVal + 1));

            tempNode = LookUpNode(newNode.xVal, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal, newNode.zVal - 1));

            tempNode = LookUpNode(newNode.xVal + 1, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal + 1, newNode.zVal + 1));

            tempNode = LookUpNode(newNode.xVal - 1, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal - 1, newNode.zVal - 1));

            tempNode = LookUpNode(newNode.xVal - 1, newNode.zVal + 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal - 1, newNode.zVal + 1));

            tempNode = LookUpNode(newNode.xVal + 1, newNode.zVal - 1);
            if (AreNodesWithinSameCluster(newNode, tempNode))
                newNode.AddNeighbor(tempNode);
            //newNode.AddNeighbor(LookUpNode(newNode.xVal + 1, newNode.zVal - 1));
        }
        
    }*/
    /*private bool AreNodesWithinSameCluster(Node newNode, Node checkNode)
    {
        
        if (newNode == null || checkNode == null)
            return false;

        int minX = (int)transform.position.x;
        int minZ = (int)transform.position.z;
        

        Vector2 currentMin = new Vector2(transform.position.x,transform.position.z);
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
        //This only works when Grid is at (0,0) and clusterSize ==10
        / *int nX = newNode.xVal;
        int nZ = newNode.zVal;

        int cX = checkNode.xVal;
        int cZ = checkNode.zVal;

        if (cX % clusterSize == 0 && cX > nX)
            return false;
        if (cZ % clusterSize == 0 && cZ > nZ)
            return false;
        if (nX % clusterSize == 0 && nX > cX)
            return false;
        if (nZ % clusterSize == 0 && nZ > cZ)
            return false;
        return true;* /
    }*/
    /*public List<Node> FindComplexPath(Node startNode, Node endNode)
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
        //List<Node> newAbstractPath = abstractGrid.FindAbstractPath(startNode, endNode);
        

        List<Node> tempList = new List<Node>();
        List<Node> outList = new List<Node>();

        if (newAbstractPath == null)
        {
            Debug.Log("Abstract Path is Null");
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
        
    }*/
   
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
                    if (true/*currentNode.clusterParent == currentNode.neighbors[i].clusterParent*/)
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

            //Debug.Log("Path Success");
            ResetGrid();
            return newPath;
        }
        else
        {
            //Debug.Log("Path Failed");
            ResetGrid();
            return null;
        }
        
    }
    /*public float DoesPathExist(Node startNode, Node endNode)
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
    }*/
    private void ResetGrid()
    {
        foreach (Node n in nodeDictionary.Values)
        {
            n.Reset();
        }
        frontierHeap.Clear();
        
    }
    private void CreateHeightMap()
    {
        heightMap = new float[gridSize * gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                heightMap[(i * gridSize) + j] = transform.position.y;
            }
        }
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
            
    }*/
    /*public bool ToggleNodeAvailability(int xVal, int zVal, bool availability = false)
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
}