using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RealMorph : MonoBehaviour
{
    public MeshFilter mesh2;
    public MeshRenderer meshRenderer2;
    public SkinnedMeshRenderer skinnedMeshRenderer2;
    public Mesh bakedMesh2;
    public bool meshFilter2Bool = false;
    public MyMeshStructure myMeshStructure;
    private Vector3[] vertices1;
    private Vector3[] vertices2;
    private Vector3[] vertices2AfterScalingAndRotating;
    private Vector2[] uv2;
    private Vector3[] normals1;
    private Vector3[] normals2;
    private int[] triangles1;
    private int[] triangles2;
    private Vector3[] triangles2MidPoint;
    private bool[] triangles2MidPointHandled;
    public float triangle1TravelDistance = 0.5f;
    private ComputeBuffer computeBuffer;
    public Material morphMaterial;
    public Material targetMaterial;
    public Texture targetTexture;
    public GameObject baseCoordinateObject;
    // Start is called before the first frame update
    void Awake()
    {
        myMeshStructure = GetComponent<MyMeshStructure>();
    }

    void Start()
    {
        
    }

    public int PrepareMorphing(GameObject gameObject)
    {
        meshFilter2Bool = false;
        if ((mesh2 = gameObject.GetComponent<MeshFilter>()) == null)
        {
            if ((mesh2 = gameObject.GetComponentInChildren<MeshFilter>()) == null)
            {
                if ((skinnedMeshRenderer2 = gameObject.GetComponent<SkinnedMeshRenderer>()) == null)
                {
                    skinnedMeshRenderer2 = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                }
            }
            else meshFilter2Bool = true;
        }
        else meshFilter2Bool = true;

        if (meshFilter2Bool)
        {
            meshRenderer2 = gameObject.GetComponent<MeshRenderer>();
            targetTexture = meshRenderer2.material.mainTexture;
            vertices2 = mesh2.mesh.vertices;
            uv2 = mesh2.mesh.uv;
            normals2 = mesh2.mesh.normals;
            triangles2 = mesh2.mesh.triangles;
        }
        else
        {
            targetTexture = skinnedMeshRenderer2.material.mainTexture;
            bakedMesh2 = new Mesh();
            skinnedMeshRenderer2.BakeMesh(bakedMesh2);
            vertices2 = bakedMesh2.vertices;
            uv2 = skinnedMeshRenderer2.sharedMesh.uv;
            normals2 = skinnedMeshRenderer2.sharedMesh.normals;
            triangles2 = skinnedMeshRenderer2.sharedMesh.triangles;
        }
        morphMaterial.SetTexture("_MorphTex", targetTexture);
        targetMaterial.SetTexture("_MainTex", targetTexture);

        triangles2MidPoint = new Vector3[triangles2.Length / 3];
        triangles2MidPointHandled = new bool[triangles2MidPoint.Length];
        for (int i = 0; i < triangles2MidPointHandled.Length; i++) triangles2MidPointHandled[i] = false;
        vertices1 = myMeshStructure.BasePositions;
        triangles1 = myMeshStructure.BaseTriangles;

        int equality1 = 0;

        if (triangles1.Length > triangles2.Length)
        {
            myMeshStructure.MeshDecimatingVertexMerging(triangles2.Length / 3);
            vertices1 = myMeshStructure.DecimatedPositions;
            normals1 = myMeshStructure.DecimatedNormals;
            triangles1 = myMeshStructure.DecimatedTriangles;
            equality1 = 1;
        }
        else if (triangles1.Length < triangles2.Length)
        {
            myMeshStructure.MeshRefiningTriangleSplitting(triangles2.Length / 3);
            vertices1 = myMeshStructure.RefinedPositions;
            normals1 = myMeshStructure.RefinedNormals;
            triangles1 = myMeshStructure.RefinedTriangles;
            equality1 = -1;
        }
        
        Mesh2PositionCorrecting();
        CalculateMidPointOfTriangle2();
        StoreDataForEachTriangle();
        return equality1;
    }

    public GameObject gameObjectCheckingForPrepareMorphing;
    int equality = 0;
    public void StartMorphing(GameObject gameObject)
    {
        if (gameObject != gameObjectCheckingForPrepareMorphing)
        {
            equality = PrepareMorphing(gameObject);
            gameObjectCheckingForPrepareMorphing = gameObject;
        }

        if (triangles1.Length == triangles2.Length)
        {
            myMeshStructure.SwapMesh(equality);
            morphMaterial.SetFloat("_TimeOffset", Time.timeSinceLevelLoad);
            myMeshStructure.SwapMaterial(morphMaterial);
        }
        else return;
    }

    public void ResetMorphing()
    {
        myMeshStructure.ResetMesh();
        myMeshStructure.RevertMaterial();
    }

    public void CalculateMidPointOfTriangle2()
    {
        for (int i = 0; i < triangles2.Length; i += 3)
        {
            triangles2MidPoint[i / 3] = (vertices2[triangles2[i]] + vertices2[triangles2[i + 1]] + vertices2[triangles2[i + 2]]) / 3;
        }
    }

    public Vector3 ConstructCorrectTransformForMesh2(Vector3 vertex)
    {
        Matrix4x4 matrix4X4;
        Matrix4x4 mesh1RotationMatrix = Matrix4x4.Rotate(baseCoordinateObject.transform.localRotation);
        mesh1RotationMatrix = mesh1RotationMatrix.inverse;

        if (meshFilter2Bool) 
        {
            matrix4X4 = mesh2.transform.localToWorldMatrix;
            matrix4X4.SetColumn(3, new Vector4(0, 0, 0, 1));
            return Vector3.zero;
        }
        else 
        {
            Matrix4x4 localPos = Matrix4x4.Translate(skinnedMeshRenderer2.transform.localPosition);
            Debug.Log("Local Pos: " + localPos);
            Matrix4x4 localRot = Matrix4x4.Rotate(skinnedMeshRenderer2.transform.localRotation);
            Debug.Log("Local Rot: " + localRot);
            matrix4X4 = skinnedMeshRenderer2.localToWorldMatrix;
            matrix4X4.SetColumn(3, new Vector4(0, 0, 0, 1));
            Debug.Log("Local to World: " + matrix4X4);

            return mesh1RotationMatrix.MultiplyPoint3x4(localRot.inverse.MultiplyPoint3x4(localPos.inverse.MultiplyPoint3x4(matrix4X4.MultiplyPoint3x4(vertex))));   
        }
    }

    public void Mesh2PositionCorrecting()
    {
        vertices2AfterScalingAndRotating = new Vector3[vertices2.Length];
        for (int i = 0; i < vertices2.Length; i++)
        {
            vertices2AfterScalingAndRotating[i] = ConstructCorrectTransformForMesh2(vertices2[i]);
        }
    }

    public List<PerTriangleData> perTriangleDatas = new List<PerTriangleData>();
    public Vector3[] Vertices1 { get => vertices1; set => vertices1 = value; }
    public Vector3[] Vertices2 { get => vertices2; set => vertices2 = value; }
    public Vector3[] Vertices2AfterScalingAndRotating { get => vertices2AfterScalingAndRotating; set => vertices2AfterScalingAndRotating = value; }
    public Vector2[] Uv2 { get => uv2; set => uv2 = value; }
    public int[] Triangles1 { get => triangles1; set => triangles1 = value; }
    public int[] Triangles2 { get => triangles2; set => triangles2 = value; }
    public Vector3[] Triangles2MidPoint { get => triangles2MidPoint; set => triangles2MidPoint = value; }
    public bool[] Triangles2MidPointHandled { get => triangles2MidPointHandled; set => triangles2MidPointHandled = value; }
    public Vector3[] Normals2 { get => normals2; set => normals2 = value; }

    public void StoreDataForEachTriangle()
    {
        computeBuffer?.Release();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Vector3 tempNormal, tempDirection;
        perTriangleDatas.Clear();
        int ip1, ip2;
        for (int i=0;i<triangles1.Length;i+=3)
        {
            ip1 = i + 1; ip2 = i + 2;
            tempNormal = Vector3.Cross(vertices1[triangles1[ip1]] - vertices1[triangles1[i]], vertices1[triangles1[ip2]] - vertices1[triangles1[i]]);
            tempNormal = tempNormal == Vector3.zero ? new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)) : tempNormal;
            tempDirection = tempNormal.normalized * triangle1TravelDistance;

            perTriangleDatas.Add
            (
                new PerTriangleData
                (
                    vertices2AfterScalingAndRotating[triangles2[i]],
                    vertices2AfterScalingAndRotating[triangles2[ip1]],
                    vertices2AfterScalingAndRotating[triangles2[ip2]],
                    uv2[triangles2[i]],
                    uv2[triangles2[ip1]],
                    uv2[triangles2[ip2]],
                    normals2[triangles2[i]],
                    normals2[triangles2[ip1]],
                    normals2[triangles2[ip2]],
                    tempDirection,
                    vertices1[triangles1[i]] + tempDirection,
                    vertices1[triangles1[ip1]] + tempDirection,
                    vertices1[triangles1[ip2]] + tempDirection
                )
            );
        }

        computeBuffer = new ComputeBuffer(perTriangleDatas.Count, 3 * sizeof(float) * 10 + 2 * sizeof(float) * 3);
        computeBuffer.SetData(perTriangleDatas.ToArray());
        stopwatch.Stop();
        Debug.Log("Time taken to store data for each triangle: " + stopwatch.ElapsedMilliseconds + " ms");

        morphMaterial.SetBuffer("_PerTriangleData", computeBuffer);
    }
}

public struct PerTriangleData
{
    public Vector3 v0;
    public Vector3 v1;
    public Vector3 v2;
    public Vector2 uv0;
    public Vector2 uv1;
    public Vector2 uv2;
    public Vector3 normal1;
    public Vector3 normal2;
    public Vector3 normal3;
    public Vector3 triangle1TravelDirection;
    public Vector3 v0MaxPos;
    public Vector3 v1MaxPos;
    public Vector3 v2MaxPos;

    public PerTriangleData(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector3 normal1, Vector3 normal2, Vector3 normal3, Vector3 triangle1TravelDirection, Vector3 v0MaxPos, Vector3 v1MaxPos, Vector3 v2MaxPos)
    {
        this.v0 = v0;
        this.v1 = v1;
        this.v2 = v2;
        this.uv0 = uv0;
        this.uv1 = uv1;
        this.uv2 = uv2;
        this.normal1 = normal1;
        this.normal2 = normal2;
        this.normal3 = normal3;
        this.triangle1TravelDirection = triangle1TravelDirection;
        this.v0MaxPos = v0MaxPos;
        this.v1MaxPos = v1MaxPos;
        this.v2MaxPos = v2MaxPos;
    }
}
