using System;
using System.Collections.Generic;
using System.IO;

public class VertexBinary
{
    public int Id { get; set; }
    public bool Changed { get; set; }
    public int ChangedDat1 { get; set; }
    public int ChangedDat2 { get; set; }
    public bool IsNull { get; set; }
    public int NewDat1 { get; set; }
    public int NewDat2 { get; set; }
    public int NewDat3 { get; set; }
}

public class TriangleBinary
{
    public int V1 { get; set; }
    public int V2 { get; set; }
    public int V3 { get; set; }
    public bool IsNull { get; set; }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(V1);
        writer.Write(V2);
        writer.Write(V3);
        writer.Write(IsNull);
    }
}

public class BinarySave
{
    public List<VertexBinary> loadedVertices;
    public List<TriangleBinary> loadedTriangles;
    public void Main1()
    {

        // Save to file
        // SaveToFile("data.bin", vertices, triangles);

        // Load from file
        
        // LoadFromFile("data.bin", out loadedVertices, out loadedTriangles);

        // foreach (var triangle in loadedTriangles)
        // {
        //     Console.WriteLine($"Triangle {triangle.Id} has vertices:");
        //     foreach (var vertex in triangle.Vertices)
        //     {
        //         Console.WriteLine($"- Vertex {vertex.Id}");
        //     }
        // }
    }

    public void Serialize(BinaryWriter writer, Vertex vertex)
    {
        writer.Write(vertex.index);
        writer.Write(vertex.isNull);
        writer.Write(vertex.isChanged);
        if (vertex.isChanged)
        {
            writer.Write(vertex.changedData[0]);
            writer.Write(vertex.changedData[1]);
        }
        if (vertex.newData.Count != 0)
        {
            writer.Write(vertex.newData[0]);
            writer.Write(vertex.newData[1]);
            writer.Write(vertex.newData[2]);
        }
    }

    public void Serialize(BinaryWriter writer, Triangle triangle)
    {
        writer.Write(triangle.vertices[0].index);
        writer.Write(triangle.vertices[1].index);
        writer.Write(triangle.vertices[2].index);
        writer.Write(triangle.isNull);
    }

    public void SaveToFile(string filename, List<Vertex> vertices, List<Triangle> triangles)
    {
        using BinaryWriter writer = new(File.Open(filename, FileMode.Create));
        
        vertices.ForEach(vertex => 
        {
            Serialize(writer, vertex);
        });

        triangles.ForEach(triangle => 
        {
            Serialize(writer, triangle);
        });
    }

    public void LoadFromFile(string filename, out List<VertexBinary> vertices, out List<TriangleBinary> triangles)
    {
        vertices = new List<VertexBinary>();
        triangles = new List<TriangleBinary>();

        using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
        {
            
        }
    }
}