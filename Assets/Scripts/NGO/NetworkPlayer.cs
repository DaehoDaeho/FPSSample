// NetworkPlayer.cs
// ----------------------------------------------------
// 역할:
//   - 이 오브젝트가 네트워크로 스폰될 때(OnNetworkSpawn),
//     내가 소유(Owner)한 플레이어라면 Main Camera를 CameraMount에 붙인다.
// ----------------------------------------------------

using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform cameraMount;
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
                    if (cameraMount != null)
                    {
                        cam.transform.SetParent(cameraMount);
                        cam.transform.localPosition = Vector3.zero;
                        cam.transform.localRotation = Quaternion.identity;
                    }
                }
            }
        }
    }
}
