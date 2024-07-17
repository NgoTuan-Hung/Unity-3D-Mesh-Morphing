using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshTest : MonoBehaviour
{
    public SkinnedMeshRenderer[] skinnedMeshRenderers;
    public Material[] materials;
    public GameObject test;
    // Start is called before the first frame update
    void Start()
    {
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        materials = new Material[skinnedMeshRenderers.Length];
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            materials[i] = skinnedMeshRenderers[i].material;
        }
        AddMeshToGameObject();
    }

    public void AddMeshToGameObject()
    {
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            GameObject part = Instantiate(new GameObject(), test.transform);
            MeshFilter meshFilter = part.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
            meshFilter.mesh = skinnedMeshRenderers[i].sharedMesh;
            meshRenderer.material = materials[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
