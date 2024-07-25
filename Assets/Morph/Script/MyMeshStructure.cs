using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.IO;
public class MyMeshStructure : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Mesh bakedMesh;
    private Vector3[] bakedPosition;
    private Vector3[] bakedNormals;
    private Vector2[] bakedUVs;
    private int[] bakedTriangles;
    private bool meshFilterBool = false;
    private Vector3[] basePositions;
    private Vector3[] baseNormals;
    private Vector2[] baseUVs;
    private int[] baseTriangles;
    private Vector3[] decimatedPositions;
    private Vector3[] decimatedNormals;
    private Vector2[] decimatedUVs;
    private int[] decimatedTriangles;
    private Vector3[] refinedPositions;
    private Vector3[] refinedNormals;
    private Vector2[] refinedUVs;
    private int[] refinedTriangles;
    private List<Vertex> verticesData;
    private List<Triangle> trianglesData;
    private Material mainMaterial;
    private BinarySave binarySave = new BinarySave();
    public MeshFilter MeshFilter { get => meshFilter; set => meshFilter = value; }
    public SkinnedMeshRenderer SkinnedMeshRenderer { get => skinnedMeshRenderer; set => skinnedMeshRenderer = value; }
    public bool MeshFilterBool { get => meshFilterBool; set => meshFilterBool = value; }
    public Vector3[] BasePositions { get => basePositions; set => basePositions = value; }
    public Vector3[] BaseNormals { get => baseNormals; set => baseNormals = value; }
    public Vector2[] BaseUVs { get => baseUVs; set => baseUVs = value; }
    public int[] BaseTriangles { get => baseTriangles; set => baseTriangles = value; }
    public Vector3[] DecimatedPositions { get => decimatedPositions; set => decimatedPositions = value; }
    public Vector3[] DecimatedNormals { get => decimatedNormals; set => decimatedNormals = value; }
    public Vector2[] DecimatedUVs { get => decimatedUVs; set => decimatedUVs = value; }
    public int[] DecimatedTriangles { get => decimatedTriangles; set => decimatedTriangles = value; }
    public List<Vertex> VerticesData { get => verticesData; set => verticesData = value; }
    public List<Triangle> TrianglesData { get => trianglesData; set => trianglesData = value; }
    public Vector3[] RefinedPositions { get => refinedPositions; set => refinedPositions = value; }
    public Vector3[] RefinedNormals { get => refinedNormals; set => refinedNormals = value; }
    public Vector2[] RefinedUVs { get => refinedUVs; set => refinedUVs = value; }
    public int[] RefinedTriangles { get => refinedTriangles; set => refinedTriangles = value; }
    public MeshRenderer MeshRenderer { get => meshRenderer; set => meshRenderer = value; }
    public Material MainMaterial { get => mainMaterial; set => mainMaterial = value; }
    public Mesh BakedMesh { get => bakedMesh; set => bakedMesh = value; }

    // Start is called before the first frame update
    void Awake()
    {
        if (TryGetComponent<MeshFilter>(out meshFilter))
        {
            meshFilterBool = true;
            basePositions = meshFilter.mesh.vertices;
            baseNormals = meshFilter.mesh.normals;
            baseUVs = meshFilter.mesh.uv;
            baseTriangles = meshFilter.mesh.triangles;
            meshRenderer = GetComponent<MeshRenderer>();
            mainMaterial = meshRenderer.material;
        }
        else
        {
            if ((meshFilter = GetComponentInChildren<MeshFilter>()) != null)
            {
                meshFilterBool = true;
                basePositions = meshFilter.mesh.vertices;
                baseNormals = meshFilter.mesh.normals;
                baseUVs = meshFilter.mesh.uv;
                baseTriangles = meshFilter.mesh.triangles;
                meshRenderer = GetComponentInChildren<MeshRenderer>();
                mainMaterial = meshRenderer.material;
            }
            else
            {
                if (!TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer))
                {
                    skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                }

                basePositions = skinnedMeshRenderer.sharedMesh.vertices;
                baseNormals = skinnedMeshRenderer.sharedMesh.normals;
                baseUVs = skinnedMeshRenderer.sharedMesh.uv;
                baseTriangles = skinnedMeshRenderer.sharedMesh.triangles;
                mainMaterial = skinnedMeshRenderer.material;
            }
        }
    }
    public void GenerateMeshStructure(bool baked, bool genTris)
    {
        verticesData = new List<Vertex>();
        trianglesData = new List<Triangle>();
        if (baked)
        {
            Stopwatch stopwatch = new Stopwatch();
            bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh, true);
            bakedMesh.vertices.CopyTo(bakedPosition = new Vector3[bakedMesh.vertices.Length], 0);
            
            stopwatch.Start();
            for (int i = 0; i < basePositions.Length; i++)
            {
                Vertex vertex = new Vertex();
                vertex.position = bakedPosition[i];
                vertex.normal = baseNormals[i];
                vertex.uv = baseUVs[i];
                vertex.index = i;
                verticesData.Add(vertex);
            }
        }
        else
        {
            for (int i = 0; i < basePositions.Length; i++)
            {
                Vertex vertex = new Vertex();
                vertex.position = basePositions[i];
                vertex.normal = baseNormals[i];
                vertex.uv = baseUVs[i];
                verticesData.Add(vertex);
            }
        }
        

        if (genTris)
        {
            int ipj;
            for (int i = 0; i < baseTriangles.Length; i += 3)
            {
                Triangle triangle = new Triangle();
                for (int j = 0; j < 3; j++)
                {
                    ipj = i + j;
                    triangle.vertices.Add(verticesData[baseTriangles[ipj]]);
                    verticesData[baseTriangles[ipj]].triangles.Add(triangle);
                }
                trianglesData.Add(triangle);
            }
        }
    }
    public void MeshDecimatingVertexMerging(int faceCount, bool baked, string objectName)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<Vertex> copyTempVerticesData = new List<Vertex>();
        List<Triangle> copyTempTrianglesData = new List<Triangle>();
        String file = "Assets\\MeshData\\" +  objectName + faceCount + ".bin";
        Debug.Log("Current Face Count: " + baseTriangles.Length / 3);
        
        if (File.Exists(file))
        {
            binarySave.LoadFromFile(file);
            GenerateMeshStructure(baked, false);
            int shrink = 0;

            for (int i=0;i<verticesData.Count;i++)
            {
                if (binarySave.loadedVertices[i].IsNull)
                {
                    shrink++;
                }
                else
                {
                    if (binarySave.loadedVertices[i].Changed)
                    {
                        verticesData[i].position = (verticesData[binarySave.loadedVertices[i].ChangedDat1].position + verticesData[binarySave.loadedVertices[i].ChangedDat2].position) / 2;
                        verticesData[i].normal = (verticesData[binarySave.loadedVertices[i].ChangedDat1].normal + verticesData[binarySave.loadedVertices[i].ChangedDat2].normal) / 2;
                        verticesData[i].uv = (verticesData[binarySave.loadedVertices[i].ChangedDat1].uv + verticesData[binarySave.loadedVertices[i].ChangedDat2].uv) / 2;
                    }
                    
                    verticesData[i].index = i - shrink;
                    copyTempVerticesData.Add(verticesData[i]);
                }
            }

            for (int i=0;i<binarySave.loadedTriangles.Count;i++)
            {
                if (!binarySave.loadedTriangles[i].IsNull)
                {
                    Triangle triangle = new Triangle();
                    triangle.vertices.Add(copyTempVerticesData[verticesData[binarySave.loadedTriangles[i].V1].index]);
                    triangle.vertices.Add(copyTempVerticesData[verticesData[binarySave.loadedTriangles[i].V2].index]);
                    triangle.vertices.Add(copyTempVerticesData[verticesData[binarySave.loadedTriangles[i].V3].index]);

                    copyTempTrianglesData.Add(triangle);
                }
            }
        }
        else
        {
            GenerateMeshStructure(baked, true);
            Vertex vcore, ncore;
            List<Triangle> unhandledTriangles, removedTriangles, filteredTriangles;
            Triangle tempTriangle;
            long vcoreTravelIndex = 0;
            int currentFaceCount = trianglesData.Count;

            while (true)
            {
                if (currentFaceCount <= faceCount + 1)
                {
                    if (currentFaceCount == faceCount + 1)
                    {
                        vcoreTravelIndex = Random.Range(0, verticesData.Count);
                        vcore = verticesData[(int)vcoreTravelIndex];
                        while (vcore.isNull)
                        {
                            vcore = verticesData[(int)(vcoreTravelIndex++ % verticesData.Count)];
                        }
                        tempTriangle = vcore.triangles[0];
                        Vertex n1 = null, n2 = null;
                        for (int i=0;i<3;i++) if (tempTriangle.vertices[i] != vcore)
                        {
                            if (n1 == null) n1 = tempTriangle.vertices[i];
                            else n2 = tempTriangle.vertices[i];
                        }
                        vcore.position = (n1.position + n2.position) / 2;
                        vcore.normal = (n1.normal + n2.normal) / 2;
                        vcore.uv = (n1.uv + n2.uv) / 2;
                        vcore.isChanged = true;
                        vcore.changedData.Add(n1.index); vcore.changedData.Add(n2.index);
                        tempTriangle.vertices.ForEach(vertex => vertex.triangles.Remove(tempTriangle));
                        tempTriangle.isNull = true; currentFaceCount--;
                        tempTriangle.vertices.ForEach(vertex => 
                        {
                            if (vertex.triangles.Count == 0) 
                            vertex.isNull = true;
                        });
                    } else break;
                }
                else
                {
                    vcoreTravelIndex = Random.Range(0, verticesData.Count);
                    vcore = verticesData[(int)vcoreTravelIndex];
                    while (vcore.isNull)
                    {
                        vcore = verticesData[(int)(vcoreTravelIndex++ % verticesData.Count)];
                    }
                    unhandledTriangles = vcore.triangles;
                    tempTriangle = unhandledTriangles[0];
                    ncore = null;
                    for (int i=0;i<3;i++) if (tempTriangle.vertices[i] != vcore)
                    {
                        ncore = tempTriangle.vertices[i];
                        break;
                    }
                    removedTriangles = new List<Triangle>();
                    filteredTriangles = new List<Triangle>();
                    for (int i=0;i<unhandledTriangles.Count;i++)
                    {
                        if (unhandledTriangles[i].vertices.Contains(ncore))
                        {
                            removedTriangles.Add(unhandledTriangles[i]);
                            unhandledTriangles[i].isNull = true;
                            currentFaceCount--;
                        }
                        else filteredTriangles.Add(unhandledTriangles[i]);
                    }
                    removedTriangles.ForEach(triangle => 
                    {
                        triangle.vertices.ForEach(vertex => 
                        {
                            vertex.triangles.Remove(triangle);
                        });
                    });
                    filteredTriangles.ForEach(triangle => 
                    {
                        triangle.vertices[triangle.vertices.IndexOf(vcore)] = ncore;
                        ncore.triangles.Add(triangle);
                    });
                    vcore.triangles = new List<Triangle>();
                    
                    removedTriangles.ForEach(triangle => 
                        triangle.vertices.ForEach(vertex => 
                        {
                            if (vertex.triangles.Count == 0) 
                            vertex.isNull = true;
                        })
                    );
                }
            }
            binarySave.SaveToFile(file, verticesData, trianglesData);

            for (int i=0;i<verticesData.Count;i++) if (!verticesData[i].isNull) copyTempVerticesData.Add(verticesData[i]);
            for (int i=0;i<trianglesData.Count;i++) if (!trianglesData[i].isNull) copyTempTrianglesData.Add(trianglesData[i]);
        }
        
        verticesData = copyTempVerticesData;
        trianglesData = copyTempTrianglesData;
        Debug.Log("Vertex Count: " + verticesData.Count + "-----Final Vertex Count: " + verticesData.Count);
        Debug.Log("Triangle Count: " + trianglesData.Count + "-----Final Triangle Count: " + trianglesData.Count);
        Vector3[] finalVerticesForMesh = new Vector3[verticesData.Count];
        Vector3[] finalNormalsForMesh = new Vector3[verticesData.Count];
        Vector2[] finalUVsForMesh = new Vector2[verticesData.Count];
        int[] finalTrianglesForMesh = new int[trianglesData.Count * 3];
        for (int i=0;i<verticesData.Count;i++)
        {
            finalVerticesForMesh[i] = verticesData[i].position;
            finalNormalsForMesh[i] = verticesData[i].normal;
            finalUVsForMesh[i] = verticesData[i].uv;
            verticesData[i].finalIndex = i;
        }
        for (int i=0;i<trianglesData.Count;i++)
        {
            finalTrianglesForMesh[i * 3] = trianglesData[i].vertices[0].finalIndex;
            finalTrianglesForMesh[i * 3 + 1] = trianglesData[i].vertices[1].finalIndex;
            finalTrianglesForMesh[i * 3 + 2] = trianglesData[i].vertices[2].finalIndex;
        }
        
        decimatedPositions = finalVerticesForMesh;
        decimatedNormals = finalNormalsForMesh;
        decimatedUVs = finalUVsForMesh;
        decimatedTriangles = finalTrianglesForMesh;

        stopwatch.Stop();
        Debug.Log("Time taken to decimate mesh: " + stopwatch.ElapsedMilliseconds + " ms");
    }
    public void MeshRefiningTriangleSplitting(int faceCount, bool baked, string objectName)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<Vertex> copyTempVerticesData = new List<Vertex>();
        List<Triangle> copyTempTrianglesData = new List<Triangle>();
        String file = "Assets\\MeshData\\" +  objectName + faceCount + ".bin";
        Debug.Log("Current Face Count: " + baseTriangles.Length / 3);

        if (File.Exists(file))
        {
            int newVert = binarySave.LoadFromFile(file, basePositions.Length);
            GenerateMeshStructure(baked, false);
            int currentVerts = verticesData.Count;
            int totalVerts = currentVerts + newVert;
            int shrink = 0;

            for (int i=0;i<verticesData.Count;i++)
            {
                if (binarySave.loadedVertices[i].IsNull)
                {
                    shrink++;
                }
                else
                {
                    if (binarySave.loadedVertices[i].Changed)
                    {
                        verticesData[i].position = (verticesData[binarySave.loadedVertices[i].ChangedDat1].position + verticesData[binarySave.loadedVertices[i].ChangedDat2].position) / 2;
                        verticesData[i].normal = (verticesData[binarySave.loadedVertices[i].ChangedDat1].normal + verticesData[binarySave.loadedVertices[i].ChangedDat2].normal) / 2;
                        verticesData[i].uv = (verticesData[binarySave.loadedVertices[i].ChangedDat1].uv + verticesData[binarySave.loadedVertices[i].ChangedDat2].uv) / 2;
                    }
                    
                    verticesData[i].index = i - shrink;
                    copyTempVerticesData.Add(verticesData[i]);
                }
            }

            for (int i=currentVerts;i<totalVerts;i++)
            {
                Vertex vertex = new Vertex();
                vertex.position = (verticesData[binarySave.loadedVertices[i].NewDat1].position + verticesData[binarySave.loadedVertices[i].NewDat2].position + verticesData[binarySave.loadedVertices[i].NewDat3].position) / 3;
                vertex.normal = (verticesData[binarySave.loadedVertices[i].NewDat1].normal + verticesData[binarySave.loadedVertices[i].NewDat2].normal + verticesData[binarySave.loadedVertices[i].NewDat3].normal) / 3;
                vertex.uv = (verticesData[binarySave.loadedVertices[i].NewDat1].uv + verticesData[binarySave.loadedVertices[i].NewDat2].uv + verticesData[binarySave.loadedVertices[i].NewDat3].uv) / 3;

                vertex.index = i - shrink;
                copyTempVerticesData.Add(vertex);
                verticesData.Add(vertex);
            }

            for (int i=0;i<binarySave.loadedTriangles.Count;i++)
            {
                if (!binarySave.loadedTriangles[i].IsNull)
                {
                    Triangle triangle = new Triangle();
                    triangle.vertices.Add(copyTempVerticesData[verticesData[binarySave.loadedTriangles[i].V1].index]);
                    triangle.vertices.Add(copyTempVerticesData[verticesData[binarySave.loadedTriangles[i].V2].index]);
                    triangle.vertices.Add(copyTempVerticesData[verticesData[binarySave.loadedTriangles[i].V3].index]);

                    copyTempTrianglesData.Add(triangle);
                }
            }
        }
        else
        {
            GenerateMeshStructure(baked, true);
            Vertex vcore, ncore;
            Triangle tempTriangle;
            Vertex v1,v2,v3;
            Triangle t1,t2,t3;
            long vcoreTravelIndex = 0;
            int currentFaceCount = trianglesData.Count;
            while (true)
            {
                if (currentFaceCount >= faceCount - 1)
                {
                    if (currentFaceCount == faceCount - 1)
                    {
                        vcoreTravelIndex = Random.Range(0, verticesData.Count);
                        vcore = verticesData[(int)vcoreTravelIndex];
                        tempTriangle = vcore.triangles[0];
                        Vertex n1 = null, n2 = null;
                        for (int i=0;i<3;i++) if (tempTriangle.vertices[i] != vcore)
                        {
                            if (n1 == null) n1 = tempTriangle.vertices[i];
                            else n2 = tempTriangle.vertices[i];
                        }
                        vcore.position = (n1.position + n2.position) / 2;
                        vcore.normal = (n1.normal + n2.normal) / 2;
                        vcore.uv = (n1.uv + n2.uv) / 2;
                        vcore.isChanged = true;
                        vcore.changedData.Add(n1.index); vcore.changedData.Add(n2.index);
                        tempTriangle.vertices.ForEach(vertex => vertex.triangles.Remove(tempTriangle));
                        tempTriangle.isNull = true; currentFaceCount--;
                        tempTriangle.vertices.ForEach(vertex => 
                        {
                            if (vertex.triangles.Count == 0) 
                            vertex.isNull = true;
                        });
                    } else break;
                }
                else
                {
                    vcoreTravelIndex = Random.Range(0, verticesData.Count);
                    ncore = verticesData[(int)vcoreTravelIndex];
                    tempTriangle = ncore.triangles[0];
                    v1 = tempTriangle.vertices[0];
                    v2 = tempTriangle.vertices[1];
                    v3 = tempTriangle.vertices[2];
                    
                    tempTriangle.isNull = true;
                    v1.triangles.Remove(tempTriangle); v2.triangles.Remove(tempTriangle); v3.triangles.Remove(tempTriangle);
                    vcore = new Vertex();
                    vcore.position = (v1.position + v2.position + v3.position) / 3;
                    vcore.normal = (v1.normal + v2.normal + v3.normal) / 3;
                    vcore.uv = (v1.uv + v2.uv + v3.uv) / 3;
                    vcore.index = verticesData.Count;
                    vcore.newData.Add(v1.index); vcore.newData.Add(v2.index); vcore.newData.Add(v3.index);
                    verticesData.Add(vcore);
                    t1 = new Triangle();
                    t1.vertices = new List<Vertex>();
                    t1.vertices.Add(v1); t1.vertices.Add(v2); t1.vertices.Add(vcore);
                    v1.triangles.Add(t1); v2.triangles.Add(t1); vcore.triangles.Add(t1);
                    trianglesData.Add(t1);
                    t2 = new Triangle();
                    t2.vertices = new List<Vertex>();
                    t2.vertices.Add(v2); t2.vertices.Add(v3); t2.vertices.Add(vcore);
                    v2.triangles.Add(t2); v3.triangles.Add(t2); vcore.triangles.Add(t2);
                    trianglesData.Add(t2);
                    t3 = new Triangle();
                    t3.vertices = new List<Vertex>();
                    t3.vertices.Add(v3); t3.vertices.Add(v1); t3.vertices.Add(vcore);
                    v3.triangles.Add(t3); v1.triangles.Add(t3); vcore.triangles.Add(t3);
                    trianglesData.Add(t3);
                    currentFaceCount += 2;
                }
            }
            binarySave.SaveToFile(file, verticesData, trianglesData);

            for (int i=0;i<verticesData.Count;i++) if (!verticesData[i].isNull) copyTempVerticesData.Add(verticesData[i]);
            for (int i=0;i<trianglesData.Count;i++) if (!trianglesData[i].isNull) copyTempTrianglesData.Add(trianglesData[i]);
        }

        verticesData = copyTempVerticesData;
        trianglesData = copyTempTrianglesData;
        Debug.Log("Vertex Count: " + verticesData.Count + "-----Final Vertex Count: " + verticesData.Count);
        Debug.Log("Triangle Count: " + trianglesData.Count + "-----Final Triangle Count: " + trianglesData.Count);
        Vector3[] finalVerticesForMesh = new Vector3[verticesData.Count];
        Vector3[] finalNormalsForMesh = new Vector3[verticesData.Count];
        Vector2[] finalUVsForMesh = new Vector2[verticesData.Count];
        int[] finalTrianglesForMesh = new int[trianglesData.Count * 3];
        for (int i=0;i<verticesData.Count;i++)
        {
            finalVerticesForMesh[i] = verticesData[i].position;
            finalNormalsForMesh[i] = verticesData[i].normal;
            finalUVsForMesh[i] = verticesData[i].uv;
            verticesData[i].finalIndex = i;
        }
        for (int i=0;i<trianglesData.Count;i++)
        {
            finalTrianglesForMesh[i * 3] = trianglesData[i].vertices[0].finalIndex;
            finalTrianglesForMesh[i * 3 + 1] = trianglesData[i].vertices[1].finalIndex;
            finalTrianglesForMesh[i * 3 + 2] = trianglesData[i].vertices[2].finalIndex;
        }

        refinedPositions = finalVerticesForMesh;
        refinedNormals = finalNormalsForMesh;
        refinedUVs = finalUVsForMesh;
        refinedTriangles = finalTrianglesForMesh;

        stopwatch.Stop();
        Debug.Log("Time taken to refine mesh: " + stopwatch.ElapsedMilliseconds + " ms");
    }
}
public class Vertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
    public List<Triangle> triangles = new List<Triangle>();
    public bool isChanged = false;
    public List<int> changedData = new List<int>();
    public List<int> newData = new List<int>();
    public bool isNull = false;
    public int index;
    public int finalIndex;
    public String ToString()
    {
        return "Position: " + position.ToString() + "-----Normal: " + normal.ToString() + "-----UV: " + uv.ToString() + "-----Triangle Count: " + triangles.Count;
    }
    public Vertex()
    {
    }
    public Vertex(Vertex vertex)
    {
        position = vertex.position;
        normal = vertex.normal;
        uv = vertex.uv;
    }
}
public class Triangle
{
    public List<Vertex> vertices = new List<Vertex>();
    public bool isNull = false;
}
