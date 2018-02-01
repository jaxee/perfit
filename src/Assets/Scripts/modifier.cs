using UnityEngine;
using System.Collections;

public class modifier : MonoBehaviour
{

	//The subdivision size of lattice 
	public int L=2, M=2, N=2; 
	//lattice size
	private Vector3 S,T,U;
	//parameterized points 
	private float s=0.0f, t=0.0f, u=0.0f;
	//FFD lattice origin 
	private Vector3 P0;
	//target mesh 
	public GameObject target;
	private SkinnedMeshRenderer render; 
	private Mesh copyMesh;
	private Vector3[] _vrts;
	private Vector3[] normals; 
	//sets prefab control point
	public GameObject CtrlPnt;
	//hold all generated control points 
	private GameObject[,,] ctrlPoints;

	//set gobal mesh information first 
	void Awake(){
	
		render 		= target.GetComponent<SkinnedMeshRenderer> (); 
		copyMesh	= (Mesh) Instantiate (render.sharedMesh); 
		_vrts 		= new Vector3[copyMesh.vertices.Length];
		P0 = transform.position;
		S = new Vector3(1.0f, 0.0f, 0.0f);
		T = new Vector3(0.0f, 1.0f, 0.0f);
		U = new Vector3(0.0f, 0.0f, 1.0f);
	}

	// Use this for initialization
	void Start (){
		GenGrid ();
	}
	
	// Update is called once per frame
	void Update (){
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 500.0f))
			{
				MeshUpdate (Parameterize());
			}
		}
	}
		
	//set points relative to ffd 
	Vector3[] Parameterize(){

		Vector3 npt = Vector3.zero;
		for(int v=0; v < copyMesh.vertexCount; v++){
			s = ConvertPoint (T, U, S, copyMesh.vertices[v], P0);
			t = ConvertPoint (S, U, T, copyMesh.vertices[v], P0);
			u = ConvertPoint (S, T, U, copyMesh.vertices[v], P0);

			Debug.Log (string.Format ("S: {0} T:{0} U: {0}", s, t, u)); 

			for (int i = 0; i <= L; i++){
				for (int j = 0; j <= M; j++){
					for (int k = 0; k <= N; k++){
						float pi = bernstein(L, i, s);
						float pj = bernstein(M, j, t);
						float pk = bernstein(N, k, u); 
						npt +=  pi * pj * pk * ctrlPoints[i, j, k].transform.localPosition;
					}
				}
			}
			_vrts[v] = npt;	
		}
		return _vrts; 
	}

	//convert point from world space into ffd local space 
	float ConvertPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 x, Vector3 x0){
		float pt = 0.0f; 
		pt = (Vector3.Dot(Vector3.Cross(a,b), (x-x0)))/(int)(Vector3.Dot(Vector3.Cross(a, b),c));
		return pt;
	}

	// generate FFD lattice on start at give set origin of Empty Gameobject 
	void GenGrid(){

		ctrlPoints = new GameObject[L+1, M+1, N+1];
		for (int i = 0; i <= L; i++){
			for (int j = 0; j <= M; j++){
				for (int k = 0; k <= N; k++){
					Vector3 position = Vector3.zero;
					position += P0 + (i / (float)L * S) + (j / (float)M * T) + (k / (float)N * U);
					ctrlPoints[i, j, k] = (GameObject)Instantiate(CtrlPnt,position, Quaternion.identity,transform);
				}
			}
		}
	}

	//taking (ctrl-uvw,point pos, local coordinate of target pos) based on trivariate Bézier interpolating
	float bernstein(int n, int i, float x) {

		float xn = 1.0f; 
		for (int k = 1; k <= i; k++) {
			xn *= (n - (i-k)) / (float)i; 
		}
		float result = xn * Mathf.Pow (x,(float)i) * Mathf.Pow ((float)(1.0f-x), (float)(n-i)); 
		return result; 
	}


	void MeshUpdate(Vector3[] vertices){
		render.sharedMesh = copyMesh; 
		render.sharedMesh.vertices = vertices;
		render.sharedMesh.RecalculateNormals(); 
	}

}

