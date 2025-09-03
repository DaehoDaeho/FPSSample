// ------------------------------------------------------
// ����:
//   - Animator�� Speed(0~1)�� IsGrounded�� �� ������ ����.
//   - �̵��� CharacterController�� �ϰ�, �츮�� "���̴� �ӵ�"�� ���.
// ����:
//   - ���� ������Ʈ�� Animator, CharacterController ����.
//   - ServerAuthoritativeMotor�� moveSpeed ���� ����(������ �⺻ 4.0f).
using UnityEngine;

//[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(CharacterController))]
public class PlayerAnimationLocomotion : MonoBehaviour
{
    public ServerAuthoritativeMotor motor; // ����: ������ �⺻�ӵ� ���
    public float speedSmooth = 10.0f;      // ȭ��� �ӵ� ����

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
        // ���� �ӵ� ���
        Vector3 curPos = transform.position;
        Vector3 delta = curPos - prevPos;
        delta.y = 0.0f;

        float dt = Time.deltaTime;
        float speed = 0.0f;
        if (dt > 0.0001f)
        {
            speed = delta.magnitude / dt; // m/s
        }

        // 0~1 ����ȭ
        float norm = 0.0f;
        if (moveSpeedRef > 0.0f)
        {
            norm = Mathf.Clamp01(speed / moveSpeedRef);
        }

        // �ε巴��
        smoothedSpeed01 = Mathf.Lerp(smoothedSpeed01, norm, Time.deltaTime * speedSmooth);

        // Animator ����
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
