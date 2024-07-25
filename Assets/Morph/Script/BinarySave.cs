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

    public VertexBinary DeserializeVertex(BinaryReader reader)
    {
        VertexBinary vertex = new()
        {
            Id = reader.ReadInt32(),
            IsNull = reader.ReadBoolean(),
            Changed = reader.ReadBoolean()
        };

        if (vertex.Changed)
        {
            vertex.ChangedDat1 = reader.ReadInt32();
            vertex.ChangedDat2 = reader.ReadInt32();
        }
        return vertex;
    }

    public VertexBinary DeserializeVertexNew(BinaryReader reader)
    {
        VertexBinary vertex = new()
        {
            Id = reader.ReadInt32(),
            IsNull = reader.ReadBoolean(),
            Changed = reader.ReadBoolean(),
            NewDat1 = reader.ReadInt32(),
            NewDat2 = reader.ReadInt32(),
            NewDat3 = reader.ReadInt32()
        };
        return vertex;
    }

    public TriangleBinary DeserializeTriangle(BinaryReader reader)
    {
        TriangleBinary triangle = new()
        {
            V1 = reader.ReadInt32(),
            V2 = reader.ReadInt32(),
            V3 = reader.ReadInt32(),
            IsNull = reader.ReadBoolean()
        };
        return triangle;
    }

    public void SaveToFile(string filename, List<Vertex> vertices, List<Triangle> triangles)
    {
        BinaryWriter writer = new(File.Open(filename, FileMode.Create));

        writer.Write(vertices.Count);
        vertices.ForEach(vertex => 
        {
            Serialize(writer, vertex);
        });

        writer.Write(triangles.Count);
        triangles.ForEach(triangle => 
        {
            Serialize(writer, triangle);
        });

        writer.Close();
        writer.Dispose();
    }

    public void LoadFromFile(string fileName)
    {
        loadedVertices = new List<VertexBinary>();
        loadedTriangles = new List<TriangleBinary>();

        BinaryReader reader = new(File.Open(fileName, FileMode.Open));
        int totalVerts = reader.ReadInt32();

        for (int i = 0; i < totalVerts; i++)
        {
            loadedVertices.Add(DeserializeVertex(reader));
        }

        int totalTris = reader.ReadInt32();

        for (int i = 0; i < totalTris; i++)
        {
            loadedTriangles.Add(DeserializeTriangle(reader));
        }
        reader.Close();
        reader.Dispose();
    }

    public int LoadFromFile(string fileName, int currentVerts)
    {
        loadedVertices = new List<VertexBinary>();
        loadedTriangles = new List<TriangleBinary>();

        int totalVerts, totalTris, newVerts;
        BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
        
        totalVerts = reader.ReadInt32();
        newVerts = totalVerts - currentVerts;

        for (int i = 0; i < currentVerts; i++)
        {
            loadedVertices.Add(DeserializeVertex(reader));
        }

        for (int i = 0; i < newVerts; i++)
        {
            loadedVertices.Add(DeserializeVertexNew(reader));
        }

        totalTris = reader.ReadInt32();

        for (int i = 0; i < totalTris; i++)
        {
            loadedTriangles.Add(DeserializeTriangle(reader));
        }
        reader.Close();
        reader.Dispose();

        return newVerts;
    }
}