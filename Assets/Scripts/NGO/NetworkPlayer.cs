using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform cameraMount;   // 머리 위치
    public Transform pitchPivot;    // 상하 회전용(카메라의 부모가 됨)
    public bool attachCameraOnOwner = true;

    public override void OnNetworkSpawn()
    {
        if (IsOwner == true)
        {
            if (attachCameraOnOwner == true)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Transform target = cameraMount;

                    if (pitchPivot != null)
                    {
                        target = pitchPivot;  // ★ 핵심: PitchPivot에 붙인다
                    }

                    if (target != null)
                    {
                        cam.transform.SetParent(target);
                        cam.transform.localPosition = Vector3.zero;
                        cam.transform.localRotation = Quaternion.identity;
                    }
                }
            }
        }
    }
}
