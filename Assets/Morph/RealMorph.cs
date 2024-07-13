using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealMorph : MonoBehaviour
{
    public MeshFilter mesh1;
    public MeshFilter mesh2;
    public MeshRenderer meshRenderer2;
    public MyMeshStructure myMeshStructure;
    private Vector3[] vertices1;
    private Vector3[] vertices2;
    private Vector3[] vertices2AfterScalingAndRotating;
    private Vector2[] uv2;
    private Vector3[] normals2;
    private int[] triangles1;
    private int[] triangles2;
    private Vector3[] triangles2MidPoint;
    private bool[] triangles2MidPointHandled;
    public float triangle1TravelDistance = 100f;
    private ComputeBuffer computeBuffer;
    public Material morphMaterial;
    public Material normalMaterial;
    public GameObject baseCoordinateObject;
    public MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        mesh1 = GetComponent<MeshFilter>();
        myMeshStructure = GetComponent<MyMeshStructure>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        
    }

    public int PrepareMorphing(GameObject gameObject)
    {
        mesh2 = gameObject.GetComponent<MeshFilter>();
        meshRenderer2 = gameObject.GetComponent<MeshRenderer>();
        vertices2 = mesh2.mesh.vertices;
        triangles2 = mesh2.mesh.triangles;
        triangles2MidPoint = new Vector3[triangles2.Length / 3];
        uv2 = mesh2.mesh.uv;
        normals2 = mesh2.mesh.normals;
        triangles2MidPointHandled = new bool[triangles2MidPoint.Length];
        for (int i = 0; i < triangles2MidPointHandled.Length; i++) triangles2MidPointHandled[i] = false;
        vertices1 = mesh1.mesh.vertices;
        triangles1 = mesh1.mesh.triangles;
        morphMaterial.SetTexture("_MorphTex", meshRenderer.material.mainTexture);
        int equality1 = 0;
        if (triangles1.Length > triangles2.Length)
        {
            myMeshStructure.MeshDecimatingVertexMerging(triangles2.Length / 3);
            vertices1 = myMeshStructure.DecimatedPositions;
            triangles1 = myMeshStructure.DecimatedTriangles;
            equality1 = 1;
        }
        else if (triangles1.Length < triangles2.Length)
        {
            myMeshStructure.MeshRefiningTriangleSplitting(triangles2.Length / 3);
            vertices1 = myMeshStructure.RefinedPositions;
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
            morphMaterial.SetFloat("_Triangle1TravelDistance", triangle1TravelDistance);
            morphMaterial.SetFloat("_TimeOffset", Time.timeSinceLevelLoad);
            meshRenderer.material = morphMaterial;
        }
        else return;
    }

    public void ResetMorphing()
    {
        myMeshStructure.ResetMesh();
        meshRenderer.material = normalMaterial;
    }

    public void CalculateMidPointOfTriangle2()
    {
        for (int i = 0; i < triangles2.Length; i += 3)
        {
            triangles2MidPoint[i / 3] = (vertices2[triangles2[i]] + vertices2[triangles2[i + 1]] + vertices2[triangles2[i + 2]]) / 3;
        }
    }

    public Matrix4x4 ConstructRotationAndScaleMatrixForMesh2()
    {
        Matrix4x4 matrix4X4 = mesh2.transform.localToWorldMatrix;
        matrix4X4.SetColumn(3, new Vector4(0, 0, 0, 1));
        Matrix4x4 mesh1RotationMatrix = Matrix4x4.Rotate(baseCoordinateObject.transform.localRotation);
        mesh1RotationMatrix = mesh1RotationMatrix.inverse;
        return mesh1RotationMatrix * matrix4X4;
    }

    public void Mesh2PositionCorrecting()
    {
        vertices2AfterScalingAndRotating = new Vector3[vertices2.Length];
        Matrix4x4 matrix4X4 = ConstructRotationAndScaleMatrixForMesh2();
        for (int i = 0; i < vertices2.Length; i++)
        {
            vertices2AfterScalingAndRotating[i] = matrix4X4.MultiplyPoint3x4(vertices2[i]);
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
        computeBuffer?.Dispose();
        Vector3 tempNormal, tempMidPointPosition, tempDirection;
        float tempMinDistance, currentDistance;
        int markedMinPosition = 0;
        perTriangleDatas.Clear();
        for (int i=0;i<triangles1.Length;i+=3)
        {
            tempNormal = Vector3.Cross(vertices1[triangles1[i + 1]] - vertices1[triangles1[i]], vertices1[triangles1[i + 2]] - vertices1[triangles1[i]]);
            tempMidPointPosition = 
            (
                (vertices1[triangles1[i]] + vertices1[triangles1[i + 1]] + vertices1[triangles1[i + 2]]) / 3
                + tempNormal.normalized * triangle1TravelDistance
            );
            tempMinDistance= float.MaxValue;

            for (int j=0;j<triangles2MidPoint.Length;j++)
            {
                if (!triangles2MidPointHandled[j])
                {
                    currentDistance = Vector3.Distance(tempMidPointPosition, triangles2MidPoint[j]);
                    if (currentDistance < tempMinDistance)
                    {
                        tempMinDistance = currentDistance;
                        markedMinPosition = j;
                    }
                }
            }
            triangles2MidPointHandled[markedMinPosition] = true;

            markedMinPosition *= 3;
            tempDirection = tempNormal * triangle1TravelDistance;
            perTriangleDatas.Add
            (
                new PerTriangleData
                (
                    vertices2AfterScalingAndRotating[triangles2[markedMinPosition]],
                    vertices2AfterScalingAndRotating[triangles2[markedMinPosition + 1]],
                    vertices2AfterScalingAndRotating[triangles2[markedMinPosition + 2]],
                    uv2[triangles2[markedMinPosition]],
                    uv2[triangles2[markedMinPosition + 1]],
                    uv2[triangles2[markedMinPosition + 2]],
                    normals2[triangles2[markedMinPosition]],
                    normals2[triangles2[markedMinPosition + 1]],
                    normals2[triangles2[markedMinPosition + 2]],
                    tempDirection,
                    vertices1[triangles1[i]] + tempDirection,
                    vertices1[triangles1[i + 1]] + tempDirection,
                    vertices1[triangles1[i + 2]] + tempDirection
                )
            );
        }

        computeBuffer = new ComputeBuffer(perTriangleDatas.Count, 3 * sizeof(float) * 10 + 2 * sizeof(float) * 3);
        computeBuffer.SetData(perTriangleDatas.ToArray());

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
