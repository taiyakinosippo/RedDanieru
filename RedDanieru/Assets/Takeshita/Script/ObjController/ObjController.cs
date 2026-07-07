using UnityEngine;

public class ObjController: MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); //A,D
        float vertical = Input.GetAxisRaw("Vertical"); //W,S

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        rb.MovePosition(
            rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime
            );
    }
}
