using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;  // 이동속도.
    public float mouseSensitivity = 100.0f; // 마우스 감도.
    public Transform cameraTransform;   // 카메라 객체.

    private Rigidbody rb;
    private float xRotation = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;   // 마우스 커서를 잠근다.
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // 카메라 시야를 지정한 범위 값으로 고정하기 위한 처리.
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);    // 카메라 회전 적용.
        transform.Rotate(Vector3.up * mouseX);  // 좌우 회전.
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");  // 키보드의 A, D키 입력 시.
        float v = Input.GetAxis("Vertical");    // 키보드의 W, S키 입력 시.
        Vector3 move = transform.right * h + transform.forward * v; // 실제 이동 처리.
        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.velocity.y; // 중력 유지 (땅 밖으로 벗어나면 밑으로 떨어짐.)
        rb.velocity = velocity; // 최종 속도 적용.
    }
}
