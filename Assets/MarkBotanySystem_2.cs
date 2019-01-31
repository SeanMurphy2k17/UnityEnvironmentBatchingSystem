using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MarkBotanySystem_2 : MonoBehaviour
{

    public bool ShowGrassInEditor;

    public bool GenerateIndiviualBranchMeshes;
    public bool GenerateConsolidatedMesh;
    public bool GenerateBranchCollision;

    public bool LimitSizeForBetterLODManagment;

    public GameObject[] MainModels;
    public GameObject[] SecondaryModels;

    private GameObject[] GeneratedModels;

    private GameObject[] Branches;
    private int[] BranchIds;

    private GameObject[] FullGeneratedModel;

    private GameObject GeneratedFullMeshParent;
    private GameObject GeneratedBranchParent;

    private GameObject[] ColliderObj_;
    private Vector3[] ColliderPos_;
    private Vector3[] ColliderScale_;
    private Quaternion[] ColliderRot_;

    public Material Material;

    private float CurXScale;
    private float CurYScale;
    private float CurZScale;


    [Space(25)]
    [Range(1, 0.45f)]
    public float Spread;

    [Range(0.1f, 10)]
    public float Height;

    [Space]

    [Range(1, 10)]
    public int MinHeightSegments;
    [Range(1, 10)]
    public int MaxHeightSegments;
    [Space(15)]
    [Range(0, 10)]
    public float XRandomAngle;

    [Range(0, 360)]
    public float YRandomAngle;

    [Range(0, 10)]
    public float ZRandomAngle;
    [Space]
    [Range(1, 10)]
    public float BendFactor;

    public float StartTime;

    private GameObject Player;
    private MarkBotanySystem_2[] OtherBotanySystems;

    public float PlayerDistance;

    private int TreeBranchCount;

    private bool RegenNeeded;

    //!!!!!!!!THIS INCLUDES MY PERSONAL CLASS DYNAMIC ARRAY WHICH IS ULTILIZED FOR FAST RESIZING, ADDING, REMOVING AND ADJUSTING OF ARRAYS!!!!!!!!!!!
    //!!!!!!!!ARRAYS OFFER SIGNIFICANT PERFORMANCE BENIFITS OVER SYSTEM LISTS IN SPEED OF OPERATION, COMPILATION, AND RUNTIME PERFORMANCE !!!!!!!!!!!
    //!!!!!!!!DO NOT USE LISTS IN THIS TYPE OF SYSTEM, LISTS WILL SIGNIFCATNLY HURT PERFORMANCE!!!!!!!!!!

    void Start()
    {
        if (Application.isPlaying &&  /*DO NOT REMOVE TIME.TIME == 0 ---->*/ Time.time == 0 /*<----DO NOT REMOVE TIME.TIME == 0*/)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            StartTime = Time.time;
            CurXScale = transform.localScale.x;
            CurYScale = transform.localScale.y;
            CurZScale = transform.localScale.z;
            transform.localScale = new Vector3(1, 1, 1);
            GeneratedModels = new GameObject[0];

            ColliderObj_ = new GameObject[0];
            ColliderPos_ = new Vector3[0];
            ColliderScale_ = new Vector3[0];
            ColliderRot_ = new Quaternion[0];
            BranchIds = new int[0];
            Branches = new GameObject[0];
            FullGeneratedModel = new GameObject[0];

            if (Time.time != 0 && GetComponent<Rigidbody>())
            {
                GetComponent<Rigidbody>().isKinematic = true;
            }
            if (GenerateIndiviualBranchMeshes)
            {
                GeneratedBranchParent = new GameObject();
                GeneratedBranchParent.name = "Close_LOD_Branches";
                GeneratedBranchParent.transform.parent = transform;
            }
            if (GenerateConsolidatedMesh)
            {
                GeneratedFullMeshParent = new GameObject();
                GeneratedFullMeshParent.name = "Far_LOD_Mesh";
                GeneratedFullMeshParent.transform.parent = transform;
            }
            Generate();
        }
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

        float Height_ = 0;
        //GameObject TempObj = new GameObject(); // Removed due to never being used - Nathan
        for (int x = 0, i = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                xVal_ = x * 0.2f;
                zVal_ = z * 0.2f;
                P = Mathf.PerlinNoise(xVal_ + transform.position.x, zVal_ + transform.position.z);
                if (P > Spread)
                {
                    float XR = Random.Range(-XRandomAngle, XRandomAngle);
                    float YR = Random.Range(-YRandomAngle, YRandomAngle);
                    float ZR = Random.Range(-ZRandomAngle, ZRandomAngle);

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

                            int Segments_ = Random.Range(MinHeightSegments, MaxHeightSegments);
                            GameObject[] TempObj_ = new GameObject[Segments_];
                            MeshFilter[] BranchObjects_ = new MeshFilter[0];
                            for (int ii = 0; ii < Segments_; ii++)
                            {
                                float RT_ = ((100 / Segments_) * ii) * 0.01f;
                                RC = Quaternion.Slerp(Rot_T, Rot_TMax, RT_);
                                if (ii > 0)
                                {
                                    TempPos_ = TempObj_[ii - 1].transform.position + (TempObj_[ii - 1].transform.up * TempObj_[ii - 1].transform.localScale.y);
                                }
                                GameObject Model = Instantiate(MainModels[Random.Range(0, MainModels.Length)], TempPos_, RC);
                                if (Random.Range(0, 5) < 2 && SecondaryModels.Length != 0)
                                {
                                    Quaternion SecondaryRotation = Quaternion.Euler(RC.x * XR, RC.y * YR, RC.z * ZR);

                                    GameObject SecondaryModel = Instantiate(SecondaryModels[Random.Range(0, SecondaryModels.Length)], TempPos_, SecondaryRotation);
                                    SecondaryModel.transform.Rotate(XR, 0, 0);
                                    SecondaryModel.transform.parent = transform;
                                    GeneratedModels = DynamicArray.AddGameObjectToArray(GeneratedModels, SecondaryModel);
                                    MeshFilter[] Temp_MS = SecondaryModel.GetComponentsInChildren<MeshFilter>();
                                    BranchObjects_ = DynamicArray.AddMeshFilterArrayToArray(BranchObjects_, Temp_MS);
                                }
                                TempObj_[ii] = Model;
                                Model.transform.localScale = new Vector3(1, Height_ / Segments_, 1);
                                Model.transform.parent = transform;
                                GeneratedModels = DynamicArray.AddGameObjectToArray(GeneratedModels, Model);
                                ColliderPos_ = DynamicArray.AddVector3ToArray(ColliderPos_, TempPos_);
                                ColliderScale_ = DynamicArray.AddVector3ToArray(ColliderScale_, Model.transform.localScale);
                                ColliderRot_ = DynamicArray.AddQuaternionToArray(ColliderRot_, RC);
                                BranchIds = DynamicArray.AddIntToArray(BranchIds, i);

                                MeshFilter[] Temp_MM = Model.GetComponentsInChildren<MeshFilter>();
                                BranchObjects_ = DynamicArray.AddMeshFilterArrayToArray(BranchObjects_, Temp_MM);
                            }
                            if (GenerateIndiviualBranchMeshes)
                            {
                                GenerateBranch(BranchObjects_, i++);
                            }
                        }
                    }
                }
            }
        }
        if (GenerateConsolidatedMesh)
        {
            GenerateMesh();
        }
        DestroyGeneratedModels_();
        OtherBotanySystems = FindObjectsOfType<MarkBotanySystem_2>();
    }
    void GenerateBranch(MeshFilter[] meshFilters, int Id)
    {
        MeshFilter[] Temp_ = new MeshFilter[0];
        int VertCount = 0;

        string Name_ = "Branch_LOD_Close_";
        for (int i = 0; i < meshFilters.Length; i++)
        {
            VertCount += meshFilters[i].GetComponent<MeshFilter>().sharedMesh.vertices.Length;
            if (VertCount < 65000)
            {
                Temp_ = DynamicArray.AddMeshFilterToArray(Temp_, meshFilters[i]);
            }
            else
            {
                CombineMeshes(Temp_, Id, Name_, 1);
                VertCount = 0;
                Temp_ = new MeshFilter[i];
                Temp_ = DynamicArray.AddMeshFilterToArray(Temp_, meshFilters[i]);
            }
        }
        if (Temp_.Length > 0)
        {
            CombineMeshes(Temp_, Id, Name_, 1);
        }
    }
    void GenerateMesh()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        MeshFilter[] Temp_ = new MeshFilter[0];

        int VertCount = 0;
        int CominationCount = 0;
        string Name_ = "LOD_Far_Mesh";

        for (int i = 0; i < meshFilters.Length; i++)
        {
            VertCount += meshFilters[i].GetComponent<MeshFilter>().sharedMesh.vertices.Length;
            if (VertCount < 65000)
            {
                Temp_ = DynamicArray.AddMeshFilterToArray(Temp_, meshFilters[i]);
            }
            else
            {
                CombineMeshes(Temp_, CominationCount++, Name_, 0);
                VertCount = 0;
                Temp_ = new MeshFilter[0];
                Temp_ = DynamicArray.AddMeshFilterToArray(Temp_, meshFilters[i]);
            }
        }
        if (Temp_.Length > 0)
        {
            CombineMeshes(Temp_, CominationCount++, Name_, 0);
        }
    }
    void CombineMeshes(MeshFilter[] meshFilters, int Count, string Name_, int Type)
    {
        GameObject NewMesh = new GameObject();

        NewMesh.name = name + "_" + Name_ + "_" + "_Mesh_" + Count;
        NewMesh.AddComponent<MeshFilter>();
        NewMesh.AddComponent<MeshRenderer>();
        NewMesh.GetComponent<MeshRenderer>().material = Material;

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        NewMesh.GetComponent<MeshFilter>().mesh = new Mesh();
        NewMesh.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        if (Type == 0)
        {
            FullGeneratedModel = DynamicArray.AddGameObjectToArray(FullGeneratedModel, NewMesh);
            NewMesh.transform.parent = GeneratedFullMeshParent.transform;
        }
        if (Type == 1)
        {
            Branches = DynamicArray.AddGameObjectToArray(Branches, NewMesh);
            NewMesh.transform.parent = GeneratedBranchParent.transform;
        }
    }

    void ReGenerateFullModelMesh()
    {
        for (int i = 0; i < FullGeneratedModel.Length; i++)
        {
            Destroy(FullGeneratedModel[i]);
        }
        GenerateMesh();
    }
    void DestroyGeneratedModels_()
    {
        for (int i = 0; i < GeneratedModels.Length; i++)
        {
            Destroy(GeneratedModels[i]);
        }
        if (GenerateBranchCollision)
        {
            AddColliders();
        }
    }
    void AddColliders()
    {
        for (int i = 0; i < ColliderPos_.Length; i++)
        {
            GameObject Col = new GameObject();
            Col.name = "Col_" + i;
            Col.AddComponent<BoxCollider>();
            Col.transform.position = ColliderPos_[i];
            Col.transform.rotation = ColliderRot_[i];
            Col.transform.localScale = ColliderScale_[i];
            //Col.GetComponent<BoxCollider>().isTrigger = true;
            Col.GetComponent<BoxCollider>().size = new Vector3(0.2f, 1, 0.2f);
            Col.GetComponent<BoxCollider>().center = new Vector3(0, 0.5f, 0);
            ColliderObj_ = DynamicArray.AddGameObjectToArray(ColliderObj_, Col);
            Col.transform.parent = Branches[BranchIds[i]].transform;
        }
        //transform.position += new Vector3(0, 0.25f, 0);
    }
    // Update is called once per frame
    void Update()
    {
        if (LimitSizeForBetterLODManagment)
        {
            //////////////Force max scale
            //X
            if (transform.localScale.x > 5)
            {
                transform.localScale = new Vector3(5, CurYScale, CurZScale);
            }
            if (transform.localScale.x < 0.1f)
            {
                transform.localScale = new Vector3(0.1f, CurYScale, CurZScale);
            }
            //Y
            if (transform.localScale.y > 5)
            {
                transform.localScale = new Vector3(CurXScale, 5, CurZScale);
            }
            if (transform.localScale.y < 0.1f)
            {
                transform.localScale = new Vector3(CurXScale, 0.1f, CurZScale);
            }
            //Z
            if (transform.localScale.z > 5)
            {
                transform.localScale = new Vector3(CurXScale, CurYScale, 5);
            }
            if (transform.localScale.z < 0.1f)
            {
                transform.localScale = new Vector3(CurXScale, CurYScale, 0.1f);
            }
        }
        if (!Application.isPlaying)
        {
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
        else
        {
            if (Player)
            {
                PlayerDistance = Vector3.Distance(transform.position + new Vector3(CurXScale / 2, 0, CurZScale / 2), Player.transform.position);
                if (GenerateIndiviualBranchMeshes)
                {
                    if (PlayerDistance < 7)
                    {
                        int CloserCount = 0;
                        for (int i = 0; i < OtherBotanySystems.Length; i++)
                        {
                            if (OtherBotanySystems[i].PlayerDistance < PlayerDistance)
                            {
                                CloserCount++;
                            }
                        }
                        if (CloserCount < 3)
                        {
                            GeneratedBranchParent.SetActive(true);
                            GeneratedFullMeshParent.SetActive(false);
                        }
                        else
                        {
                            GeneratedBranchParent.SetActive(false);
                            GeneratedFullMeshParent.SetActive(true);
                        }
                    }
                    /////////REGEN ONLY NEEDED IF SLICING PLUGIN INCLUDUED, OTHERWISE DISREGARD/////////
                    if (PlayerDistance > 10 && RegenNeeded)
                    {
                        ReGenerateFullModelMesh();
                    }
                }
            }
        }
    }
}
