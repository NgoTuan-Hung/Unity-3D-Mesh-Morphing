using Unity.VisualScripting;
using UnityEngine;

public class TestQEM : MonoBehaviour
{
    public MyMeshStructure myMeshStructure;
    public int targetFaceCount = 1000;
    bool pressed = false;
    private void FixedUpdate() 
    {
        if (Input.GetKey(KeyCode.K) && !pressed)
        {
            pressed = true;
            print("Pressed");
            myMeshStructure.GenerateMeshStructure(false);
            myMeshStructure.QuadricErrorInit();
            myMeshStructure.QuadricErrorStart(targetFaceCount);
        }

        if (Input.GetKey(KeyCode.R)) myMeshStructure.RevertMesh();
    }
}