using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float rotateAngle = 1f;
    private void FixedUpdate() 
    {
        RotateYAnglePerFixedDeltaTime(rotateAngle);
    }

    public void RotateYAnglePerFixedDeltaTime(float angle)
    {
        transform.Rotate(0, angle * Time.fixedDeltaTime, 0);
    }
}
