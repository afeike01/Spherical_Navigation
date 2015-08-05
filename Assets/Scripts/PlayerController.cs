using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour 
{

    public SphereGrid mainGrid;

    //private List<GridAgent> agents = new List<GridAgent>();
    public GameObject agentPrefab;
    public int agentCount = 0;
    public Text agentCountText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        //agentCount = agents.Count;
        agentCountText.text = "Unit Count: " + agentCount.ToString();
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {

                if (hit.collider.gameObject.GetComponent<MeshCollider>())
                {

                    Node newNode = mainGrid.GetClosestNode(hit.point);
                    mainGrid.SpawnNodeVisual(newNode.sphereCoordinates);
                    mainGrid.SpawnLocationVisual(hit.point);

                    mainGrid.targetNode = newNode;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameObject newAgentPrefab = Instantiate(agentPrefab, new Vector3(0, 25, 0), Quaternion.identity) as GameObject;

            GridAgent newAgent = newAgentPrefab.GetComponent<GridAgent>();
            newAgent.Initialize(mainGrid);
            agentCount++;
            
            //agents.Add(newAgent);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }
	}
}
