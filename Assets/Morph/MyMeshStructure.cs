using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MyMeshStructure : MonoBehaviour
{
    private MeshFilter meshFilter;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private bool meshFilterBool = false;
    private bool skinnedMeshRendererBool = false;
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
    private List<Vertex> vertices;
    private List<Triangle> triangles;

    public MeshFilter MeshFilter { get => meshFilter; set => meshFilter = value; }
    public SkinnedMeshRenderer SkinnedMeshRenderer { get => skinnedMeshRenderer; set => skinnedMeshRenderer = value; }
    public bool MeshFilterBool { get => meshFilterBool; set => meshFilterBool = value; }
    public bool SkinnedMeshRendererBool { get => skinnedMeshRendererBool; set => skinnedMeshRendererBool = value; }
    public Vector3[] BasePositions { get => basePositions; set => basePositions = value; }
    public Vector3[] BaseNormals { get => baseNormals; set => baseNormals = value; }
    public Vector2[] BaseUVs { get => baseUVs; set => baseUVs = value; }
    public int[] BaseTriangles { get => baseTriangles; set => baseTriangles = value; }
    public Vector3[] DecimatedPositions { get => decimatedPositions; set => decimatedPositions = value; }
    public Vector3[] DecimatedNormals { get => decimatedNormals; set => decimatedNormals = value; }
    public Vector2[] DecimatedUVs { get => decimatedUVs; set => decimatedUVs = value; }
    public int[] DecimatedTriangles { get => decimatedTriangles; set => decimatedTriangles = value; }
    public List<Vertex> Vertices { get => vertices; set => vertices = value; }
    public List<Triangle> Triangles { get => triangles; set => triangles = value; }
    public Vector3[] RefinedPositions { get => refinedPositions; set => refinedPositions = value; }
    public Vector3[] RefinedNormals { get => refinedNormals; set => refinedNormals = value; }
    public Vector2[] RefinedUVs { get => refinedUVs; set => refinedUVs = value; }
    public int[] RefinedTriangles { get => refinedTriangles; set => refinedTriangles = value; }

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
        }
        else if (TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer))
        {
            skinnedMeshRendererBool = true;
            basePositions = skinnedMeshRenderer.sharedMesh.vertices;
            baseNormals = skinnedMeshRenderer.sharedMesh.normals;
            baseUVs = skinnedMeshRenderer.sharedMesh.uv;
            baseTriangles = skinnedMeshRenderer.sharedMesh.triangles;
        }
        else
        {
            Debug.LogError("No MeshFilter or SkinnedMeshRenderer found on the GameObject");
        }
    }

    public void ResetMesh()
    {
        if (meshFilterBool)
        {
            meshFilter.mesh.Clear(false);
            meshFilter.mesh.vertices = basePositions;
            meshFilter.mesh.normals = baseNormals;
            meshFilter.mesh.uv = baseUVs;
            meshFilter.mesh.triangles = baseTriangles;
        }
        else if (skinnedMeshRendererBool)
        {
            skinnedMeshRenderer.sharedMesh.Clear(false);
            skinnedMeshRenderer.sharedMesh.vertices = basePositions;
            skinnedMeshRenderer.sharedMesh.normals = baseNormals;
            skinnedMeshRenderer.sharedMesh.uv = baseUVs;
            skinnedMeshRenderer.sharedMesh.triangles = baseTriangles;
        }
    }

    public void GenerateMeshStructure()
    {
        vertices = new List<Vertex>();
        triangles = new List<Triangle>();
        for (int i = 0; i < basePositions.Length; i++)
        {
            Vertex vertex = new Vertex();
            vertex.position = basePositions[i];
            vertex.normal = baseNormals[i];
            vertex.uv = baseUVs[i];
            vertices.Add(vertex);
        }

        int ipj;
        for (int i = 0; i < baseTriangles.Length; i += 3)
        {
            Triangle triangle = new Triangle();
            triangle.vertices = new List<Vertex>();
            for (int j = 0; j < 3; j++)
            {
                ipj = i + j;
                triangle.vertices.Add(vertices[baseTriangles[ipj]]);
                vertices[baseTriangles[ipj]].triangles.Add(triangle);
            }
            triangles.Add(triangle);
        }
    }

    public void MeshDecimatingVertexMerging(int faceCount)
    {
        GenerateMeshStructure();
        Debug.Log("Current Face Count: " + triangles.Count);

        Vertex vcore, ncore;
        List<Triangle> unhandledTriangles, removedTriangles;
        Triangle tempTriangle;
        while (true)
        {
            if (triangles.Count <= faceCount + 1)
            {
                if (triangles.Count == faceCount + 1)
                {
                    vcore = vertices[Random.Range(0, vertices.Count)];
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
                    tempTriangle.vertices.ForEach(vertex => vertex.triangles.Remove(tempTriangle));
                    triangles.Remove(tempTriangle);

                    tempTriangle.vertices.ForEach(vertex => 
                    {
                        if (vertex.triangles.Count == 0) vertices.Remove(vertex);
                    });
                } else break;
            }
            else
            {
                vcore = vertices[Random.Range(0, vertices.Count)];
                unhandledTriangles = vcore.triangles;

                tempTriangle = unhandledTriangles[0];
                ncore = null;
                for (int i=0;i<3;i++) if (tempTriangle.vertices[i] != vcore)
                {
                    ncore = tempTriangle.vertices[i];
                    break;
                }

                removedTriangles = new List<Triangle>();
                for (int i=0;i<unhandledTriangles.Count;i++)
                {
                    if (unhandledTriangles[i].vertices.Contains(ncore))
                    {
                        removedTriangles.Add(unhandledTriangles[i]);
                        triangles.Remove(unhandledTriangles[i]);
                        unhandledTriangles.Remove(unhandledTriangles[i--]);
                    }
                }
                removedTriangles.ForEach(triangle => 
                {
                    triangle.vertices.ForEach(vertex => 
                    {
                        vertex.triangles.Remove(triangle);
                    });
                });

                unhandledTriangles.ForEach(triangle => 
                {
                    triangle.vertices[triangle.vertices.IndexOf(vcore)] = ncore;
                    ncore.triangles.Add(triangle);
                });
                vcore.triangles = new List<Triangle>();
                
                removedTriangles.ForEach(triangle => 
                    triangle.vertices.ForEach(vertex => 
                    {
                        if (vertex.triangles.Count == 0) vertices.Remove(vertex);
                    })
                );
            }
        }
        Debug.Log("Vertex Count: " + vertices.Count + "-----Final Vertex Count: " + vertices.Count);
        Debug.Log("Triangle Count: " + triangles.Count + "-----Final Triangle Count: " + triangles.Count);

        Vector3[] finalVerticesForMesh = new Vector3[vertices.Count];
        Vector3[] finalNormalsForMesh = new Vector3[vertices.Count];
        Vector2[] finalUVsForMesh = new Vector2[vertices.Count];
        int[] finalTrianglesForMesh = new int[triangles.Count * 3];
        for (int i=0;i<vertices.Count;i++)
        {
            finalVerticesForMesh[i] = vertices[i].position;
            finalNormalsForMesh[i] = vertices[i].normal;
            finalUVsForMesh[i] = vertices[i].uv;
        }
        for (int i=0;i<triangles.Count;i++)
        {
            finalTrianglesForMesh[i * 3] = vertices.IndexOf(triangles[i].vertices[0]);
            finalTrianglesForMesh[i * 3 + 1] = vertices.IndexOf(triangles[i].vertices[1]);
            finalTrianglesForMesh[i * 3 + 2] = vertices.IndexOf(triangles[i].vertices[2]);
        }

        if (meshFilterBool)
        {
            decimatedPositions = finalVerticesForMesh;
            decimatedNormals = finalNormalsForMesh;
            decimatedUVs = finalUVsForMesh;
            decimatedTriangles = finalTrianglesForMesh;
        }
        else if (skinnedMeshRendererBool)
        {
            #region Not Supported
            return;
            #endregion
        }
    }

    public void MeshRefiningTriangleSplitting(int faceCount)
    {
        GenerateMeshStructure();
        Debug.Log("Current Face Count: " + triangles.Count);

        Vertex vcore, ncore;
        List<Triangle> unhandledTriangles, removedTriangles;
        Triangle tempTriangle;

        Vertex v1,v2,v3;
        Triangle t1,t2,t3;

        while (true)
        {
            if (triangles.Count >= faceCount - 1)
            {
                if (triangles.Count == faceCount - 1)
                {
                    vcore = vertices[Random.Range(0, vertices.Count)];
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
                    tempTriangle.vertices.ForEach(vertex => vertex.triangles.Remove(tempTriangle));
                    triangles.Remove(tempTriangle);

                    tempTriangle.vertices.ForEach(vertex => 
                    {
                        if (vertex.triangles.Count == 0) vertices.Remove(vertex);
                    });
                } else break;
            }
            else
            {
                tempTriangle = vertices[Random.Range(0, vertices.Count)].triangles[0];

                v1 = tempTriangle.vertices[0];
                v2 = tempTriangle.vertices[1];
                v3 = tempTriangle.vertices[2];
                triangles.Remove(tempTriangle);
                v1.triangles.Remove(tempTriangle); v2.triangles.Remove(tempTriangle); v3.triangles.Remove(tempTriangle);
                vcore = new Vertex();
                vcore.position = (v1.position + v2.position + v3.position) / 3;
                vcore.normal = (v1.normal + v2.normal + v3.normal) / 3;
                vcore.uv = (v1.uv + v2.uv + v3.uv) / 3;
                vertices.Add(vcore);

                t1 = new Triangle();
                t1.vertices = new List<Vertex>();
                t1.vertices.Add(v1); t1.vertices.Add(v2); t1.vertices.Add(vcore);
                v1.triangles.Add(t1); v2.triangles.Add(t1); vcore.triangles.Add(t1);
                triangles.Add(t1);

                t2 = new Triangle();
                t2.vertices = new List<Vertex>();
                t2.vertices.Add(v2); t2.vertices.Add(v3); t2.vertices.Add(vcore);
                v2.triangles.Add(t2); v3.triangles.Add(t2); vcore.triangles.Add(t2);
                triangles.Add(t2);

                t3 = new Triangle();
                t3.vertices = new List<Vertex>();
                t3.vertices.Add(v3); t3.vertices.Add(v1); t3.vertices.Add(vcore);
                v3.triangles.Add(t3); v1.triangles.Add(t3); vcore.triangles.Add(t3);
                triangles.Add(t3);
            }
        }
        Debug.Log("Vertex Count: " + vertices.Count + "-----Final Vertex Count: " + vertices.Count);
        Debug.Log("Triangle Count: " + triangles.Count + "-----Final Triangle Count: " + triangles.Count);

        Vector3[] finalVerticesForMesh = new Vector3[vertices.Count];
        Vector3[] finalNormalsForMesh = new Vector3[vertices.Count];
        Vector2[] finalUVsForMesh = new Vector2[vertices.Count];
        int[] finalTrianglesForMesh = new int[triangles.Count * 3];
        for (int i=0;i<vertices.Count;i++)
        {
            finalVerticesForMesh[i] = vertices[i].position;
            finalNormalsForMesh[i] = vertices[i].normal;
            finalUVsForMesh[i] = vertices[i].uv;
        }
        for (int i=0;i<triangles.Count;i++)
        {
            finalTrianglesForMesh[i * 3] = vertices.IndexOf(triangles[i].vertices[0]);
            finalTrianglesForMesh[i * 3 + 1] = vertices.IndexOf(triangles[i].vertices[1]);
            finalTrianglesForMesh[i * 3 + 2] = vertices.IndexOf(triangles[i].vertices[2]);
        }

        if (meshFilterBool)
        {
            refinedPositions = finalVerticesForMesh;
            refinedNormals = finalNormalsForMesh;
            refinedUVs = finalUVsForMesh;
            refinedTriangles = finalTrianglesForMesh;
        }
        else if (skinnedMeshRendererBool)
        {
            #region Not Supported
            return;
            #endregion
        }
    }

    public void SwapMesh(int equality)
    {
        if (meshFilterBool)
        {
            if (equality == 1)
            {
                meshFilter.mesh.Clear(false);
                meshFilter.mesh.vertices = decimatedPositions;
                meshFilter.mesh.normals = decimatedNormals;
                meshFilter.mesh.uv = decimatedUVs;
                meshFilter.mesh.triangles = decimatedTriangles;
            }
            else if (equality == -1)
            {
                meshFilter.mesh.Clear(false);
                meshFilter.mesh.vertices = refinedPositions;
                meshFilter.mesh.normals = refinedNormals;
                meshFilter.mesh.uv = refinedUVs;
                meshFilter.mesh.triangles = refinedTriangles;
            }
            else return;
            
        }
        else if (skinnedMeshRendererBool)
        {
            #region Not Supported
            return;
            #endregion
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Vertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
    public List<Triangle> triangles = new List<Triangle>();

    public String ToString()
    {
        return "Position: " + position.ToString() + "-----Normal: " + normal.ToString() + "-----UV: " + uv.ToString() + "-----Triangle Count: " + triangles.Count;
    }
}

public class Triangle
{
    public List<Vertex> vertices;
}
