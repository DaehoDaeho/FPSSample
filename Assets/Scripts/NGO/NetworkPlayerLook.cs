// ------------------------------------------------------
// ����:
//   - ������ ���콺 Y�� PitchPivot�� ȸ���Ѵ�(��ü Yaw�� ���� ȸ������ ����).
//   - ������ Main Camera�� PitchPivot(�Ǵ� CameraMount)�� �����Ѵ�.
// ����:
//   - Player �����տ� CameraMount, PitchPivot�� �־�� �Ѵ�.
//   - ��ü Yaw�� ServerAuthoritativeMotor�� �������� ����Ѵ�.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerLook : NetworkBehaviour
{
    public Transform cameraMount;
    public Transform pitchPivot;
    public float mouseSensitivity = 1.5f;
    public float minPitch = -70.0f;
    public float maxPitch = 70.0f;

    private float pitch = 0.0f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner == true)
        {
            AttachCamera();
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

        float my = Input.GetAxis("Mouse Y");

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

    private void AttachCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Transform target = pitchPivot;
        if (target == null)
        {
            target = cameraMount;
        }

        if (target != null)
        {
            cam.tag = "MainCamera";
            cam.transform.SetParent(target);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }
    }
}
