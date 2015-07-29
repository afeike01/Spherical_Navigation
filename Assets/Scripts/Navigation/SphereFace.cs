using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FaceType : int
{
    Top = 0,
    Bottom = 1,
    Right =2,
    Left = 3,
    Front = 4,
    Back = 5,
}
public class SphereFace
{
    public List<Node> nodeList = new List<Node>();
    public FaceType sphereFaceType;

    public SphereFace(FaceType newType)
    {
        sphereFaceType = newType;

    }
    public bool ManageNodeList(Node newNode)
    {

        if (!nodeList.Contains(newNode))
        {
            nodeList.Add(newNode);
            return true;
        }
        else
            return false;
    }
}
