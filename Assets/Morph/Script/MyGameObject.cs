using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyGameObject : MonoBehaviour
{
    public MyGameInputAction myGameInputAction;
    public Animator animator;
    public Vector2 moveVector;
    public Vector2 prevMoveVector = Vector2.zero;
    public Vector3 direction = Vector3.zero;

    void Awake()
    {
        myGameInputAction = new MyGameInputAction();
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        moveVector = myGameInputAction.Default.Move.ReadValue<Vector2>();
        animator.SetFloat("Speed", moveVector.magnitude);
        direction.x = moveVector.x;
        direction.z = moveVector.y;
        transform.position += direction * Time.deltaTime;

        if (prevMoveVector != moveVector) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 360 * Time.fixedDeltaTime);
        prevMoveVector = moveVector;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        myGameInputAction.Default.Enable();
    }

    void OnDisable()
    {
        myGameInputAction.Default.Disable();
    }
}
