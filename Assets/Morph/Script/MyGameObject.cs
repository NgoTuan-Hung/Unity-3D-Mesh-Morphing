using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyGameObject : MonoBehaviour
{
    public MyGameInputAction myGameInputAction;
    public MyMeshStructure myMeshStructure;
    public RealMorph realMorph;
    public Animator animator;
    public Vector2 moveVector;
    public Vector3 direction = Vector3.zero;
    public bool isPlayable = false;

    void Awake()
    {
        myMeshStructure = GetComponentInChildren<MyMeshStructure>();
        myGameInputAction = new MyGameInputAction();
        myGameInputAction.Default.Attack.performed += Attack;
        myGameInputAction.Default.Morph.performed += Morph;
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (isPlayable) Move();
    }

    void Move()
    {
        moveVector = myGameInputAction.Default.Move.ReadValue<Vector2>();
        animator.SetFloat("Speed", moveVector.magnitude);
        direction.x = moveVector.x;
        direction.z = moveVector.y;
        transform.position += direction * Time.deltaTime;

        if (moveVector != Vector2.zero) transform.rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, direction, Vector3.up), 0);
    }

    void Attack(InputAction.CallbackContext context)
    {
        if (isPlayable) animator.SetBool("Attack", true);
    }

    void StopAttack()
    {
        animator.SetBool("Attack", false);
    }

    void Morph(InputAction.CallbackContext context)
    {
        if (isPlayable) realMorph.StartMorphing();
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
