// ------------------------------------------------------
// ����:
//   - �� �÷��̾��� ų/����/ǥ���̸��� ������ �����ϰ�, ��ΰ� ���� �� �ֵ��� ����ȭ�Ѵ�.
//   - ǥ���̸��� ������ "P{ClientId}"�� ��ü�Ѵ�.
// ����:
//   - Player �����տ� ����.
//   - GameModeServer/Scoreboard/KillFeed�� �� ���� �д´�.
// C# ���� ����:
//   - NetworkVariable<T>: ��Ʈ��ũ���� �ڵ� ����ȭ�Ǵ� ����.
//     * ReadPermission: Everyone = ��� �б� ����
//     * WritePermission: Server  = ������ ���� ����(ġƮ ����)
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;
using Unity.Collections; // FixedString ���.

public class NetworkPlayerStats : NetworkBehaviour
{
    // Everyone �б� / Server ����.
    public NetworkVariable<int> kills =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    public NetworkVariable<int> deaths =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    // �̸��� ���̰� ���ѵ� FixedString ���(���ڿ� ����ȭ ���/���� ���)
    public NetworkVariable<FixedString64Bytes> displayName =
        new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone,
                                                   NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // ���������� �̸� �ʱ�ȭ. ���ٸ� "P{ClientId}"��.
        if (IsServer == true)
        {
            if (displayName.Value.Length == 0)
            {
                string autoName = "P" + OwnerClientId.ToString();
                displayName.Value = autoName;
            }
        }
    }

    // ���� ����: ų/���� ����.
    public void ServerAddKill()
    {
        if (IsServer == false)
        {
            return;
        }
        kills.Value = kills.Value + 1;
    }

    public void ServerAddDeath()
    {
        if (IsServer == false)
        {
            return;
        }
        deaths.Value = deaths.Value + 1;
    }

    // ���� ����: �̸� ����(�г��� �ý����� �ִٸ� ���⼭ ����)
    public void ServerSetDisplayName(string name)
    {
        if (IsServer == false)
        {
            return;
        }
        if (string.IsNullOrEmpty(name) == true)
        {
            return;
        }
        displayName.Value = name;
    }
}
