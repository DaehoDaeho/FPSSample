// ------------------------------------------------------
// 변경점 요약:
//  1) 마우스 X를 "축 값(축 범위 -∞~∞)"으로 받고, 서버에서 "초당 각도"로 환산해 회전.
//  2) 프레임당 회전 최대치를 제한해(클램프) 과도 회전 스파이크 방지.
//  3) Host(서버+오너)도 입력이 즉시 서버에 반영되도록 동일 처리.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class ServerAuthoritativeMotor : NetworkBehaviour
{
    [Header("Move")]
    public float moveSpeed = 4.0f;
    public float gravity = 9.81f;
    public float jumpSpeed = 5.0f;

    [Header("Yaw (Mouse)")]
    public float yawDegreesPerSecond = 180.0f; // 초당 회전 각도(기본 180도/s)
    public float maxYawPerFrame = 10.0f;       // 한 프레임 최대 회전 각도(안전 클램프)

    private CharacterController controller;

    // 서버 입력 스냅샷.
    private float srvForwardAxis = 0.0f;  // -1 ~ 1
    private float srvStrafeAxis = 0.0f;  // -1 ~ 1
    private float srvYawAxis = 0.0f;  // 마우스 X 축 값(범위 제한 안함, 서버에서 각도 변환)
    private bool srvWantsJump = false;

    // 서버 물리 상태.
    private float verticalVelocity = 0.0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float fwd = 0.0f;
        float strafe = 0.0f;
        float yawAxis = 0.0f;
        bool jump = false;

        // 1) 오너만 입력을 만든다.
        if (IsOwner == true)
        {
            if (Input.GetKey(KeyCode.W) == true || Input.GetKey(KeyCode.UpArrow) == true)
            {
                fwd = 1.0f;
            }
            else
            {
                if (Input.GetKey(KeyCode.S) == true || Input.GetKey(KeyCode.DownArrow) == true)
                {
                    fwd = -1.0f;
                }
            }

            if (Input.GetKey(KeyCode.A) == true || Input.GetKey(KeyCode.LeftArrow) == true)
            {
                strafe = -1.0f;
            }
            else
            {
                if (Input.GetKey(KeyCode.D) == true || Input.GetKey(KeyCode.RightArrow) == true)
                {
                    strafe = 1.0f;
                }
            }

            // 마우스 X 축 값(프레임 델타가 섞여 있을 수 있으므로 "각도"는 서버에서 만들기)
            yawAxis = Input.GetAxis("Mouse X");

            if (Input.GetKeyDown(KeyCode.Space) == true)
            {
                jump = true;
            }
        }

        // 2) 입력 전달.
        if (IsOwner == true)
        {
            if (IsServer == true)
            {
                ApplyInputOnServer(fwd, strafe, yawAxis, jump);
            }
            else
            {
                SubmitInputServerRpc(fwd, strafe, yawAxis, jump);
            }
        }

        // 3) 서버 이동/회전 계산.
        if (IsServer == true)
        {
            DoServerMovement();
        }
    }

    private void ApplyInputOnServer(float forwardAxis, float strafeAxis, float yawAxis, bool wantsJump)
    {
        // 이동 축은 -1~1 범위로 제한.
        srvForwardAxis = Mathf.Clamp(forwardAxis, -1.0f, 1.0f);
        srvStrafeAxis = Mathf.Clamp(strafeAxis, -1.0f, 1.0f);

        // Yaw 축은 클램프하지 않고(마우스 튀는 값까지 포함), 각도 변환 단계에서 제한.
        srvYawAxis = yawAxis;

        if (wantsJump == true)
        {
            srvWantsJump = true;
        }
    }

    [ServerRpc]
    private void SubmitInputServerRpc(float forwardAxis, float strafeAxis, float yawAxis, bool wantsJump, ServerRpcParams rpcParams = default)
    {
        ApplyInputOnServer(forwardAxis, strafeAxis, yawAxis, wantsJump);
    }

    private void DoServerMovement()
    {
        if (controller == null)
        {
            return;
        }
        if (controller.enabled == false)
        {
            return;
        }

        // --- 회전(Yaw): "초당 각도" 기준 + 프레임당 최대치 제한 ---
        // 마우스 축값 * 초당 각도 * dt = 이번 프레임 회전 각도.
        float yawDeg = srvYawAxis * yawDegreesPerSecond * Time.deltaTime;

        // 프레임당 최대 회전 각도 제한(스파이크 방지)
        if (yawDeg > maxYawPerFrame)
        {
            yawDeg = maxYawPerFrame;
        }
        if (yawDeg < -maxYawPerFrame)
        {
            yawDeg = -maxYawPerFrame;
        }

        if (yawDeg != 0.0f)
        {
            transform.Rotate(0.0f, yawDeg, 0.0f);
        }

        // --- 수평 이동(대각선 정규화) ---
        Vector3 fwdDir = transform.forward;
        Vector3 rightDir = transform.right;
        Vector3 wish = (fwdDir * srvForwardAxis) + (rightDir * srvStrafeAxis);
        if (wish.sqrMagnitude > 1.0f)
        {
            wish = wish.normalized;
        }
        Vector3 horizontalVel = wish * moveSpeed;

        // --- 중력/점프 ---
        bool grounded = controller.isGrounded;
        if (grounded == true)
        {
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -0.5f;
            }
            if (srvWantsJump == true)
            {
                verticalVelocity = jumpSpeed;
            }
        }
        else
        {
            verticalVelocity = verticalVelocity - gravity * Time.deltaTime;
        }

        Vector3 velocity = new Vector3(horizontalVel.x, verticalVelocity, horizontalVel.z);
        Vector3 displacement = velocity * Time.deltaTime;
        controller.Move(displacement);

        if (srvWantsJump == true)
        {
            PlayerAnimationRelay relay = GetComponentInChildren<PlayerAnimationRelay>();
            if (relay != null)
            {
                relay.ServerPlayJump();
            }
        }

        // 점프 트리거는 1회성.
        srvWantsJump = false;
    }
}
