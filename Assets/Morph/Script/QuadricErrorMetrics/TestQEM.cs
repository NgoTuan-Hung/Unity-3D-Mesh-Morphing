using Unity.VisualScripting;
using UnityEngine;

public class TestQEM : MonoBehaviour
{
    public MyMeshStructure myMeshStructure;

    private void FixedUpdate() 
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            myMeshStructure.GenerateMeshStructure(false);
            myMeshStructure.QuadricErrorInit();
        }
    }
}