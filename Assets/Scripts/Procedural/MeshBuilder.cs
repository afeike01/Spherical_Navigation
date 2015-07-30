using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshBuilder : MonoBehaviour 
{
    

    private List<Vector3> m_Vertices = new List<Vector3>();
    public List<Vector3> Vertices { get { return m_Vertices; } }

    private List<Vector3> m_Normals = new List<Vector3>();
    public List<Vector3> Normals { get { return m_Normals; } }

    private List<Vector2> m_UVs = new List<Vector2>();
    public List<Vector2> UVs { get { return m_UVs; } }

    private List<int> m_Indices = new List<int>();

    public Mesh mesh;
    public void AddTriangle(int index0, int index1, int index2)
    {
        m_Indices.Add(index0);
        m_Indices.Add(index1);
        m_Indices.Add(index2);
    }
    public Mesh CreateMesh(bool reverse = true)
    {
        
        mesh = new Mesh();
        mesh.vertices = m_Vertices.ToArray();
        //EXPERIMENTAL
        if(reverse)
            m_Indices.Reverse();
        mesh.triangles = m_Indices.ToArray();
        //Normals are optiona. only use them if we ahve the correct amount;
        if (m_Normals.Count == m_Vertices.Count)
            mesh.normals = m_Normals.ToArray();
        if (m_UVs.Count == m_Vertices.Count)
            mesh.uv = m_UVs.ToArray();
        mesh.RecalculateBounds();
        return mesh;
    }
    public void UpdateMesh()
    {
        mesh.RecalculateBounds();
    }
	// Use this for initialization
	void Start () 
    {
        //CallBuild();	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
