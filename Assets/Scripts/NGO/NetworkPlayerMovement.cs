// NetworkPlayerMovement.cs
// ----------------------------------------------------
// ����:
//   - Owner�� ��쿡�� �Է�(WASD, Space)�� �о� CharacterController�� �̵��Ѵ�.
//   - NetworkTransform�� ������/ȸ�� ������ �ٸ� Ŭ���̾�Ʈ�� ������ �ش�.
// �ʿ� ������Ʈ:
//   - CharacterController (Player_Network�� ����)
//   - NetworkTransform (Player_Network�� ����)
// ----------------------------------------------------

using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 4.0f;      // ��� �̵� �ӵ�
    public float rotateSpeed = 150.0f;  // �¿� ȸ�� �ӵ�(���콺 X �Ǵ� Ű)
    public float gravity = 9.81f;       // �߷� ���ӵ�
    public float jumpSpeed = 5.0f;      // ���� �ʱ� �ӵ�

    private CharacterController controller;
    private float verticalVelocity = 0.0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Owner�� �ƴϸ� �Է��� ó������ �ʴ´�.
        if (IsOwner == false)
        {
            return;
        }

        if (controller == null)
        {
            return;
        }

        // ---- ȸ�� �Է�(�¿�) ----
        float yawInput = 0.0f;
        // �¿� ȭ��ǥ �Ǵ� A/D �� ȸ��(�ʺ��ڿ�: ���콺 ȸ���� ���� �ð���)
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

        // ---- ��� �̵� �Է�(W/S) ----
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

        // ---- �߷�/���� ----
        bool isGrounded = controller.isGrounded;

        if (isGrounded == true)
        {
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -0.5f; // ���鿡 �ٿ��α�(���� ����)
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
        // ��� ��ġ/ȸ���� NetworkTransform�� �˾Ƽ� ����ȭ�Ѵ�.
    }
}
