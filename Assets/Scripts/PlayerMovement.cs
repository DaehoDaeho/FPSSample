using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;    // 이동속도.
    public float mouseSensitivity = 2f; // 마우스 감도.

    public float recoilX = -8f;
    public float recoilY = 4f;
    public float recoilReturnSpeed = 8f;
    public float recoilSnappiness = 12f;

    private CharacterController controller; // 캐릭터 이동을 위한 컨트롤러 컴포넌트.
    private Transform cameraTransform;  // 메인 카메라 참조.
    private float verticalRotation = 0f;    // 상하 카메라 회전 값 누적 저장.
        
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
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);  // 카메라 시야를 지정한 범위 값으로 고정하기 위한 처리.
                
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation,
            0, 0f);
        
        float moveX = Input.GetAxis("Horizontal");  // 키보드의 A, D키 입력 시.
        float moveZ = Input.GetAxis("Vertical");    // 키보드의 W, S키 입력 시.

        Vector3 move = transform.right * moveX + transform.forward * moveZ; // 실제 이동 처리.
        controller.Move(move * speed * Time.deltaTime);
    }
}
