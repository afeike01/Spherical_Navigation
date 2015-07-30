using UnityEngine;
using System.Collections;

public class MeshGenerator : MonoBehaviour 
{
    public float m_Length;
    public float m_Width;
    public int m_SegmentCount;

    public int xVal;
    public int zVal;

    
    
    MeshBuilder meshBuilder;
    //Mesh myMesh;
    MeshFilter filter;

    Vector3[] allVerts;
    private float offset = .025f;
    public float xOffset;
    public float zOffset;
    public bool moveArea = false;
    public Material[] allMats = new Material[3];
	// Use this for initialization
	void Start () 
    {
           
	}
    public void BeginBuild(bool reverse = true)
    {
        
        meshBuilder = GetComponent<MeshBuilder>();
       
        
        
        filter = GetComponent<MeshFilter>();
        
        
        
        
        for (int i = 0; i <= xVal; i++)//xVal
        {
            float z = m_Length * i;
            float v = (1.0f / xVal) * i;//xVal
            for (int j = 0; j <= zVal; j++)//zVal
            {
                float x = m_Width * j;
                float u = (1.0f / zVal) * j;//zval
                Vector3 offset = new Vector3(x, 0, z);
                Vector2 uv = new Vector2(u, v);
                bool buildTriangles = i > 0 && j > 0;
                BuildQuadForGrid(meshBuilder, offset, uv, buildTriangles, zVal + 1);//zval
            }
        }


        if (filter != null)
        {

            filter.sharedMesh = meshBuilder.CreateMesh(reverse);
            
            //Debug.Log(filter.sharedMesh.vertexCount + " Vertices.");
        }


        
        
    }
    void BuildQuadForGrid(MeshBuilder meshBuilder, Vector3 position, Vector2 uv, bool buildTriangles, int vertsPerRow)
    {
        meshBuilder.Vertices.Add(position);
        meshBuilder.UVs.Add(uv);
        if (buildTriangles)
        {
            
            int baseIndex = meshBuilder.Vertices.Count - 1;
            int index0 = baseIndex;
            int index1 = baseIndex - 1;
            int index2 = baseIndex - vertsPerRow;
            int index3 = baseIndex - vertsPerRow - 1;

            //meshBuilder.AddTriangle(index2, index3, index1);
            //meshBuilder.AddTriangle(index0, index2, index1);
            meshBuilder.AddTriangle(index0, index2, index1);
            meshBuilder.AddTriangle(index2, index3, index1);
            //testing something
        }
    }
    public void UpdateMesh(int index, Vector3 pos)
    {
        Mesh myMesh = filter.mesh;
        

        allVerts = myMesh.vertices;

        pos -= (pos - transform.root.position) * offset;

        //pos.y -= offset;
        if (moveArea)
        {
            pos.x -= xOffset;
            pos.z -= zOffset;
        }
        allVerts[index] = pos;
        myMesh.vertices = allVerts;
        
        myMesh.RecalculateBounds();
        myMesh.RecalculateNormals();
        
        
    }
    public void SetX(int newX)
    {
        xVal = newX;
    }
    public void SetZ(int newZ)
    {
        zVal = newZ;
    }
    
	
}
