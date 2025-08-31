// NetworkPlayerServerSetup.cs
// ------------------------------------------------------
// ����:
//   - ������ �� �÷��̾�� ���� ����(������ ���� ������ 0/1 ������)�Ѵ�.
//   - ������ ���� ���� ��ġ�� �÷��̾ ��ġ�Ѵ�.
//   - Owner(����)���Ը� ī�޶� �ٿ� 1��Ī �þ߸� �����.
// ����:
//   - ���� SpawnPointRegistry�� �־�� �Ѵ�.
//   - Player �����տ� CameraMount, PitchPivot�� �־�� �Ѵ�.
// ����:
//   - �� ������ NetworkVariable<int>�� ��� Ŭ�� �а�, ������ ����.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerServerSetup : NetworkBehaviour
{
    public Transform cameraMount;   // ī�޶� ���� ��ġ(�Ӹ�)
    public Transform pitchPivot;    // ���� ȸ�� �ǹ�(���콺 ���)

    // Everyone(������ �б�), Server(������ ����)
    public NetworkVariable<int> team =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // ������ �� ���� �� ���� ��ġ ����
        if (IsServer == true)
        {
            // ���� ��Ģ: ���� Ŭ���̾�Ʈ ID�� Ȧ¦�� ���� �� ����
            int assigned = 0;
            if (OwnerClientId % 2 == 1)
            {
                assigned = 1;
            }
            team.Value = assigned;

            // ���� ����Ʈ ã��
            SpawnPointRegistry reg = GameObject.FindObjectOfType<SpawnPointRegistry>();
            if (reg != null)
            {
                Vector3 pos = reg.GetNextSpawnPosition(team.Value);
                // CharacterController�� �ִٸ� �����̵� �� �浹�� ���ϱ� ����
                // ���/������ �������� controller.enabled�� �����Ѵ�(�Ʒ� ��ũ��Ʈ ����).
                transform.position = pos;
            }
        }

        // Owner(����)���Ը� ī�޶� ����
        if (IsOwner == true)
        {
            Camera cam = Camera.main;

            if (cam != null)
            {
                // PitchPivot�� ������ �ű⿡ �ٿ� ���� ȸ���� ī�޶� �ݿ��ǵ��� �Ѵ�.
                Transform target = pitchPivot;
                if (target == null)
                {
                    target = cameraMount;
                }

                if (target != null)
                {
                    cam.transform.SetParent(target);
                    // ���� ��ġ/ȸ���� 0���� ���� ��Ȯ�� ����
                    cam.transform.localPosition = Vector3.zero;
                    cam.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
