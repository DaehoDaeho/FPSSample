using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;    // �̵��ӵ�.
    public float mouseSensitivity = 2f; // ���콺 ����.

    public float recoilX = -8f;
    public float recoilY = 4f;
    public float recoilReturnSpeed = 8f;
    public float recoilSnappiness = 12f;

    private CharacterController controller; // ĳ���� �̵��� ���� ��Ʈ�ѷ� ������Ʈ.
    private Transform cameraTransform;  // ���� ī�޶� ����.
    private float verticalRotation = 0f;    // ���� ī�޶� ȸ�� �� ���� ����.
        
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);  // ī�޶� �þ߸� ������ ���� ������ �����ϱ� ���� ó��.
                
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation,
            0, 0f);
        
        float moveX = Input.GetAxis("Horizontal");  // Ű������ A, DŰ �Է� ��.
        float moveZ = Input.GetAxis("Vertical");    // Ű������ W, SŰ �Է� ��.

        Vector3 move = transform.right * moveX + transform.forward * moveZ; // ���� �̵� ó��.
        controller.Move(move * speed * Time.deltaTime);
    }
}
