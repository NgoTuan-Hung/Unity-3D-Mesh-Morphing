using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RealMorph : MonoBehaviour
{
    public GameObject sourceObject;
    public GameObject targetObject;
    public MyMeshStructure sourceMeshStructure;
    public MyMeshStructure targetMeshStructure;
    public SkinnedMeshRenderer morphSkinnedMeshRenderer;
    public Mesh sourceBakedMesh;
    public Mesh targetBakedMesh;
    private Vector3[] sourceVertices;
    private Vector3[] targetVertices;
    private Vector3[] sourceCorrectPosition;
    private Vector3[] targetCorrectPosition;
    private Vector2[] sourceUVs;
    private Vector2[] targetUVs;
    private Vector3[] sourceNormals;
    private Vector3[] targetNormals;
    private int[] sourceTriangles;
    private int[] targetTriangles;
    public float triangle1TravelDistance = 0.5f;
    [SerializeField] private float morphTime = 1f;
    private ComputeBuffer computeBuffer;
    public Material morphMaterial;
    public Material targetMaterial;
    // Start is called before the first frame update
    void Awake()
    {
        morphSkinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        sourceMeshStructure = sourceObject.GetComponentInChildren<MyMeshStructure>();
        targetMeshStructure = targetObject.GetComponentInChildren<MyMeshStructure>();
        sourceBakedMesh = new Mesh();
        targetBakedMesh = new Mesh();
    }

    void Start()
    {
        
    }

    public void PrepareMorphing()
    {
        targetBakedMesh = new Mesh();
        morphMaterial.SetTexture("_MainTex", sourceMeshStructure.MainMaterial.mainTexture);
        morphMaterial.SetTexture("_MorphTex", targetMeshStructure.MainMaterial.mainTexture);
        morphMaterial.SetFloat("_TimeScale", 2 / morphTime);
        // targetMaterial = targetMeshStructure.MainMaterial;
        targetMaterial.SetTexture("_MainTex", targetMeshStructure.MainMaterial.mainTexture);
        sourceVertices = sourceMeshStructure.BasePositions;
        sourceTriangles = sourceMeshStructure.BaseTriangles;
        

        if (targetMeshStructure.MeshFilterBool)
        {
            targetVertices = targetMeshStructure.BasePositions;
            targetUVs = targetMeshStructure.BaseUVs;
            targetNormals = targetMeshStructure.BaseNormals;
            targetTriangles = targetMeshStructure.BaseTriangles;
        }
        else
        {
            targetMeshStructure.SkinnedMeshRenderer.BakeMesh(targetBakedMesh);
            targetVertices = targetBakedMesh.vertices;
            targetUVs = targetMeshStructure.BaseUVs;
            targetNormals = targetMeshStructure.BaseNormals;
            targetTriangles = targetMeshStructure.BaseTriangles;
        }

        if (sourceTriangles.Length > targetTriangles.Length)
        {
            if (sourceMeshStructure.MeshFilterBool) sourceMeshStructure.MeshDecimatingVertexMerging(targetTriangles.Length / 3, false);
            else sourceMeshStructure.MeshDecimatingVertexMerging(targetTriangles.Length / 3, true);

            sourceVertices = sourceMeshStructure.DecimatedPositions;
            sourceUVs = sourceMeshStructure.DecimatedUVs;
            sourceNormals = sourceMeshStructure.DecimatedNormals;
            sourceTriangles = sourceMeshStructure.DecimatedTriangles;
        }
        else if (sourceTriangles.Length < targetTriangles.Length)
        {
            if (sourceMeshStructure.MeshFilterBool) sourceMeshStructure.MeshRefiningTriangleSplitting(targetTriangles.Length / 3, false);
            else sourceMeshStructure.MeshRefiningTriangleSplitting(targetTriangles.Length / 3, true);

            sourceVertices = sourceMeshStructure.RefinedPositions;
            sourceUVs = sourceMeshStructure.RefinedUVs;
            sourceNormals = sourceMeshStructure.RefinedNormals;
            sourceTriangles = sourceMeshStructure.RefinedTriangles;
        }
        
        MeshPositionCorrecting();
        StoreDataForEachTriangle();
    }

    public void StartMorphing()
    {
        PrepareMorphing();

        morphMaterial.SetFloat("_TimeOffset", Time.timeSinceLevelLoad);
        morphSkinnedMeshRenderer.sharedMesh = new Mesh();
        morphSkinnedMeshRenderer.sharedMesh.SetVertices(sourceCorrectPosition);
        morphSkinnedMeshRenderer.sharedMesh.SetUVs(0, sourceUVs);
        morphSkinnedMeshRenderer.sharedMesh.SetNormals(sourceNormals);
        morphSkinnedMeshRenderer.sharedMesh.SetTriangles(sourceTriangles, 0);

        morphSkinnedMeshRenderer.material = morphMaterial;

        StartCoroutine(TrickSwapMesh());
    }

    IEnumerator TrickSwapMesh()
    {
        yield return new WaitForSeconds(morphTime);

        morphSkinnedMeshRenderer.sharedMesh.Clear();
        morphSkinnedMeshRenderer.sharedMesh.SetVertices(targetCorrectPosition);
        morphSkinnedMeshRenderer.sharedMesh.SetUVs(0, targetUVs);
        morphSkinnedMeshRenderer.sharedMesh.SetNormals(targetNormals);
        morphSkinnedMeshRenderer.sharedMesh.SetTriangles(targetTriangles, 0);

        morphSkinnedMeshRenderer.material = targetMaterial;
    }

    public void MeshPositionCorrecting()
    {
        Matrix4x4 matrix4X4;
        sourceCorrectPosition = new Vector3[sourceVertices.Length];
        if (sourceMeshStructure.MeshFilterBool) matrix4X4 = sourceMeshStructure.MeshFilter.transform.localToWorldMatrix;
        else matrix4X4 = sourceMeshStructure.SkinnedMeshRenderer.localToWorldMatrix;
        matrix4X4.SetColumn(3, new Vector4(0, 0, 0, 1));

        for (int i = 0; i < sourceVertices.Length; i++)
        {
            sourceCorrectPosition[i] = matrix4X4.MultiplyPoint3x4(sourceVertices[i]);
        }

        targetCorrectPosition = new Vector3[targetVertices.Length];
        if (targetMeshStructure.MeshFilterBool) matrix4X4 = targetMeshStructure.MeshFilter.transform.localToWorldMatrix;
        else matrix4X4 = targetMeshStructure.SkinnedMeshRenderer.localToWorldMatrix;
        matrix4X4.SetColumn(3, new Vector4(0, 0, 0, 1));

        for (int i = 0; i < targetVertices.Length; i++)
        {
            targetCorrectPosition[i] = matrix4X4.MultiplyPoint3x4(targetVertices[i]);
        }
    }

    public List<PerTriangleData> perTriangleDatas = new List<PerTriangleData>();

    public void StoreDataForEachTriangle()
    {
        computeBuffer?.Dispose();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Vector3 tempNormal, tempDirection;
        perTriangleDatas.Clear();
        int ip1, ip2;
        for (int i=0;i<sourceTriangles.Length;i+=3)
        {
            ip1 = i + 1; ip2 = i + 2;
            tempNormal = Vector3.Cross(sourceVertices[sourceTriangles[ip1]] - sourceVertices[sourceTriangles[i]], sourceVertices[sourceTriangles[ip2]] - sourceVertices[sourceTriangles[i]]);
            tempNormal = tempNormal == Vector3.zero ? new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)) : tempNormal;
            tempDirection = tempNormal.normalized * triangle1TravelDistance;

            perTriangleDatas.Add
            (
                new PerTriangleData
                (
                    targetCorrectPosition[targetTriangles[i]],
                    targetCorrectPosition[targetTriangles[ip1]],
                    targetCorrectPosition[targetTriangles[ip2]],
                    targetUVs[targetTriangles[i]],
                    targetUVs[targetTriangles[ip1]],
                    targetUVs[targetTriangles[ip2]],
                    targetNormals[targetTriangles[i]],
                    targetNormals[targetTriangles[ip1]],
                    targetNormals[targetTriangles[ip2]],
                    tempDirection,
                    sourceVertices[sourceTriangles[i]] + tempDirection,
                    sourceVertices[sourceTriangles[ip1]] + tempDirection,
                    sourceVertices[sourceTriangles[ip2]] + tempDirection
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
