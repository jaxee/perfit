using UnityEngine;
using System;
using System.Collections;

public class obj : MonoBehaviour {


[Header("Target model")]
public GameObject targetmodel;

//get mesh data
SkinnedMeshRenderer target;

SaveManager sm;

//copied mesh ref
Mesh clone;

[Header("Ctrl point prefab")]
//ctrl point prefab
public GameObject ctrlPoint;

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

//store all ctrl points postion
GameObject[,,] ctrlPoints;

//lattice origin
Vector3 P0;

//store vertices information
Vector3[] vrts;

private System.Diagnostics.Stopwatch tStart;

//update mesh
[Header("Mesh update")]
public bool updateMesh= false;

//array to target ctrl group
string [] leftHip   = new string[] { "2,2,0","2,2,1","2,2,2"};
string [] rightHip  = new string[] { "4,2,0", "4,2,1", "4,2,2" };
string [] height    = new string[] { "0,3,0","0,3,1","0,3,2","0,4,0","0,4,1","0,4,2", "1,3,0", "1,3,1", "1,3,2", "1,4,0", "1,4,1", "1,4,2", "2,3,0", "2,3,1", "2,3,2", "2,4,0", "2,4,1", "2,4,2", "3,3,0", "3,3,1", "3,3,2", "3,4,0", "3,4,1", "3,4,2", "4,3,0", "4,3,1", "4,3,2", "4,4,0", "4,4,1", "4,4,2", "5,3,0", "5,3,1", "5,3,2", "5,4,0", "5,4,1", "5,4,2", "6,3,0", "6,3,1", "6,3,2", "6,4,0", "6,4,1", "6,4,2" };
string butt         = "3,2,0";
string chest        = "3,3,2";
string stomach      = "3,2,2";

private Vector3 tmpTU = Vector3.zero;
private Vector3 tmpSU = Vector3.zero;
private Vector3 tmpST = Vector3.zero;

private float tmpTUS = 0;
private float tmpSUT = 0;
private float tmpSTU = 0;

private BodyscanSave bodyData;
private ModelSave modelData;
private Sizing sizing; 


    private void Start()
{
        sizing = new Sizing(); 
		bodyData = FindObjectOfType<BodyscanSave> ();
        sm = FindObjectOfType<SaveManager>();
        target = targetmodel.GetComponent<SkinnedMeshRenderer>();//get target model in scene mesh info
        clone = (Mesh)Instantiate(target.sharedMesh);//make copy of mesh taken
        vrts = new Vector3[clone.vertexCount];//set array size of vertice on mesh
        ctrlPoints = new GameObject[L + 1, M + 1, N + 1];//set empty array to lattice size input
        SetOrigin();
        BuildLattice();
        GameObject.FindObjectOfType<ParticleSystem>().Play();
        StartCoroutine(applyFX());
    }

private void Update()
{
        if (Input.GetKeyDown("u"))
        {
                gameObject.SetActive(true);
                updateMesh = true;
                if (updateMesh)
                {//ensure timed update of mesh as deformation happen

                        Deform ();
                        updateMesh = false;
                        gameObject.SetActive(false);
                }

        }

}

void SetOrigin()
{    //set lattice local coordinate system

        //get max and min of 3d model vertices
        Vector3 min = new Vector3 (Mathf.Infinity,Mathf.Infinity,Mathf.Infinity);
        Vector3 max = new Vector3 (-Mathf.Infinity,-Mathf.Infinity,-Mathf.Infinity);
        for (int i = 0; i < clone.vertices.Length; i++) {//loop through all points and find max vertice and min vertice
                max = Vector3.Max(clone.vertices[i],max);
                min = Vector3.Min(clone.vertices[i],min);
        }

        S  = new Vector3(max.x-min.x, 0.0f, 0.0f);
        T  = new Vector3(0.0f, max.y-min.y, 0.0f);
        U  = new Vector3(0.0f, 0.0f, max.z-min.z);

        tmpTU = Vector3.Cross(T, U);
        tmpSU = Vector3.Cross(S, U);
        tmpST = Vector3.Cross(S, T);

        tmpTUS = Vector3.Dot(tmpTU, S);
        tmpSUT = Vector3.Dot(tmpSU, T);
        tmpSTU = Vector3.Dot(tmpST, U);
        P0 = min;
}
void Deform()
{    //place vertices of object into lattice local grid for calculation

        System.Diagnostics.Stopwatch tStart = System.Diagnostics.Stopwatch.StartNew();
        tStart.Start ();
        Vector3 npt;
        Vector3[] data = clone.vertices;

        for (int v = 0; v < clone.vertexCount; v++) {
                //Vector3 p = P0 + s * S + t * T + u * U;
                //Vector3 stu = new Vector3(s, t, u);
                //Debug.Log(string.format("vrt {0} convert to {1} back to {2}", clone.vertices[v], stu, p));
                npt = Vector3.zero;
                Vector3 x = data [v] - P0;
                float s = Vector3.Dot (tmpTU, x/ tmpTUS);
                float t = Vector3.Dot (tmpSU, x/ tmpSUT);
                float u = Vector3.Dot (tmpST, x/ tmpSTU);

                for (int i = 0; i <= L; i++) {
                        float pi = Bernstein(L, i, s);
                        for (int j = 0; j <= M; j++) {
                                float pj = Bernstein(M, j, t);
                                for (int k = 0; k <= N; k++) {
                                        float pk = Bernstein (N, k, u);
                                        npt +=  pi * pj * pk * ctrlPoints[i,j,k].transform.localPosition;
                                }
                        }
                }
                vrts[v] = npt;
        }

        MeshUpdate (vrts);
        tStart.Stop ();
        TimeSpan delta = tStart.Elapsed;
        UnityEngine.Debug.Log("Delta: "+ delta);
        tStart.Reset();
        gameObject.SetActive(false);
}


void BuildLattice()
{
        for (int i = 0; i <= L; i++)
        {
                for (int j=0; j <= M; j++)
                {
                        for (int k = 0; k <= N; k++)
                        {
                                Vector3 position = P0 + (i / (float)L * S) + (j / (float)M * T) + (k / (float)N * U);
                                ctrlPoints[i, j, k] = (GameObject)Instantiate(ctrlPoint, position, Quaternion.identity, transform);
                                ctrlPoints[i, j, k].name = string.Format("{0},{1},{2}", i, j, k);
								ctrlPoints[i, j, k].layer = 8;
                        }
                }
        }//end-of-nested loop
        loadProfile();
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
{    //calculate berstein polynomial
        float binomial = factorials(n) / (factorials(n - i) * factorials(i));
        return binomial * Mathf.Pow(x,(float)i) * Mathf.Pow((float)(1.0f - x), (float)(n - i));
}

void MeshUpdate(Vector3[] vertices)
{    //take new mesh data and apply copied points to render
        updateMesh = false;//reset bool
        target.sharedMesh = clone;
        target.sharedMesh.vertices = vertices;
        target.sharedMesh.RecalculateBounds();
        target.sharedMesh.RecalculateNormals();
        modelData = FindObjectOfType<ModelSave>();
        modelData.mesh = target.sharedMesh;
}

void adjust(float measurement,string section)
{    //take values and move respective ctrl points

        if (section == "hips") {
                Vector3 adjA = new Vector3(measurement * 0.10f, 0,0);
                foreach (string hip in leftHip) {
                        GameObject tmp = GameObject.Find(hip);
                        tmp.transform.position -= adjA;

                }
                foreach (string hip in rightHip) {
                        GameObject tmp = GameObject.Find(hip);
                        tmp.transform.position += adjA;

                }
                //hipCol.radius += 2 * 0.10f;
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

                //bustACol.radius += 2 * 0.10f;
                //bustBCol.radius += 2 * 0.10f;
        }
        if (section == "height")
        {
            for (int i = 0; i< height.Length; i++) {
                Vector3 adjustment = new Vector3(0, measurement * 0.01f,0);
                GameObject tmp = GameObject.Find(height[i]);
                tmp.transform.position += adjustment;

            }

        }
}

    void loadProfile(){
        adjust(sizing.ConvertInput(1,bodyData.Height), "height");
        adjust(sizing.ConvertInput(2,bodyData.Bust),"bust");
		adjust(sizing.ConvertInput(3, bodyData.Waist), "hips");
    }

    private IEnumerator applyFFD() {
        yield return new WaitForFixedUpdate();
        Deform();
        updateMesh = false;
        gameObject.SetActive(false);
    }

    private IEnumerator applyFX() {
        StartCoroutine(applyFFD());
        yield return new WaitForSeconds(2);
        GameObject.FindObjectOfType<ParticleSystem>().Stop();
    }




}//-end-of-script
