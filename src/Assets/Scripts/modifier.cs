using UnityEngine;
using System.Collections;

public class modifier : MonoBehaviour
{
    //get mesh data 
    SkinnedMeshRenderer target;
    //copied mesh ref
    Mesh clone;
    [Header("ctrl point prefab")]
    //ctrl point prefab
    public GameObject ctrlPoint;
    //lattice dimension 
    [Header("FFD dimension")]
    public int L, M, N;
    //Localized grid
    Vector3 S = Vector3.zero, T = Vector3.zero, U = Vector3.zero;
    //localized point position values 
    float s, t, u;
    //store all ctrl points postion
    GameObject[,,] ctrlPoints;
    //lattice origin 
    Vector3 P0;

    //store vertices information 
    Vector3[] vrts;


    private void Start()
    {
        target = GameObject.FindObjectOfType<SkinnedMeshRenderer>();//get target model in scene mesh info 
        clone = (Mesh)Instantiate(target.sharedMesh);//make copy of mesh taken
        vrts = new Vector3[clone.vertexCount];//set array size of vertice on mesh 
        S = new Vector3(0.5f, 0.0f, 0.0f);
        T = new Vector3(0.0f, 0.5f, 0.0f);
        U = new Vector3(0.0f, 0.0f, 0.5f);
        ctrlPoints = new GameObject[L + 1, M + 1, N + 1];
        BuildLattice();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 500.0f))
            {
                P0 = ctrlPoints[0, 0, 0].transform.localPosition;
                MeshUpdate(Parameterize());
            }
        }
    }


    Vector3[] Parameterize()
    {//place vertices of object into lattice local grid for calculation 

        for (int v = 0; v < clone.vertexCount; v++)
        {
            Vector3 npt = Vector3.zero;
            s = EmbedPoint(T, U, S, clone.vertices[v], P0);
            t = EmbedPoint(S, U, T, clone.vertices[v], P0);
            u = EmbedPoint(S, T, U, clone.vertices[v], P0);

            Vector3 p = P0 + s * S + t * T + u * U;
            Vector3 stu = new Vector3(s, t, u);
            Debug.Log(string.Format("vrt {0} convert to {1} back to {2}", clone.vertices[v], stu, p));

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
        for (int i = 0; i <= L; i++)
        {
            for (int j = 0; j <= M; j++)
            {
                for (int k = 0; k <= N; k++)
                {
                    Vector3 position = (i / L * S) + (j / M * T) + (k / N * U);
                    ctrlPoints[i, j, k] = (GameObject)Instantiate(ctrlPoint, position, Quaternion.identity);
                }
            }
        }//end-of-nested loop
    }

    float EmbedPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 x, Vector3 x0)
    {//embedding points into lattice

        return Vector3.Dot(Vector3.Cross(a, b), ((x - x0) / Vector3.Dot(Vector3.Cross(a, b), c)));
    }

    float Bernstein(int n, int i, float x)
    {//calculate berstein polynomial

        float xn = 1.0f;
        for (int k = 1; k <= i; k++)
        {
            xn *= (n - (i - k)) / i;
        }
        return xn * Mathf.Pow(x, (float)i) * Mathf.Pow((float)(1.0f - x), (float)(n - i));
    }

    void MeshUpdate(Vector3[] vertices)
    {//take new mesh data and apply copied points to render 
        target.sharedMesh = clone;
        target.sharedMesh.vertices = vertices;
        target.sharedMesh.RecalculateBounds();
        target.sharedMesh.RecalculateNormals();
    }







}