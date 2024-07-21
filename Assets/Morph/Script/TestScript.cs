
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
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            realMorph.StartMorphing();
        }

        // if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     realMorph.ResetMorphing();
        // }

        // if (Input.GetKeyDown(KeyCode.Alpha3))
        // {
        //     morphToGameObject = morphToGameObjects[++currentMorphIndex % morphToGameObjects.Count];
        // }
    }

    
}