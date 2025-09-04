// ------------------------------------------------------
// ����:
//   - �� ������ "���̴�" �̵� �ӵ�(0~1)�� ���� ���¸� ����ؼ� Animator�� �ִ´�.
//   - �̵��� CharacterController(��Ʈ)�� �ϰ�, �츮�� ��ġ ��ȭ�� �ӵ��� �����Ѵ�.
// ����:
//   - �� ��ũ��Ʈ�� MeshRoot(Animator�� �ִ� ��)�� ���δ�.
//   - ��Ʈ�� CharacterController�� ServerAuthoritativeMotor�� �θ𿡼� ã�� ����Ѵ�.
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationLocomotion : MonoBehaviour
{
    public ServerAuthoritativeMotor motor;   // ����(������ �ڵ����� �θ𿡼� ã��)
    public float speedSmooth = 10.0f;        // �ӵ� ����(�ð���)

    private Animator anim;
    private CharacterController cc;          // �θ��� CC ����.
    private Vector3 prevPos;
    private float smoothedSpeed01 = 0.0f;
    private float moveSpeedRef = 4.0f;       // motor.moveSpeed ����.

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
        // ���� �ӵ� ���(��Ʈ��ũ ������ Ʈ������ ����)
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
            }
            anim.SetBool("IsGrounded", grounded);
        }

        prevPos = curPos;
    }
}
