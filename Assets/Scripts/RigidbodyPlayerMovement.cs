using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;  // �̵��ӵ�.
    public float mouseSensitivity = 100.0f; // ���콺 ����.
    public Transform cameraTransform;   // ī�޶� ��ü.

    private Rigidbody rb;
    private float xRotation = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;   // ���콺 Ŀ���� ��ٴ�.
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // ī�޶� �þ߸� ������ ���� ������ �����ϱ� ���� ó��.
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);    // ī�޶� ȸ�� ����.
        transform.Rotate(Vector3.up * mouseX);  // �¿� ȸ��.
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");  // Ű������ A, DŰ �Է� ��.
        float v = Input.GetAxis("Vertical");    // Ű������ W, SŰ �Է� ��.
        Vector3 move = transform.right * h + transform.forward * v; // ���� �̵� ó��.
        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.velocity.y; // �߷� ���� (�� ������ ����� ������ ������.)
        rb.velocity = velocity; // ���� �ӵ� ����.
    }
}
