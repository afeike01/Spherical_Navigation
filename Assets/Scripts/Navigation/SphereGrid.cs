using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Noise;


public class SphereGrid : MonoBehaviour 
{
    public Dictionary<string, List<Node>> storedPathsDictionary = new Dictionary<string, List<Node>>();
    private BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();

    private const int SIZE = 40;
    private const float RADIUS = 10;

    private float gDist = 0;
    public float gDistInc = .01f;

    public Dictionary<Orientation, Grid> gridDictionary = new Dictionary<Orientation, Grid>();
    public Dictionary<string, Node> nodeDictionary = new Dictionary<string, Node>();
    public int nodeCounter = 0;

    public GameObject nodeVisual;
    public GameObject locationPrefab;
    public GameObject nodeClusterVisual;

    public MeshGenerator meshGen_BackFace;
    public MeshGenerator meshGen_FrontFace;
    public MeshGenerator meshGen_TopFace;
    public MeshGenerator meshGen_BottomFace;
    public MeshGenerator meshGen_RightFace;
    public MeshGenerator meshGen_LeftFace;

    private Node startNode;
    private Node endNode;
    private List<Node> mainPath = new List<Node>();

    public Node targetNode;

	void Start () 
    {
        InitializeGrid();
	}
    private void InitializeGrid()
    {
        CreateGrids();
        SetAllGridConnections();
        SetConnectionNodeNeighbors();
        SetConnections();
        CreateSurface();

        
    }
    public static Vector3 GridToCubeCoordinates(Node newNode)
    {
        Orientation nodeOrientation = newNode.gridParent.gridOrientation;
        Vector3 originalCoordinates = newNode.gridCoordinates;
        Vector3 cubeCoordinates;
        float maxVal = (SIZE)-1;
        if (nodeOrientation == Orientation.Bottom)
        {
            //cubeCoordinates = new Vector3(originalCoordinates.x, -maxVal, originalCoordinates.z);
            cubeCoordinates = new Vector3(originalCoordinates.x, originalCoordinates.y, originalCoordinates.z);
            cubeCoordinates += new Vector3(-(maxVal / 2), -(maxVal / 2), -(maxVal / 2));
            return cubeCoordinates;
        }
        else if (nodeOrientation == Orientation.Top)
        {
            //cubeCoordinates = new Vector3(originalCoordinates.x, maxVal, originalCoordinates.z);
            cubeCoordinates = new Vector3(originalCoordinates.x, maxVal, originalCoordinates.z);
            cubeCoordinates += new Vector3(-(maxVal / 2), -(maxVal / 2), -(maxVal / 2));
            return cubeCoordinates;
        }
        else if (nodeOrientation == Orientation.Front)
        {
            //cubeCoordinates = new Vector3(originalCoordinates.x, originalCoordinates.z, -maxVal);
            cubeCoordinates = new Vector3(originalCoordinates.x,originalCoordinates.z,0);
            cubeCoordinates += new Vector3(-(maxVal / 2), -(maxVal / 2), -(maxVal / 2));
            return cubeCoordinates;
        }
        else if (nodeOrientation == Orientation.Back)
        {
            cubeCoordinates = new Vector3(maxVal-originalCoordinates.x, originalCoordinates.z, maxVal);
            cubeCoordinates += new Vector3(-(maxVal / 2), -(maxVal / 2), -(maxVal / 2));
            return cubeCoordinates;
        }
        else if (nodeOrientation == Orientation.Right)
        {
            cubeCoordinates = new Vector3(maxVal, originalCoordinates.z, originalCoordinates.x);
            cubeCoordinates += new Vector3(-(maxVal / 2), -(maxVal / 2), -(maxVal / 2));
            return cubeCoordinates;
        }
        else if (nodeOrientation == Orientation.Left)
        {
            //cubeCoordinates = new Vector3(-maxVal, originalCoordinates.z, -originalCoordinates.x);
            cubeCoordinates = new Vector3(0, originalCoordinates.z, maxVal-originalCoordinates.x);
            cubeCoordinates += new Vector3(-(maxVal / 2), -(maxVal / 2), -(maxVal / 2));
            return cubeCoordinates;
        }
        else
        {
            return new Vector3(-1, -1, -1);
        }
    }
    public static Vector3 CubeToSphereCoordinates(Vector3 newLocation)
    {
        Vector3 newPoint = newLocation.normalized * RADIUS;

        newPoint.x = Round(newPoint.x, 1);
        newPoint.y = Round(newPoint.y, 1);
        newPoint.z = Round(newPoint.z, 1);

        float gain = 0.65f;
        float lacunarity = 1.86f;
        int octaves = 5;
        float total = 0.0f;
        float frequency = 1.0f / (float)SIZE;
        float amplitude = gain;
        for(int i = 0;i<octaves;++i)
        {
            total += Noise.Noise.GetOctaveNoise(newPoint.x * frequency, newPoint.y * frequency, newPoint.z * frequency, octaves) * amplitude;
            frequency *= lacunarity;
            amplitude *= gain;
        }


        newPoint+= newPoint*total;
        return newPoint;
    }
    //NEEDS REFINEMENT
    public Grid RawSphereCoordinateToGrid(Vector3 sphereCoordinate)
    {
        sphereCoordinate *= .01f;
        
        Vector3 position = new Vector3(sphereCoordinate.x,sphereCoordinate.y,sphereCoordinate.z);

        double x, y, z;
        x = sphereCoordinate.x;
        y = sphereCoordinate.y;
        z = sphereCoordinate.z;

        double fx, fy, fz;
        fx = (double)Mathf.Abs((float)x);
        fy = (double)Mathf.Abs((float)y);
        fz = (double)Mathf.Abs((float)z);

        const double inverseSqrt2 = 0.70710676908493042;

        if (fy >= fx && fy >= fz)
        {
            double a2 = x * x * 2.0;
            double b2 = z * z * 2.0;
            double inner = -a2 + b2 - 3;
            double innersqrt = -(double)Mathf.Sqrt(((float)inner * (float)inner) - 12.0f * (float)a2);

            if (x == 0.0 || x == -0.0)
                position.x = 0.0f;
            else
                position.x = Mathf.Sqrt((float)innersqrt + (float)a2 - (float)b2 + 3.0f) * (float)inverseSqrt2;
            if (z == 0.0 || z == -0.0)
                position.z = 0.0f;
            else
                position.z = Mathf.Sqrt((float)innersqrt - (float)a2 + (float)b2 + 3.0f) * (float)inverseSqrt2;

            if (position.x > 1.0f) position.x = 1.0f;
            if (position.z > 1.0) position.z = 1.0f;

            if (x < 0) position.x = -position.x;
            if (z < 0) position.z = -position.z;

            if (y > 0)
            {
                //top face
                position.y = 1.0f;
                //return position;
                return gridDictionary[Orientation.Top];
            }
            else
            {
                //bottom face
                position.y = -1.0f;
                //return position;
                return gridDictionary[Orientation.Bottom];
            }
        }
        else if (fx >= fy && fx >= fz)
        {
            double a2 = y * y * 2.0;
            double b2 = z * z * 2.0;
            double inner = -a2 + b2 - 3;
            double innersqrt = -(double)Mathf.Sqrt(((float)inner * (float)inner) - 12.0f * (float)a2);

            if (y == 0.0 || y == -0.0)
                position.y = 0.0f;
            else
                position.y = Mathf.Sqrt((float)innersqrt + (float)a2 - (float)b2 + 3.0f) * (float)inverseSqrt2;
            if (z == 0.0 || z == -0.0)
                position.z = 0.0f;
            else
                position.z = (float)Mathf.Sqrt((float)innersqrt - (float)a2 + (float)b2 + 3.0f) * (float)inverseSqrt2;

            if (position.y > 1.0f) position.y = 1.0f;
            if (position.z > 1.0f) position.z = 1.0f;

            if (y < 0) position.y = -position.y;
            if (z < 0) position.z = -position.z;

            if (x > 0)
            {
                //right face
                position.x = 1.0f;
                //return position;
                return gridDictionary[Orientation.Right];
            }
            else
            {
                //left face
                position.x = -1.0f;
                //return position;
                return gridDictionary[Orientation.Left];
            }
        }
        else
        {
            double a2 = x * x * 2.0;
            double b2 = y * y * 2.0;
            double inner = -a2 + b2 - 3;
            double innersqrt = -(double)Mathf.Sqrt(((float)inner * (float)inner) - 12.0f * (float)a2);

            if (x == 0.0 || x == -0.0)
                position.x = 0.0f;
            else
                position.x = Mathf.Sqrt((float)innersqrt + (float)a2 - (float)b2 + 3.0f) * (float)inverseSqrt2;
            if (y == 0.0 || y == -0.0)
                position.y = 0.0f;
            else
                position.y = Mathf.Sqrt((float)innersqrt - (float)a2 + (float)b2 + 3.0f) * (float)inverseSqrt2;

            if (position.x > 1.0) position.x = 1.0f;
            if (position.y > 1.0) position.y = 1.0f;

            if (x < 0) position.x = -position.x;
            if (y < 0) position.y = -position.y;

            if (z > 0)
            {
                //front face
                position.z = 1.0f;
                //return position;
                return gridDictionary[Orientation.Back];
            }
            else
            {
                //back face
                position.z = -1.0f;
                //return position;
                return gridDictionary[Orientation.Front];
            }
        }
    }
    public void SpawnNodeVisual(Vector3 newLocation)
    {
        GameObject newVisual = Instantiate(nodeVisual, newLocation, Quaternion.identity) as GameObject;
    }
    public void SpawnLocationVisual(Vector3 newLocation)
    {
        GameObject newVisual = Instantiate(locationPrefab, newLocation, Quaternion.identity) as GameObject;
    }

    private void CreateGrids()
    {
        Orientation[] gridOrientations = { Orientation.Top, Orientation.Bottom, Orientation.Right, Orientation.Left, Orientation.Front, Orientation.Back };
        for (int i = 0; i < gridOrientations.Length; i++)
        {
            Grid newGrid = new Grid(gridOrientations[i], SIZE);
            gridDictionary.Add(gridOrientations[i], newGrid);
        }
        
    }
    private void CreateSurface()
    {
        meshGen_BackFace.SetX(SIZE-1);
        meshGen_BackFace.SetZ(SIZE-1);
        meshGen_BackFace.BeginBuild();
        Grid newGrid = gridDictionary[Orientation.Back];
        int i = 0;
        foreach (Node n in newGrid.initNodeDictionary.Values)
        {
            meshGen_BackFace.UpdateMesh(i, n.sphereCoordinates);
            i++;
        }
        meshGen_BackFace.gameObject.AddComponent<MeshCollider>();

        meshGen_FrontFace.SetX(SIZE-1);
        meshGen_FrontFace.SetZ(SIZE-1);
        meshGen_FrontFace.BeginBuild();
        newGrid = gridDictionary[Orientation.Front];
        i = 0;
        foreach (Node n in newGrid.initNodeDictionary.Values)
        {
            meshGen_FrontFace.UpdateMesh(i, n.sphereCoordinates);
            i++;
        }
        meshGen_FrontFace.gameObject.AddComponent<MeshCollider>();

        meshGen_TopFace.SetX(SIZE-1);
        meshGen_TopFace.SetZ(SIZE-1);
        meshGen_TopFace.BeginBuild();
        newGrid = gridDictionary[Orientation.Top];
        i = 0;
        foreach (Node n in newGrid.initNodeDictionary.Values)
        {
            meshGen_TopFace.UpdateMesh(i, n.sphereCoordinates);
            i++;
        }
        meshGen_TopFace.gameObject.AddComponent<MeshCollider>();

        meshGen_BottomFace.SetX(SIZE-1);
        meshGen_BottomFace.SetZ(SIZE-1);
        meshGen_BottomFace.BeginBuild(false);
        newGrid = gridDictionary[Orientation.Bottom];
        i = 0;
        foreach (Node n in newGrid.initNodeDictionary.Values)
        {
            meshGen_BottomFace.UpdateMesh(i, n.sphereCoordinates);
            i++;
        }
        meshGen_BottomFace.gameObject.AddComponent<MeshCollider>();

        meshGen_RightFace.SetX(SIZE-1);
        meshGen_RightFace.SetZ(SIZE-1);
        meshGen_RightFace.BeginBuild();
        newGrid = gridDictionary[Orientation.Right];
        i = 0;
        foreach (Node n in newGrid.initNodeDictionary.Values)
        {
            meshGen_RightFace.UpdateMesh(i, n.sphereCoordinates);
            i++;
        }
        meshGen_RightFace.gameObject.AddComponent<MeshCollider>();

        meshGen_LeftFace.SetX(SIZE-1);
        meshGen_LeftFace.SetZ(SIZE-1);
        meshGen_LeftFace.BeginBuild();
        newGrid = gridDictionary[Orientation.Left];
        i = 0;
        foreach (Node n in newGrid.initNodeDictionary.Values)
        {
            meshGen_LeftFace.UpdateMesh(i, n.sphereCoordinates);
            i++;
        }
        meshGen_LeftFace.gameObject.AddComponent<MeshCollider>();
    }
    public Node GetClosestNode(Vector3 newLocation)
    {
        Grid newGrid = RawSphereCoordinateToGrid(newLocation);//GetGridFrontLocationOnSphere(newLocation);
        NodeCluster newCluster = GetNodeClusterFromGridAndLocation(newLocation, newGrid);
        Node newNode = GetNodeFromNodeClusterAndLocation(newLocation, newCluster);
        return newNode;
    }
    private Grid GetGridFrontLocationOnSphere(Vector3 newLocation)
    {
        //COULD USE A LOT OF WORK

        //Front
        Grid frontGrid = gridDictionary[Orientation.Front];
        float maxZFront = frontGrid.LookUpNode(0, 0).sphereCoordinates.z;
        float minXFrontBack = frontGrid.LookUpNode(0, (frontGrid.gridSize/2)-1).sphereCoordinates.x;
        float maxXFrontBack = frontGrid.LookUpNode(frontGrid.gridSize - 1, (frontGrid.gridSize / 2) - 1).sphereCoordinates.x;
        float minYFrontBack = frontGrid.LookUpNode((frontGrid.gridSize / 2) - 1, 0).sphereCoordinates.y;
        float maxYFrontBack = frontGrid.LookUpNode((frontGrid.gridSize / 2) - 1, frontGrid.gridSize-1).sphereCoordinates.y;

        //Back
        Grid backGrid = gridDictionary[Orientation.Back];
        float minZBack = backGrid.LookUpNode(0,0).sphereCoordinates.z;


        //Top
        Grid topGrid = gridDictionary[Orientation.Top];
        float minYTop = topGrid.LookUpNode(0,0).sphereCoordinates.y;
        float minXTopBottom = topGrid.LookUpNode(0, (topGrid.gridSize / 2) - 1).sphereCoordinates.x;
        float maxXTopBottom = topGrid.LookUpNode(topGrid.gridSize-1,(topGrid.gridSize / 2) - 1).sphereCoordinates.x;
        float minZTopBottom = topGrid.LookUpNode((topGrid.gridSize / 2) - 1, 0).sphereCoordinates.z;
        float maxZTopBottom = topGrid.LookUpNode((topGrid.gridSize / 2) - 1, topGrid.gridSize-1).sphereCoordinates.z;

        //Bottom
        Grid bottomGrid = gridDictionary[Orientation.Bottom];
        float maxYBottom = bottomGrid.LookUpNode(0,0).sphereCoordinates.y;

        //Left
        Grid leftGrid = gridDictionary[Orientation.Left];
        float maxXLeft = leftGrid.LookUpNode(0, 0).sphereCoordinates.x;
        
        //Right
        Grid rightGrid = gridDictionary[Orientation.Right];
        float minXRight = rightGrid.LookUpNode(0, 0).sphereCoordinates.x;
        float minYRightLeft = rightGrid.LookUpNode((rightGrid.gridSize/2)-1,0).sphereCoordinates.y;
        float maxYRightLeft = rightGrid.LookUpNode((rightGrid.gridSize / 2) - 1, rightGrid.gridSize-1).sphereCoordinates.y;
        float minZRightLeft = rightGrid.LookUpNode(0, (rightGrid.gridSize / 2) - 1).sphereCoordinates.z;
        float maxZRightLeft = rightGrid.LookUpNode(rightGrid.gridSize - 1, (rightGrid.gridSize / 2) - 1).sphereCoordinates.z;

        float x = newLocation.x;
        float y = newLocation.y;
        float z = newLocation.z;

        Grid newGrid=null;

        if (y >= minYTop)
        {
            if (x >= minXTopBottom && x <= maxXTopBottom)
            {
                if (z >= minZTopBottom && z <= maxZTopBottom)
                {
                    //Top
                    newGrid= gridDictionary[Orientation.Top];
                }

            }
            
        }
        if (y <= maxYBottom)
        {
            if (x >= minXTopBottom && x <= maxXTopBottom)
            {
                if (z >= minZTopBottom && z <= maxZTopBottom)
                {
                    //Bottom
                    newGrid = gridDictionary[Orientation.Bottom];
                }

            }
        }
        if (z <= maxZFront)
        {
            if (x >= minXFrontBack && x <= maxXFrontBack)
            {
                if (y >= minYFrontBack && y <= maxYFrontBack)
                {
                    //Front
                    newGrid = gridDictionary[Orientation.Front];
                }
                
            }
             
        }
        if (z >= minZBack)
        {
           
            if (x >= minXFrontBack && x <= maxXFrontBack)
            {
                if (y >= minYFrontBack && y <= maxYFrontBack)
                {
                    //Back
                    newGrid = gridDictionary[Orientation.Back];
                }
                
            }
            
        }
        if (x >= minXRight)
        {
            if (z >= minZRightLeft && z <= maxZRightLeft)
            {
                if (y >= minYRightLeft && y <= maxYRightLeft)
                {
                    //Right
                    newGrid = gridDictionary[Orientation.Right];
                }

            }
            
        }
        if (x <= maxXLeft)
        {
            if (z >= minZRightLeft && z <= maxZRightLeft)
            {
                if (y >= minYRightLeft && y <= maxYRightLeft)
                {
                    //Left
                    newGrid = gridDictionary[Orientation.Left];
                }

            }
            
        }
        if (newGrid == null)
        {
            Debug.LogError("Could Not Find Grid!");
        }
        return newGrid;
    }
    private NodeCluster GetNodeClusterFromGridAndLocation(Vector3 newLocation, Grid newGrid)
    {
        NodeCluster closestCluster = null;
        float closestDist = 1000000;
        foreach (NodeCluster newCluster in newGrid.abstractGrid.nodeClusterDictionary.Values)
        {
            float tempDist = Vector3.Distance(newLocation, newCluster.centerSphereCoordinates);
            if (tempDist < closestDist)
            {
                closestDist = tempDist;
                closestCluster = newCluster;
            }
        }
        return closestCluster;
    }
    private Node GetNodeFromNodeClusterAndLocation(Vector3 newLocation, NodeCluster newCluster)
    {
        Vector3 startLocation = newCluster.gridCoordinates;
        Vector3 endLocation = newCluster.gridCoordinates + new Vector3(newCluster.clusterSize, newCluster.clusterSize, newCluster.clusterSize);

        Node closestNode = null;
        Grid newGrid = newCluster.mainAbstractGrid.mainGrid;
        float closestDist = 100000000;
        for (int i = (int)startLocation.x; i < endLocation.x; i++)
        {
            for (int j = (int)startLocation.z; j < endLocation.z; j++)
            {
                Node tempNode = newGrid.LookUpNode(i, j);
                float tempDist = Vector3.Distance(newLocation, tempNode.sphereCoordinates);
                if (tempDist < closestDist)
                {
                    closestDist = tempDist;
                    closestNode = tempNode;
                }
            }
        }
        return closestNode;
    }
    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
    public void ManageNodeList(Node refNode, bool add = true)
    {
        
        if (refNode != null)
        {
            
            if (add && !refNode.IsTemporary())
            {

                bool isValid = true;
                //Check if node already exists 
                string tempKey = GetNodeKey(refNode);
                Node tempNode = LookUpNode(tempKey);
                if (tempNode != null)
                    isValid = false;

                if (isValid)
                {
                    
                    
                    if (refNode.nodesConnectingTo != null)
                    {
                        //ISSUE WITH refNode nodesConnectingTo
                        Node newNode = new Node(refNode.gridParent, refNode.gridCoordinates, NodeType.Abstract, nodeCounter);
                        for (int i = 0; i < refNode.nodesConnectingTo.Count; i++)
                        {
                            newNode.ManageNodesConnectingTo(refNode.nodesConnectingTo[i]);
                        }
                        nodeCounter++;
                        string newKey = GetNodeKey(newNode);
                        nodeDictionary.Add(newKey, newNode);

                    }
                    
                }

            }
            else if (add && refNode.IsTemporary())
            {
                //If an Inserted Node
                if (!nodeDictionary.ContainsValue(refNode))
                {
                    string newKey = GetNodeKey(refNode);
                    nodeDictionary.Add(newKey, refNode);
                }
            }
            else
            {
                //If Remove
                string newKey = GetNodeKey(refNode);
                if (nodeDictionary.ContainsKey(newKey))
                    nodeDictionary.Remove(newKey);
                if (refNode.IsTemporary())
                    refNode = null;
            }
        }

    }
    public static string GetNodeKey(Node newNode)
    {
        string key = newNode.cubeCoordinates.ToString() + newNode.gridParent.gridOrientation.ToString();
        return key;
    }
    public Node LookUpNode(string newKey)
    {
        if (nodeDictionary.ContainsKey(newKey))
            return nodeDictionary[newKey];
        else
            return null;
    }
    private void SetAllGridConnections()
    {
        foreach (Grid currentGrid in gridDictionary.Values)
        {
            for (int i = 0; i < currentGrid.abstractGrid.connectionNodes.Count; i++)
            {
                Node currentNode = currentGrid.abstractGrid.connectionNodes[i];
                
                foreach (Grid tempGrid in gridDictionary.Values)
                {
                    if (currentGrid != tempGrid)
                    {
                        for (int j = 0; j < tempGrid.abstractGrid.connectionNodes.Count; j++)
                        {
                            Node tempNode = tempGrid.abstractGrid.connectionNodes[j];
                            if (tempNode.cubeCoordinates == currentNode.cubeCoordinates)
                            {
                                currentNode.ManageNodesConnectingTo(tempNode);
                            }
                        }
                    }
                }
                ManageNodeList(currentNode);
            }
        }
    }
    public void SetConnectionNodeNeighbors()
    {
        
        foreach(Node newNode in nodeDictionary.Values)
        {
            /*for (int i = 0; i < newNode.nodesConnectingTo.Count; i++)
            {
                string newKey = GetNodeKey(newNode.nodesConnectingTo[i]);
                Node newNeighbor = LookUpNode(newKey);
                newNode.AddNeighbor(newNeighbor);
                
            }
            foreach (Node newNode2 in nodeDictionary.Values)
            {
                if (newNode != newNode2 && newNode.gridParent == newNode2.gridParent)
                {
                    newNode.AddNeighbor(newNode2);
                }
            }*/
            foreach (Node tempNode in nodeDictionary.Values)
            {
                if (tempNode.gridParent == newNode.gridParent || tempNode.cubeCoordinates == newNode.cubeCoordinates)
                {
                    if (tempNode != newNode)
                    {
                        newNode.AddNeighbor(tempNode);
                    }
                }
            }
        }
    }
    private void SetConnections()
    {
        //OLD VERSION
        /*for (int i = 0; i < gridList.Count; i++)
        {
            for (int j = 0; j < gridList[i].connectionNodes.Count; j++)
            {
                Node startNode = gridList[i].connectionNodes[j];
                for (int k = 0; k < gridList[i].connectionNodes.Count; k++)
                {
                    Node endNode = gridList[i].connectionNodes[k];
                    if (startNode != endNode)
                    {
                        StorePath(startNode, endNode);
                    }
                }
            }
        }*/
        //NEW VERSION
        foreach (Grid g in gridDictionary.Values)
        {
            for (int i = 0; i < g.abstractGrid.connectionNodes.Count; i++)
            {
                Node startNode = g.abstractGrid.connectionNodes[i];
                for (int j = 0; j < g.abstractGrid.connectionNodes.Count; j++)
                {
                    Node endNode = g.abstractGrid.connectionNodes[j];
                    if (startNode != endNode)
                    {
                        StorePath(startNode, endNode);
                    }
                }
            }
        }
    }
    public string GetConnectionKey(Node startNode, Node endNode)
    {
        return (startNode.nodeNum + (endNode.nodeNum * 1000)).ToString()+startNode.gridParent.gridOrientation.ToString();
    }
    public List<Node> GetStoredPath(Node startNode, Node endNode)
    {
        string connectionKey = GetConnectionKey(startNode, endNode);
        if (storedPathsDictionary.ContainsKey(connectionKey))
        {
            List<Node> newPath = storedPathsDictionary[connectionKey];
            return newPath;
        }
        else
            return null;
    }
    public void StorePath(Node startNode, Node endNode)
    {
        if (startNode.gridParent != endNode.gridParent)
            return;

        List<Node> newPath = startNode.gridParent.abstractGrid.FindAbstractPath(startNode, endNode);
        string connectionKey = GetConnectionKey(startNode, endNode);
        if (!storedPathsDictionary.ContainsKey(connectionKey) && newPath != null)
        {
            storedPathsDictionary.Add(connectionKey, newPath);
        }
    }
    public List<Node> FindSpherePath(Node startNode, Node endNode)
    {
        List<Node> newPath = new List<Node>();

        if (startNode.gridParent != endNode.gridParent)
        {
            List<Node> newConnectionPath = FindConnectionPath(startNode, endNode);
            if (newConnectionPath == null)
            {
                Debug.LogError("MultiGridPath failed");
                return null;
            }
            for (int i = 0; i < newConnectionPath.Count - 1; i++)
            {
                Node sNode = newConnectionPath[i];
                Node eNode = newConnectionPath[i + 1];
                if (sNode.gridParent == eNode.gridParent)
                {
                    Grid newGrid = sNode.gridParent;

                    sNode = newGrid.LookUpNode(sNode.xVal, sNode.zVal);
                    eNode = newGrid.LookUpNode(eNode.xVal, eNode.zVal);

                    List<Node> addedPath = newGrid.FindComplexPath(sNode, eNode);
                    if (addedPath != null)
                    {
                        //Form Path
                        //Remove Duplicates
                        if (newPath.Count > 0)
                        {
                            Node n1 = newPath[newPath.Count - 1];
                            Node n2 = addedPath[0];
                            if (n1.cubeCoordinates == n2.cubeCoordinates)
                            {
                                newPath.RemoveAt(newPath.Count - 1);
                            }
                            newPath.AddRange(addedPath);
                        }
                        else
                            newPath.AddRange(addedPath);
                    }
                        
                }
            }
        }
        else
        {
            Grid newGrid = startNode.gridParent;
            newPath = newGrid.FindComplexPath(startNode, endNode);
        }
        
        
        return newPath;
    }

    public List<Node> FindConnectionPath(Node sNode, Node eNode)
    {
        string startNodeKey = GetNodeKey(sNode);
        string endNodeKey = GetNodeKey(eNode);

        bool startNodeInserted = (LookUpNode(startNodeKey)/*this.LookUpNode((int)sNode.xVal, (int)sNode.zVal)*/ == null);
        bool endNodeInserted = (LookUpNode(endNodeKey)/*this.LookUpNode((int)eNode.xVal, (int)eNode.zVal)*/ == null);



        Node startNode = startNodeInserted ? this.InsertNode(sNode) : LookUpNode(startNodeKey)/*this.LookUpNode((int)sNode.xVal, (int)sNode.zVal)*/;
        Node endNode = endNodeInserted ? this.InsertNode(eNode) : LookUpNode(endNodeKey)/*this.LookUpNode((int)eNode.xVal, (int)eNode.zVal)*/;





        frontierHeap.Add(startNode);
        gDist = 0;

        while (frontierHeap.Count > 0)
        {

            Node currentNode = frontierHeap.Remove();
            //SpawnNodeVisual(currentNode.cubeCoordinates);
/*
            if (currentNode.visited)
                break;*/
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
                newPath.Add(curNode);
                curNode = curNode.previouseNode;
            }
            newPath.Add(curNode);
            newPath.Reverse();

            if (startNodeInserted)
                RemoveNode(startNode);
            if (endNodeInserted)
                RemoveNode(endNode);
            ResetConnectionGrid();

            return newPath;
        }
        else
        {
            Debug.Log("Connection Path is NULL");

            if (startNodeInserted)
            {
                RemoveNode(startNode);
            }

            if (endNodeInserted)
            {
                RemoveNode(endNode);
            }

            ResetConnectionGrid();

            return null;

        }
    }
    public void ResetConnectionGrid()
    {
        foreach (Node newNode in nodeDictionary.Values)
        {
            newNode.Reset();
        }
        frontierHeap.Clear();
    }
    public Node InsertNode(Node refNode)
    {
        //TEMPORARY
        /*Node newNode = new Node(refNode.gridParent, refNode.gridCoordinates, NodeType.Temporary, -1);
        for (int i = 0; i < newNode.gridParent.connectionNodes.Count; i++)
        {
            Node newNeighbor = LookUpNode((int)newNode.gridParent.connectionNodes[i].xVal, (int)newNode.gridParent.connectionNodes[i].zVal);
            newNode.AddNeighbor(newNeighbor);
            newNeighbor.AddNeighbor(newNode);
        }
        ManageNodeList(newNode);
        
        return newNode;*/

        Node newNode = new Node(refNode.gridParent, refNode.gridCoordinates, NodeType.Temporary, -1);

        for (int i = 0; i < newNode.gridParent.abstractGrid.connectionNodes.Count; i++)
        {
            string key = GetNodeKey(newNode.gridParent.abstractGrid.connectionNodes[i]);
            Node newNeighbor = LookUpNode(key);//LookUpNode((int)newNode.gridParent.abstractGrid.connectionNodes[i].xVal, (int)newNode.gridParent.connectionNodes[i].zVal);
            if (newNeighbor != null)
            {
                newNode.AddNeighbor(newNeighbor);
                newNeighbor.AddNeighbor(newNode);
            }
            
        }
        ManageNodeList(newNode);
        
        return newNode;


    }
    public void RemoveNode(Node newNode)
    {
        if (newNode.IsTemporary())
        {
            foreach (Node n in nodeDictionary.Values)
            {
                Node removeNode = null;
                for (int j = 0; j < n.neighbors.Count; j++)
                {
                    if (n.neighbors[j] == newNode)
                        removeNode = newNode;
                }
                if (removeNode != null)
                    n.neighbors.Remove(removeNode);
            }
            ManageNodeList(newNode, false);
        }

    }
}
