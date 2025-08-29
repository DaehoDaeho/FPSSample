// NetworkPlayerLook.cs
// Owner�� ���콺 �Է� ó��. Yaw=��ü, Pitch=�ǹ�. Ŀ�� ���/���� ����.
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerLook : NetworkBehaviour
{
    public Transform pitchPivot;   // CameraMount�� �ڽ�(���� ȸ����)
    public float mouseSensitivity = 1.5f;
    public float minPitch = -70.0f;
    public float maxPitch = 70.0f;

    private float pitch = 0.0f;

    void Start()
    {
        if (IsOwner == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (IsOwner == false)
        {
            return;
        }
        if (pitchPivot == null)
        {
            return;
        }

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        // �¿�(Yaw): ��ü ȸ��
        if (mx != 0.0f)
        {
            float yawDelta = mx * mouseSensitivity;
            transform.Rotate(0.0f, yawDelta, 0.0f);
        }

        // ����(Pitch): �ǹ� ȸ��(���� + ����)
        if (my != 0.0f)
        {
            pitch = pitch - my * mouseSensitivity;
            if (pitch < minPitch)
            {
                pitch = minPitch;
            }
            if (pitch > maxPitch)
            {
                pitch = maxPitch;
            }
            pitchPivot.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        }

        // ESC�� Ŀ�� ���
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
