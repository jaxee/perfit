﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj : MonoBehaviour {

    //get mesh data 
    SkinnedMeshRenderer target;
    
    //copied mesh ref
    Mesh clone;

    [Header("ctrl point prefab")]
    //ctrl point prefab
    public GameObject ctrlPoint;
    public string ptName; 
    //lattice dimension 
    [Header("FFD dimension")]
    public int L;
    public int M;
    public int N;

    //Localized grid
    [Header("FFD local coordinate size")]
    public Vector3 S;
    public Vector3 T;
    public Vector3 U;

    //localized point position values 
    float s, t, u;
    
    //store all ctrl points postion
    GameObject[,,] ctrlPoints;
    
    //lattice origin 
    Vector3 P0;

    //store vertices information 
    Vector3[] vrts;


    //update mesh
    [Header("Mesh update")]
    public bool updateMesh= false; 

    //array to target ctrl group 
    string [] leftHip   = new string[] { "2,2,0","2,2,1","2,2,2"};
    string [] rightHip  = new string[] { "4,2,0", "4,2,1", "4,2,2" };
    string butt         = "3,2,0";
    string chest        = "3,3,2";
    string stomach      = "3,2,2";


    private void Start()
    {
        target = GameObject.FindObjectOfType<SkinnedMeshRenderer>();//get target model in scene mesh info 
        clone = (Mesh)Instantiate(target.sharedMesh);//make copy of mesh taken
        vrts = new Vector3[clone.vertexCount];//set array size of vertice on mesh 
        ctrlPoints = new GameObject[L+1,M+1,N+1];//set empty array to lattice size input
        SetOrigin();
        BuildLattice();//build lattice 
    }
    private void FixedUpdate()
    {
        if (updateMesh)
        {//ensure timed update of mesh as deformation happen
            MeshUpdate(Deform());
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown("u"))
        {
            updateMesh = true;
            
        }
        if (Input.GetKeyDown("a"))
        {
            adjust(10, "butt");
            adjust(5, "chest");
			adjust(40, "stomach");
            adjust(32, "lhips");
            adjust(32, "rhips");
        }
    }

   void SetOrigin()
    {//set lattice local coordinate system

		//get max and min of 3d model vertices 
		Vector3 min = new Vector3 (Mathf.Infinity,Mathf.Infinity,Mathf.Infinity); 
		Vector3 max = new Vector3 (-Mathf.Infinity,-Mathf.Infinity,-Mathf.Infinity);

		foreach(Vector3 v in clone.vertices){//loop through all points and find max vertice and min vertice
			max = Vector3.Max(v,max);
			min = Vector3.Min(v,min);
		}

		S  = new Vector3(max.x-min.x, 0.0f, 0.0f);
		T  = new Vector3(0.0f, max.y-min.y, 0.0f);
		U  = new Vector3(0.0f, 0.0f, max.z-min.z);

		P0 = min;
    }


    Vector3[] Deform()
    {//place vertices of object into lattice local grid for calculation 

        for (int v = 0; v < clone.vertexCount; v++)
        {
            Vector3 npt = Vector3.zero;
            s = EmbedPoint(T, U, S, clone.vertices[v], P0);
            t = EmbedPoint(S, U, T, clone.vertices[v], P0);
            u = EmbedPoint(S, T, U, clone.vertices[v], P0);

            //Vector3 p = P0 + s * S + t * T + u * U;
            //Vector3 stu = new Vector3(s, t, u);
            //Debug.Log(string.Format("vrt {0} convert to {1} back to {2}", clone.vertices[v], stu, p));

            for (int i = 0; i <= L; i++)
            {
                for (int j = 0; j <= M; j++)
                {
                    for (int k = 0; k <= N; k++)
                    {
                        float pi = Bernstein(L, i, s);
                        float pj = Bernstein(M, j, t);
                        float pk = Bernstein(N, k, u); 
                        npt += pi * pj * pk * ctrlPoints[i, j, k].transform.localPosition;
                    }
                }
            }
            vrts[v] = npt;

        }//end-of-loop

        return vrts;
    }

    void BuildLattice()
    {//build control point lattice 
        for (int i = 0; i <= L;i++)
        {
            for (int j=0; j <= M;j++)
            {
                for(int k=0; k <= N; k++)
                {
					Vector3 position = P0+(i /(float)L * S) + (j/ (float)M * T) + (k / (float)N * U);
                    ctrlPoints[i, j, k] = (GameObject)Instantiate(ctrlPoint, position, Quaternion.identity,transform);
                    ctrlPoints[i, j, k].name = string.Format("{0},{1},{2}",i,j,k);
                    ctrlPoints[i, j, k].layer = 8;


                }
            }
        }//end-of-nested loop
    }

    float EmbedPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 x, Vector3 x0)
    {//embedding points into lattice
        return Vector3.Dot(Vector3.Cross(a, b), (x-x0))/Vector3.Dot(Vector3.Cross(a, b), c);
    }

    float factorials(int a)
    {
        float f = 1.0f;
        for (int i=a; i>0; i--)
        {
            f *= i;
        }
        return f; 
    }

    float Bernstein(int n, int i, float x)
    {//calculate berstein polynomial
        float binomial = factorials(n) / (factorials(n - i) * factorials(i));
        return binomial * Mathf.Pow(x,(float)i) * Mathf.Pow((float)(1.0f - x), (float)(n - i));
    }

    void MeshUpdate(Vector3[] vertices)
    {//take new mesh data and apply copied points to render 
        updateMesh = false;//reset bool 
        target.sharedMesh = clone;
        target.sharedMesh.vertices = vertices;
        target.sharedMesh.RecalculateBounds();
        target.sharedMesh.RecalculateNormals();
    }

    void adjust(float measurement,string section)
    {//take values and move respective ctrl points 

		CapsuleCollider hipCol     = GameObject.Find ("Character_Hips").GetComponent<CapsuleCollider> ();
		CapsuleCollider rthighCol  = GameObject.Find ("Character_RightUpLeg").GetComponent<CapsuleCollider> ();
		CapsuleCollider lthighCol  = GameObject.Find ("Character_LeftUpLeg").GetComponent<CapsuleCollider> ();

        if (section == "lhips") {
            Vector3 adjustment = new Vector3(measurement * 0.10f, 0,0);
            foreach (string hip in leftHip) {
                GameObject tmp = GameObject.Find(hip);
                tmp.transform.position -= adjustment;
				hipCol.radius += 2 * 0.10f; 
            }
        }
        if (section == "rhips")
        {
            Vector3 adjustment = new Vector3(measurement * 0.10f, 0, 0);
            foreach (string hip in rightHip)
            {
                GameObject tmp = GameObject.Find(hip);
                tmp.transform.position += adjustment;
            }
        }
        if (section == "butt") {
            Vector3 adjustment = new Vector3(0, 0, measurement * 0.10f);
            GameObject tmp = GameObject.Find(butt);
            tmp.transform.position -= adjustment;
        }
        if (section == "stomach")
        {
            Vector3 adjustment = new Vector3(0, 0, measurement * 0.10f);
            GameObject tmp = GameObject.Find(stomach);
            tmp.transform.position += adjustment;
        }
        if (section == "chest")
        {
            Vector3 adjustment = new Vector3(0, 0, measurement * 0.10f);
            GameObject tmp = GameObject.Find(chest);
            tmp.transform.position += adjustment;
        }

    }

	public void updateSize(){
		
	}
		
}//-end-of-script 
