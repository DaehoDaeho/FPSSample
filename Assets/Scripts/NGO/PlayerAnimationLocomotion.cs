// ------------------------------------------------------
// 역할:
//   - Animator에 Speed(0~1)와 IsGrounded를 매 프레임 갱신.
//   - 이동은 CharacterController가 하고, 우리는 "보이는 속도"만 계산.
// 전제:
//   - 같은 오브젝트에 Animator, CharacterController 존재.
//   - ServerAuthoritativeMotor의 moveSpeed 값을 참조(없으면 기본 4.0f).
using UnityEngine;

//[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(CharacterController))]
public class PlayerAnimationLocomotion : MonoBehaviour
{
    public ServerAuthoritativeMotor motor; // 선택: 없으면 기본속도 사용
    public float speedSmooth = 10.0f;      // 화면용 속도 보정

    public Animator anim;
    public CharacterController cc;

    private Vector3 prevPos;
    private float smoothedSpeed01 = 0.0f;
    private float moveSpeedRef = 4.0f;

    void Awake()
    {
        //anim = GetComponent<Animator>();
        //cc = GetComponent<CharacterController>();
        prevPos = transform.position;
    }

    void Start()
    {
        if (motor != null)
        {
            moveSpeedRef = motor.moveSpeed;
        }
    }

    void Update()
    {
        // 수평 속도 계산
        Vector3 curPos = transform.position;
        Vector3 delta = curPos - prevPos;
        delta.y = 0.0f;

        float dt = Time.deltaTime;
        float speed = 0.0f;
        if (dt > 0.0001f)
        {
            speed = delta.magnitude / dt; // m/s
        }

        // 0~1 정규화
        float norm = 0.0f;
        if (moveSpeedRef > 0.0f)
        {
            norm = Mathf.Clamp01(speed / moveSpeedRef);
        }

        // 부드럽게
        smoothedSpeed01 = Mathf.Lerp(smoothedSpeed01, norm, Time.deltaTime * speedSmooth);

        // Animator 적용
        //anim.SetFloat("Speed", smoothedSpeed01);
        bool move = false;
        if(speed != 0.0f)
        {
            move = true;
        }

        anim.SetBool("Move", move);

        //bool grounded = false;
        //if (cc != null)
        //{
        //    grounded = cc.isGrounded;
        //}
        //anim.SetBool("IsGrounded", grounded);

        prevPos = curPos;
    }
}
