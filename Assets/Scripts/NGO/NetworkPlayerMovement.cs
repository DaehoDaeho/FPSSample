// NetworkPlayerMovement.cs
// ----------------------------------------------------
// 역할:
//   - Owner인 경우에만 입력(WASD, Space)을 읽어 CharacterController로 이동한다.
//   - NetworkTransform이 포지션/회전 변경을 다른 클라이언트로 복제해 준다.
// 필요 컴포넌트:
//   - CharacterController (Player_Network에 부착)
//   - NetworkTransform (Player_Network에 부착)
// ----------------------------------------------------

using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 4.0f;      // 평면 이동 속도
    public float rotateSpeed = 150.0f;  // 좌우 회전 속도(마우스 X 또는 키)
    public float gravity = 9.81f;       // 중력 가속도
    public float jumpSpeed = 5.0f;      // 점프 초기 속도

    private CharacterController controller;
    private float verticalVelocity = 0.0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Owner가 아니면 입력을 처리하지 않는다.
        if (IsOwner == false)
        {
            return;
        }

        if (controller == null)
        {
            return;
        }

        // ---- 회전 입력(좌우) ----
        float yawInput = 0.0f;
        // 좌우 화살표 또는 A/D 로 회전(초보자용: 마우스 회전은 다음 시간에)
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

        // ---- 평면 이동 입력(W/S) ----
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

        Vector3 forwardDir = transform.forward;
        Vector3 horizontalMove = forwardDir * (forwardInput * moveSpeed);

        // ---- 중력/점프 ----
        bool isGrounded = controller.isGrounded;

        if (isGrounded == true)
        {
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -0.5f; // 지면에 붙여두기(떨림 방지)
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
        // 결과 위치/회전은 NetworkTransform이 알아서 동기화한다.
    }
}
