using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform cameraMount;   // �Ӹ� ��ġ
    public Transform pitchPivot;    // ���� ȸ����(ī�޶��� �θ� ��)
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
                        target = pitchPivot;  // �� �ٽ�: PitchPivot�� ���δ�
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
