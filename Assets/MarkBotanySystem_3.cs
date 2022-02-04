using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MarkBotanySystem_3 : MonoBehaviour
{

    public bool ShowGrassInEditor;

    public GameObject[] MainModels;

    public GameObject GeneratedFullMeshParent;

    public Material Material;

    private float CurXScale;
    private float CurYScale;
    private float CurZScale;


    [Space(25)]
    [Range(1, 0.45f)]
    public float Spread;

    [Range(0.1f, 40)]
    public float Height;

    [Space]

    [Space]
    [Range(1, 10)]
    public float BendFactor = 1;

    public float StartTime;


    private int TreeBranchCount;

    private bool RegenNeeded;

    [SerializeField]
    private bool BakeData;

    private List<MeshData_> meshData = new List<MeshData_>();
    public struct MeshData_
    {
        public Vector3[] verts;
    }
    //!!!!!!!!THIS INCLUDES MY PERSONAL CLASS DYNAMIC ARRAY WHICH IS ULTILIZED FOR FAST RESIZING, ADDING, REMOVING AND ADJUSTING OF ARRAYS!!!!!!!!!!!
    //!!!!!!!!ARRAYS OFFER SIGNIFICANT PERFORMANCE BENIFITS OVER SYSTEM LISTS IN SPEED OF OPERATION, COMPILATION, AND RUNTIME PERFORMANCE !!!!!!!!!!!
    //!!!!!!!!DO NOT USE LISTS IN THIS TYPE OF SYSTEM, LISTS WILL SIGNIFCATNLY HURT PERFORMANCE!!!!!!!!!!

    void Start()
    {
        if (Application.isPlaying &&  /*DO NOT REMOVE TIME.TIME == 0 ---->*/ Time.time == 0 /*<----DO NOT REMOVE TIME.TIME == 0*/ && !GeneratedFullMeshParent)
        {
            InitilizeDataSets();
        }
    }
    void InitilizeDataSets()
    {
        StartTime = Time.time;
        CurXScale = transform.localScale.x;
        CurYScale = transform.localScale.y;
        CurZScale = transform.localScale.z;
        //transform.localScale = new Vector3(1, 1, 1);

        if (Time.time != 0 && GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
        GeneratedFullMeshParent = new GameObject();
        GeneratedFullMeshParent.name = "Far_LOD_Mesh";
        GeneratedFullMeshParent.transform.parent = transform;
        Generate();
    }
    void Generate()
    {

        int xCount = Mathf.RoundToInt(CurXScale * 5);
        int zCount = Mathf.RoundToInt(CurZScale * 5);

        float xVal_ = 0;
        float zVal_ = 0;

        RaycastHit Hit_;
        Vector3 TempPos_ = transform.position;
        float P = 0;
        //GameObject exampleMesh = 
        float Height_ = 0;
        //GameObject TempObj = new GameObject(); // Removed due to never being used - Nathan
        for (int x = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                xVal_ = x * 0.2f;
                zVal_ = z * 0.2f;
                P = Mathf.PerlinNoise(xVal_ + transform.position.x, zVal_ + transform.position.z);
                if (P > Spread)
                {
                    float XR = Random.Range(-10, 10);
                    float YR = Random.Range(-360, 360);
                    float ZR = Random.Range(-10, 10);

                    Quaternion Rot_T = Quaternion.Euler(XR, YR, ZR);
                    Quaternion Rot_TMax = Quaternion.Euler(XR * BendFactor, YR * BendFactor, ZR * BendFactor);
                    Quaternion RC = Quaternion.identity;

                    TempPos_ = transform.position + new Vector3(xVal_, CurYScale / 2, zVal_);
                    Height_ = P + Height;
                    Height_ = Mathf.Lerp(0.24f, Height, Height_);

                    if (Physics.Linecast(TempPos_, TempPos_ + new Vector3(0, -CurYScale / 2, 0), out Hit_))
                    {
                        if (Hit_.collider)
                        {
                            TempPos_ = Hit_.point;
                            TempPos_ += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                            int ran = Random.Range(0, MainModels.Length);
                            MeshFilter mf = MainModels[ran].GetComponent<MeshFilter>();
                            int length = mf.sharedMesh.triangles.Length;
                            Vector3[] newVerts = new Vector3[length];
                            Matrix4x4 m = Matrix4x4.TRS(TempPos_, Rot_TMax, new Vector3(Random.Range(1f, 3f), Height_, Random.Range(1f, 3f)));
                            for (int v = 0; v < length; v++)
                            {
                                Vector3 pos = mf.sharedMesh.vertices[mf.sharedMesh.triangles[v]];
                                newVerts[v] = m.MultiplyPoint3x4(pos);
                            }
                            MeshData_ mD_ = new MeshData_();
                            mD_.verts = newVerts;
                            meshData.Add(mD_);
                        }
                    }
                }
            }
        }
        if (Application.isPlaying)
        {
            StartCoroutine(GenerateMesh());
        }
        else
        {
            GenerateMeshRuntime();
        }
    }
    void GenerateMeshRuntime()
    {
        Vector3[] CollectiveVerts = new Vector3[0];
        int VertCount = 0;
        int CominationCount = 0;
        for (int i = 0; i < meshData.Count; i++)
        {
            VertCount += meshData[i].verts.Length;
            if (VertCount < 65000)
            {
                CollectiveVerts = DynamicArray.AddVector3ArrayToArray(CollectiveVerts, meshData[i].verts);
            }
            else
            {
                CombineMensh_Eco(CollectiveVerts, CominationCount);
                CollectiveVerts = new Vector3[0];
                CominationCount++;
                VertCount = 0;
            }
        }
        if (VertCount > 0)
        {
            CombineMensh_Eco(CollectiveVerts, CominationCount);
        }
    }
    IEnumerator GenerateMesh()
    {
        Vector3[] CollectiveVerts = new Vector3[0];
        int VertCount = 0;
        int CominationCount = 0;
        int counterHold = 0;
        for (int i = 0; i < meshData.Count; i++)
        {
            VertCount += meshData[i].verts.Length;
            if (VertCount < 65000)
            {
                CollectiveVerts = DynamicArray.AddVector3ArrayToArray(CollectiveVerts, meshData[i].verts);
            }
            else
            {
                CombineMensh_Eco(CollectiveVerts, CominationCount);
                CollectiveVerts = new Vector3[0];
                CominationCount++;
                VertCount = 0;
            }
            counterHold++;
            if (counterHold > 1000)
            {
                counterHold = 0;
                yield return new WaitForSeconds(0.5f);
            }
        }
        if (VertCount > 0)
        {
            CombineMensh_Eco(CollectiveVerts, CominationCount);
        }
    }
    void CombineMensh_Eco(Vector3[] verts, int count)
    {
        GameObject NewMesh = new GameObject();
        NewMesh.name = "eco" + "_" + "_Mesh_" + count;
        NewMesh.AddComponent<MeshFilter>();
        MeshFilter mFilter = NewMesh.GetComponent<MeshFilter>();
        int[] tris = new int[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            tris[i] = i;
        }
        mFilter.sharedMesh = new Mesh();
        mFilter.sharedMesh.name = "Eco" + count;
        mFilter.sharedMesh.vertices = verts;
        mFilter.sharedMesh.triangles = tris;
        mFilter.sharedMesh.RecalculateNormals();
        mFilter.sharedMesh.RecalculateBounds();
        NewMesh.transform.parent = GeneratedFullMeshParent.transform;
        NewMesh.AddComponent<MeshRenderer>();
        NewMesh.GetComponent<MeshRenderer>().material = Material;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
            if (BakeData)
            {
                BakeData = false;
                if(GeneratedFullMeshParent != null) 
                {
                    DestroyImmediate(GeneratedFullMeshParent);
                }
                if (!GeneratedFullMeshParent)
                {
                    InitilizeDataSets();
                }
            }
            if (ShowGrassInEditor)
            {
                int xCount = Mathf.RoundToInt(CurXScale * 5);
                int zCount = Mathf.RoundToInt(CurZScale * 5);

                float xVal_ = 0;
                float zVal_ = 0;

                RaycastHit Hit_;
                Vector3 TempPos_ = transform.position;
                float P = 0;

                float Height_ = 0;
                for (int x = 0, i = 0; x < xCount; x++)
                {
                    for (int z = 0; z < zCount; z++, i++)
                    {
                        xVal_ = x * 0.2f;
                        zVal_ = z * 0.2f;
                        P = Mathf.PerlinNoise(xVal_ + transform.position.x, zVal_ + transform.position.z);
                        if (P > Spread)
                        {
                            TempPos_ = transform.position + new Vector3(xVal_, CurYScale / 2, zVal_);
                            //Height_ = (100 / (Vector3.Distance(CenterPoint, new Vector3(xVal_, 0, zVal_) * P))) * 0.01f;
                            Height_ = Mathf.Lerp(0.24f, P + Height, P);
                            if (Physics.Linecast(TempPos_, TempPos_ + new Vector3(0, -CurYScale / 2, 0), out Hit_))
                            {
                                if (Hit_.collider)
                                {
                                    Debug.DrawLine(Hit_.point, Hit_.point + new Vector3(0, (Height_), 0), Color.green);
                                    TreeBranchCount++;
                                }
                            }
                        }
                    }
                }
            }
            CurXScale = transform.localScale.x;
            CurYScale = transform.localScale.y;
            CurZScale = transform.localScale.z;

            //Lock rotation to prevent extra processing
            transform.rotation = Quaternion.identity;


            //Draw bottom bounding area for plants
            Debug.DrawLine(transform.position, transform.position + new Vector3(0, 0, CurZScale));
            Debug.DrawLine(transform.position + new Vector3(0, 0, CurZScale), transform.position + new Vector3(CurXScale, 0, CurZScale));
            Debug.DrawLine(transform.position + new Vector3(CurXScale, 0, CurZScale), transform.position + new Vector3(CurXScale, 0, 0));
            Debug.DrawLine(transform.position + new Vector3(CurXScale, 0, 0), transform.position);
            //Draw top bounding area for plants
            Debug.DrawLine(transform.position + new Vector3(0, CurYScale, 0), transform.position + new Vector3(0, CurYScale, CurZScale));
            Debug.DrawLine(transform.position + new Vector3(0, CurYScale, CurZScale), transform.position + new Vector3(CurXScale, CurYScale, CurZScale));
            Debug.DrawLine(transform.position + new Vector3(CurXScale, CurYScale, CurZScale), transform.position + new Vector3(CurXScale, CurYScale, 0));
            Debug.DrawLine(transform.position + new Vector3(CurXScale, CurYScale, 0), transform.position + new Vector3(0, CurYScale, 0));
        }
    }
}
