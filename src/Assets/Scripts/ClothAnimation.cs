using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;


public class ClothAnimation : MonoBehaviour {

	private GameObject dressModel; 
	private Mesh dressModelMesh;
	public ObiSolver solver;				/**< solver to add the sack to.*/
	public ObiMeshTopology topology;		/**< empty topology asset used to store sack mesh information.*/

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GenerateCloth (GameObject model) {

		//dressModel = GameObject.Find (dressName);
//		dressModelMesh =  dressModel.transform.GetChild(0);
		//Debug.Log (dressModelMesh);
		//dressModel = new Mesh (model.GetComponent InChildren<Mesh>);

		RuntimeClothGenerator generator = dressModel.AddComponent<RuntimeClothGenerator>();
		generator.solver = solver;
		generator.topology = topology;
		generator.mesh = dressModelMesh;

		ObiCloth cloth = dressModel.GetComponent<ObiCloth>();

		cloth.BendingConstraints.maxBending = 0.02f;
	}
}
