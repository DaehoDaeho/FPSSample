// ------------------------------------------------------
// 목적:
//   - 오너의 마우스 Y로 PitchPivot만 회전한다(본체 Yaw는 절대 회전하지 않음).
//   - 오너의 Main Camera를 PitchPivot(또는 CameraMount)에 부착한다.
// 전제:
//   - Player 프리팹에 CameraMount, PitchPivot이 있어야 한다.
//   - 본체 Yaw는 ServerAuthoritativeMotor가 서버에서 계산한다.
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
