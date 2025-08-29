// NetworkPlayerLook.cs
// Owner만 마우스 입력 처리. Yaw=본체, Pitch=피벗. 커서 잠금/해제 포함.
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerLook : NetworkBehaviour
{
    public Transform pitchPivot;   // CameraMount의 자식(상하 회전용)
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

        // 좌우(Yaw): 본체 회전
        if (mx != 0.0f)
        {
            float yawDelta = mx * mouseSensitivity;
            transform.Rotate(0.0f, yawDelta, 0.0f);
        }

        // 상하(Pitch): 피벗 회전(누적 + 제한)
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

        // ESC로 커서 토글
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
