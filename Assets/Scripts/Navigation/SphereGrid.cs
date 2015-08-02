using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Noise;


public class SphereGrid : MonoBehaviour 
{
    public Dictionary<string, List<Node>> storedPathsDictionary = new Dictionary<string, List<Node>>();
    private BinaryHeap<Node> frontierHeap = new BinaryHeap<Node>();

    private const float spacing = 1;
    public int nodeCounter = 0;

    private float gDist = 0;
    public float gDistInc = .01f;

    public Dictionary<string, Node> nodeDictionary = new Dictionary<string, Node>();

    //MAY NOT NEED THESE
    private Dictionary<Vector3, Node> cubeNodeDictionary = new Dictionary<Vector3, Node>();
    public Dictionary<Vector3, Node> sphereNodeDictionary = new Dictionary<Vector3, Node>();

    //public int gridSize = 30;


    public GameObject nodeVisual;
    public GameObject locationPrefab;
    public GameObject nodeClusterVisual;

    private const int SIZE = 20;
    private const float RADIUS = 10;

    private List<Vector3> cubeCoordinates = new List<Vector3>();
    private List<Vector3> sphereCoordinates = new List<Vector3>();

    public SphereCollider planetCollider;

    public Dictionary<Orientation, Grid> gridDictionary = new Dictionary<Orientation, Grid>();
    public Dictionary<FaceType, SphereFace> sphereFaceDictionary = new Dictionary<FaceType, SphereFace>();
    public List<Node> cornerNodes = new List<Node>();

    private GridAgent agent;
    public GameObject agentPrefab;

    public MeshGenerator meshGen_BackFace;
    public MeshGenerator meshGen_FrontFace;
    public MeshGenerator meshGen_TopFace;
    public MeshGenerator meshGen_BottomFace;
    public MeshGenerator meshGen_RightFace;
    public MeshGenerator meshGen_LeftFace;



    private int index = 0;
    private List<Node> mainPath = new List<Node>();

	void Start () 
    {
        InitializeGrid();
        /*
         * Bring ConnectionGrid functionality to SphereGrid!!!!!!!!!!!!!!!!!!!!!!!!
         */ 
	}
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == planetCollider)
                {
                    
                    
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnNodeVisual(mainPath[index].sphereCoordinates);
            if(index<mainPath.Count-1)
                index++;
        }
        
    }
    private void InitializeGrid()
    {
        //CreateHeightMap();
        GenerateCubePoints();
        CreateGrids();
        SetAllGridConnections();
        SetConnectionNodeNeighbors();
        SetConnections();

        /*
         * CURRENT:
         *  Have created all Grids
         *  All Grids are Initialized as a normal Grid (as in previous projects) for simplicity
         *  
         * To Do:
         *  
         */

        //return;


        Node startNode = gridDictionary[Orientation.Front].LookUpNode(7,4);
        Node endNode = gridDictionary[Orientation.Back].LookUpNode(3,3);

        SpawnLocationVisual(startNode.sphereCoordinates);
        SpawnLocationVisual(endNode.sphereCoordinates);

        mainPath = FindMultiGridPath(startNode,endNode);
        



        return;
        

        //AssignAllNeighboors();
        //CreateSphereFaces();
        //SetNodeFaceParents();
        //GenerateSpherePoints();
        CreateSurface();

        
        /*foreach (Node n in sphereNodeDictionary.Values)
        {
            SpawnNodeVisual(n.GetLocation());
        }*/
        

        
        
    
        

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

        /*float noiseDensity = 2f;
        Vector3 v = newPoint;
        v.Scale(new Vector3(noiseDensity, noiseDensity, noiseDensity));
        float scale = .15f;
        float noise = Noise.Noise.GetOctaveNoise(v.x, v.y, v.z, 4) * scale;
        float factor = 1f - (scale / 2f) + noise;

        newPoint += newPoint * addedNoise*.1f;//(newPoint - transform.position) * addedNoise*.05f;
        newPoint = Vector3.Scale(newPoint, new Vector3(factor, factor, factor));*/

        return newPoint;
    }
    //NEEDS REFINEMENT
    public static Vector3 RawSphereCoordinateToRawCubeCoordinate(Vector3 sphereCoordinate)
    {
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
                return position;
            }
            else
            {
                //bottom face
                position.y = -1.0f;
                return position;
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
                return position;
            }
            else
            {
                //left face
                position.x = -1.0f;
                return position;
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
                return position;
            }
            else
            {
                //back face
                position.z = -1.0f;
                return position;
            }
        }
    }
    private void SpawnNodeVisual(Vector3 newLocation)
    {
        GameObject newVisual = Instantiate(nodeVisual, newLocation, Quaternion.identity) as GameObject;
    }
    private void SpawnLocationVisual(Vector3 newLocation)
    {
        GameObject newVisual = Instantiate(locationPrefab, newLocation, Quaternion.identity) as GameObject;
    }
    private void SpawnNodeClusterVisual(Vector3 newLocation)
    {
        GameObject newVisual = Instantiate(nodeClusterVisual, newLocation, Quaternion.identity) as GameObject;
    }
    private void VisualizePath(List<Node> newPath)
    {
        for (int i = 0; i < newPath.Count; i++)
        {
            GameObject newVisual = Instantiate(nodeVisual, newPath[i].GetLocation(), Quaternion.identity) as GameObject;
        }
    }
    
    private void CreateSphereFaces()
    {
        FaceType[] type = { FaceType.Top,FaceType.Bottom,FaceType.Right,FaceType.Left,FaceType.Front,FaceType.Back };
        for (int i = 0; i < type.Length; i++)
        {
            SphereFace newFace = new SphereFace(type[i]);
            sphereFaceDictionary.Add(type[i], newFace);
        }
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
    private void SetNodeFaceParents()
    {
        /*int maxVal = SIZE / 2;
        //Front
        for (int i = -maxVal; i <= maxVal; i++)
        {
            for (int j = -maxVal; j <= maxVal; j++)
            {
                Vector3 key = new Vector3(i, j, maxVal);
                if (cubeNodeDictionary.ContainsKey(key))
                {
                    sphereFaceDictionary[FaceType.Front].ManageNodeList(cubeNodeDictionary[key]);
                }
            }
        }
        //Back
        for (int i = -maxVal; i <= maxVal; i++)
        {
            for (int j = -maxVal; j <= maxVal; j++)
            {
                Vector3 key = new Vector3(i, j, -maxVal);
                if (cubeNodeDictionary.ContainsKey(key))
                {
                    sphereFaceDictionary[FaceType.Back].ManageNodeList(cubeNodeDictionary[key]);
                }
            }
        }
        //Top
        for (int i = -maxVal; i <= maxVal; i++)
        {
            for (int j = -maxVal; j <= maxVal; j++)
            {
                Vector3 key = new Vector3(i, maxVal, j);
                if (cubeNodeDictionary.ContainsKey(key))
                {
                    sphereFaceDictionary[FaceType.Top].ManageNodeList(cubeNodeDictionary[key]);
                }
            }
        }
        //Bottom
        for (int i = -maxVal; i <= maxVal; i++)
        {
            for (int j = -maxVal; j <= maxVal; j++)
            {
                Vector3 key = new Vector3(i, -maxVal, j);
                if (cubeNodeDictionary.ContainsKey(key))
                {
                    sphereFaceDictionary[FaceType.Bottom].ManageNodeList(cubeNodeDictionary[key]);
                }
            }
        }
        //Right
        for (int i = -maxVal; i <= maxVal; i++)
        {
            for (int j = -maxVal; j <= maxVal; j++)
            {
                Vector3 key = new Vector3(maxVal, i, j);
                if (cubeNodeDictionary.ContainsKey(key))
                {
                    sphereFaceDictionary[FaceType.Right].ManageNodeList(cubeNodeDictionary[key]);
                }
            }
        }
        //Left
        for (int i = -maxVal; i <= maxVal; i++)
        {
            for (int j = -maxVal; j <= maxVal; j++)
            {
                Vector3 key = new Vector3(-maxVal, i, j);
                if (cubeNodeDictionary.ContainsKey(key))
                {
                    sphereFaceDictionary[FaceType.Left].ManageNodeList(cubeNodeDictionary[key]);
                }
            }
        }*/
    }
    private void CreateSurface()
    {
        meshGen_BackFace.SetX(SIZE);
        meshGen_BackFace.SetZ(SIZE);
        meshGen_BackFace.BeginBuild();
        SphereFace newFace = sphereFaceDictionary[FaceType.Back];
        for (int i = 0; i < newFace.nodeList.Count; i++)
        {
            meshGen_BackFace.UpdateMesh(i, newFace.nodeList[i].GetLocation());
        }
        meshGen_FrontFace.SetX(SIZE);
        meshGen_FrontFace.SetZ(SIZE);
        meshGen_FrontFace.BeginBuild(false);
        newFace = sphereFaceDictionary[FaceType.Front];
        for (int i = 0; i < newFace.nodeList.Count; i++)
        {
            meshGen_FrontFace.UpdateMesh(i, newFace.nodeList[i].GetLocation());
        }
        meshGen_TopFace.SetX(SIZE);
        meshGen_TopFace.SetZ(SIZE);
        meshGen_TopFace.BeginBuild();
        newFace = sphereFaceDictionary[FaceType.Top];
        for (int i = 0; i < newFace.nodeList.Count; i++)
        {
            meshGen_TopFace.UpdateMesh(i, newFace.nodeList[i].GetLocation());
        }
        meshGen_BottomFace.SetX(SIZE);
        meshGen_BottomFace.SetZ(SIZE);
        meshGen_BottomFace.BeginBuild(false);
        newFace = sphereFaceDictionary[FaceType.Bottom];
        for (int i = 0; i < newFace.nodeList.Count; i++)
        {
            meshGen_BottomFace.UpdateMesh(i, newFace.nodeList[i].GetLocation());
        }
        meshGen_RightFace.SetX(SIZE);
        meshGen_RightFace.SetZ(SIZE);
        meshGen_RightFace.BeginBuild(false);
        newFace = sphereFaceDictionary[FaceType.Right];
        for (int i = 0; i < newFace.nodeList.Count; i++)
        {
            meshGen_RightFace.UpdateMesh(i, newFace.nodeList[i].GetLocation());
        }
        meshGen_LeftFace.SetX(SIZE);
        meshGen_LeftFace.SetZ(SIZE);
        meshGen_LeftFace.BeginBuild();
        newFace = sphereFaceDictionary[FaceType.Left];
        for (int i = 0; i < newFace.nodeList.Count; i++)
        {
            meshGen_LeftFace.UpdateMesh(i, newFace.nodeList[i].GetLocation());
        }
    }
    private SphereFace GetFace(Node newNode)
    {
        Vector3 newLocation = newNode.GetLocation();

        Vector3 cubeNodeLocation = new Vector3(SIZE / 2, SIZE / 2, SIZE / 2);

        Node cubeNode = GetCubeNode(cubeNodeLocation);

        float maxVal = cubeNode.GetLocation().x;

        //float PREVIOUS_NUM = 5.8f;

        if (newLocation.y >= maxVal)
        {
            //TopFace
            return sphereFaceDictionary[FaceType.Top];
        }
        if (newLocation.y <= -maxVal)
        {
            //BottomFace
            return sphereFaceDictionary[FaceType.Bottom];
        }
        if (newLocation.x >= maxVal)
        {
            //RightFace
            return sphereFaceDictionary[FaceType.Right];
        }
        if (newLocation.x <= -maxVal)
        {
            //LeftFace
            return sphereFaceDictionary[FaceType.Left];
        }
        if (newLocation.z >= maxVal)
        {
            //FrontFace
            return sphereFaceDictionary[FaceType.Front];
        }
        if (newLocation.z <= -maxVal)
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
    private Node GetSphereNodeFromCubeNode(Node newCubeNode)
    {
        Vector3 sphereNodeLocation = CubeToSphereCoordinates(newCubeNode.cubeCoordinates);
        return GetSphereNode(sphereNodeLocation);
    }
    private Node GetCubeNode(Vector3 newLocation)
    {
        if (cubeNodeDictionary.ContainsKey(newLocation))
        {
            return cubeNodeDictionary[newLocation];
        }
        else
            return null;
    }
    private Node GetSphereNode(Vector3 newLocation)
    {
        if (sphereNodeDictionary.ContainsKey(newLocation))
        {
            return sphereNodeDictionary[newLocation];
        }
        else
            return null;
    }
    private void GenerateCubePoints()
    {
        //TEMPORARY!!!

        /*int minVal = -(SIZE / 2);
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
            if (!cubeNodeDictionary.ContainsKey(cubeCoordinates[i]))
            {
                
                Node newNode = new Node(this, cubeCoordinates[i], NodeType.Normal, nodeCounter);
                cubeNodeDictionary.Add(cubeCoordinates[i], newNode);
                nodeCounter++;
            }
        }*/
    }
    private void GenerateSpherePoints()
    {
        foreach (Node n in cubeNodeDictionary.Values)
        {
            //Temporarily disabled!
            Vector3 newLocation = n.GetLocation();//CubeToSphereCoordinates(n.GetLocation());


            /*newLocation.x = Round(newLocation.x, 1);
            newLocation.y = Round(newLocation.y, 1);
            newLocation.z = Round(newLocation.z, 1);

            float addedNoise = SimplexNoise.Noise.Generate(newLocation.x, newLocation.y, newLocation.z);
            float addedVal = .05f+.05f * addedNoise;
            newLocation += (newLocation - transform.position) * addedVal;*/

            n.SetLocation(newLocation);
            if(!sphereNodeDictionary.ContainsKey(newLocation))
                sphereNodeDictionary.Add(newLocation, n);
            //SpawnNodeVisual(n.GetLocation());
        }
        //tempNodeDictionary.Clear();
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
    
    
    
   
    
   
   
    
    
    private void ResetGrid()
    {
        foreach (Node n in sphereNodeDictionary.Values)
        {
            n.Reset();
        }
        frontierHeap.Clear();
        
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
    public List<Node> FindMultiGridPath(Node startNode, Node endNode)
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
