// ------------------------------------------------------
// 역할:
//   - 매 프레임 "보이는" 이동 속도(0~1)와 지면 상태를 계산해서 Animator에 넣는다.
//   - 이동은 CharacterController(루트)가 하고, 우리는 위치 변화로 속도를 추정한다.
// 전제:
//   - 이 스크립트는 MeshRoot(Animator가 있는 곳)에 붙인다.
//   - 루트의 CharacterController와 ServerAuthoritativeMotor를 부모에서 찾아 사용한다.
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationLocomotion : MonoBehaviour
{
    public ServerAuthoritativeMotor motor;   // 선택(없으면 자동으로 부모에서 찾음)
    public float speedSmooth = 10.0f;        // 속도 보간(시각용)

    private Animator anim;
    private CharacterController cc;          // 부모의 CC 참조.
    private Vector3 prevPos;
    private float smoothedSpeed01 = 0.0f;
    private float moveSpeedRef = 4.0f;       // motor.moveSpeed 참조.

    private float forceUngroundedTimer = 0.0f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        cc = GetComponentInParent<CharacterController>();
        prevPos = transform.position;
    }

    void Start()
    {
        if (motor == null)
        {
            motor = GetComponentInParent<ServerAuthoritativeMotor>();
        }
        if (motor != null)
        {
            moveSpeedRef = motor.moveSpeed;
        }
    }

    void Update()
    {
        // 수평 속도 계산(네트워크 보간된 트랜스폼 기준)
        Vector3 curPos = transform.position;
        Vector3 delta = curPos - prevPos;
        delta.y = 0.0f;

        float dt = Time.deltaTime;
        float speed = 0.0f;
        if (dt > 0.0001f)
        {
            speed = delta.magnitude / dt; // m/s
        }

        float norm = 0.0f;
        if (moveSpeedRef > 0.0f)
        {
            norm = Mathf.Clamp01(speed / moveSpeedRef);
        }

        smoothedSpeed01 = Mathf.Lerp(smoothedSpeed01, norm, Time.deltaTime * speedSmooth);

        if (anim != null)
        {
            anim.SetFloat("Speed", smoothedSpeed01);
            bool grounded = false;
            if (cc != null)
            {
                grounded = cc.isGrounded;

                if (forceUngroundedTimer > 0.0f)
                {
                    grounded = false;
                    forceUngroundedTimer = forceUngroundedTimer - Time.deltaTime;
                    if (forceUngroundedTimer < 0.0f)
                    {
                        forceUngroundedTimer = 0.0f;
                    }
                }
            }
            anim.SetBool("IsGrounded", grounded);
        }

        prevPos = curPos;
    }

    // Jump가 트리거되면 잠깐 "공중"으로 취급하고 싶을 때 호출
    public void OnJumpTriggeredLocal()
    {
        forceUngroundedTimer = 0.10f; // 0.1초 정도
    }
}
