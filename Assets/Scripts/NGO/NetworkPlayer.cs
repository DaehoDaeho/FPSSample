// NetworkPlayer.cs
// ----------------------------------------------------
// ����:
//   - �� ������Ʈ�� ��Ʈ��ũ�� ������ ��(OnNetworkSpawn),
//     ���� ����(Owner)�� �÷��̾��� Main Camera�� CameraMount�� ���δ�.
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
