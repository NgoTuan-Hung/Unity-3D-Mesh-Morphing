
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TestScript : MonoBehaviour
{
    public RealMorph realMorph;
    public MyMeshStructure myMeshStructure;
    public List<GameObject> morphToGameObjects = new List<GameObject>();
    public GameObject morphToGameObject;
    public int currentMorphIndex = 0;

    private void Awake() 
    {
        realMorph = GetComponent<RealMorph>();
        myMeshStructure = GetComponent<MyMeshStructure>();
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            realMorph.StartMorphing(morphToGameObject);
            StartCoroutine(TrickSwapMesh());
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            realMorph.ResetMorphing();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            morphToGameObject = morphToGameObjects[++currentMorphIndex % morphToGameObjects.Count];
        }
    }

    IEnumerator TrickSwapMesh()
    {
        yield return new WaitForSeconds(2);

        myMeshStructure.TrickSwapMesh(realMorph.Vertices2AfterScalingAndRotating, realMorph.Uv2, realMorph.Triangles2, realMorph.targetMaterial);
    }
}