// NetworkPlayerBasicMovement.cs
// Owner만 입력 처리. WASD/화살표 전후 이동 + 점프 + 중력.
// 회전은 좌우 키로 간단 처리(마우스 룩은 B단계에서 추가).
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerBasicMovement : NetworkBehaviour
{
    public float moveSpeed = 4.0f;
    public float rotateSpeed = 150.0f;
    public float gravity = 9.81f;
    public float jumpSpeed = 5.0f;

    private CharacterController controller;
    private float verticalVelocity = 0.0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (IsOwner == false)
        {
            return;
        }
        if (controller == null)
        {
            return;
        }

        // 좌우(Yaw) 회전: 화살표 또는 A/D
        float yawInput = 0.0f;
        if (Input.GetKey(KeyCode.LeftArrow) == true || Input.GetKey(KeyCode.A) == true)
        {
            yawInput = -1.0f;
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow) == true || Input.GetKey(KeyCode.D) == true)
            {
                yawInput = 1.0f;
            }
        }
        if (yawInput != 0.0f)
        {
            float yawDelta = yawInput * rotateSpeed * Time.deltaTime;
            transform.Rotate(0.0f, yawDelta, 0.0f);
        }

        // 전후 이동: W/S 또는 ↑/↓
        float forwardInput = 0.0f;
        if (Input.GetKey(KeyCode.W) == true || Input.GetKey(KeyCode.UpArrow) == true)
        {
            forwardInput = 1.0f;
        }
        else
        {
            if (Input.GetKey(KeyCode.S) == true || Input.GetKey(KeyCode.DownArrow) == true)
            {
                forwardInput = -1.0f;
            }
        }
        Vector3 forward = transform.forward;
        Vector3 horizontalMove = forward * (forwardInput * moveSpeed);

        // 중력 + 점프
        bool grounded = controller.isGrounded;
        if (grounded == true)
        {
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -0.5f;
            }
            if (Input.GetKeyDown(KeyCode.Space) == true)
            {
                verticalVelocity = jumpSpeed;
            }
        }
        else
        {
            verticalVelocity = verticalVelocity - gravity * Time.deltaTime;
        }

        Vector3 velocity = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);
        Vector3 displacement = velocity * Time.deltaTime;
        controller.Move(displacement);
    }
}
