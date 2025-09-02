// NetworkHealth.cs
// ------------------------------------------------------
// ����:
//   - ������ ü��(hp)�� �����Ѵ�(������ ����). ��� Ŭ�� hp�� �ڵ����� �����޴´�.
//   - hp�� 0�� �Ǹ� ���� �ð� ���� "��� ����"�� �����, ���� ���� ����Ʈ�� �������Ѵ�.
// ����:
//   - �̵� �浹�� ���� ���� CharacterController�� ��� ���ٰ�(respawn ����) �ٽ� �Ҵ�.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class NetworkHealth : NetworkBehaviour
{
    public int maxHealth = 100;  // �ִ� ü��.
    public float respawnDelay = 3.0f; // ���������� ��� �ð�(��)

    // Everyone �б�, Server�� ����: ������ ���� �ٲٸ� ��ο��� �ڵ� �ݿ���.
    public NetworkVariable<int> hp =
        new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    private float respawnTimer = 0.0f;   // ������ ī��Ʈ�ٿ�.
    private bool waitingRespawn = false; // ��� ���� ����.
    private CharacterController controller;

    private ulong lastAttackerClientId = 0; // ���������� �������� �� �÷��̾�(������ ���)

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        // �������� ü���� �ʱ�ȭ.
        if (IsServer == true)
        {
            hp.Value = maxHealth;
        }
    }

    void Update()
    {
        // ������ ī��Ʈ�ٿ��� ���������� ����.
        if (IsServer == true)
        {
            if (waitingRespawn == true)
            {
                respawnTimer = respawnTimer - Time.deltaTime;

                if (respawnTimer <= 0.0f)
                {
                    DoRespawn();
                }
            }
        }
    }

    // ���������� ȣ���ؾ� ��: ������ ����.
    public void ApplyDamageServer(int amount)
    {
        if (IsServer == false)
        {
            return;
        }

        hp.Value = hp.Value - amount;

        if (hp.Value <= 0)
        {
            hp.Value = 0;
            StartRespawnCountdown();
        }
    }

    // ���� ApplyDamageServer(int) ��� "������ ���� ����" �����ε带 �߰�.
    public void ApplyDamageServer(int amount, ulong attackerClientId)
    {
        if (IsServer == false)
        {
            return;
        }

        lastAttackerClientId = attackerClientId;

        hp.Value = hp.Value - amount;

        if (hp.Value <= 0)
        {
            hp.Value = 0;
            // ��� ó�� ������ GameMode�� "ų �߻�" ����.
            GameModeServer gm = GameObject.FindObjectOfType<GameModeServer>();
            if (gm != null)
            {
                // �ڱ� �ڽ��� �׿��ٸ�(���� ��) ������/�����ڰ� ���� �� ���� -> �״�� ����.
                gm.ServerReportKill(lastAttackerClientId, OwnerClientId);
            }

            StartRespawnCountdown();
        }
    }

    private void StartRespawnCountdown()
    {
        waitingRespawn = true;
        respawnTimer = respawnDelay;

        // ��� ���¿����� �̵�/�浹�� ���� ���� ��Ʈ�ѷ� ��Ȱ��ȭ.
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    private void DoRespawn()
    {
        waitingRespawn = false;

        // �� ���� ����Ʈ�� �̵�.
        NetworkPlayerServerSetup setup = GetComponent<NetworkPlayerServerSetup>();
        SpawnPointRegistry reg = GameObject.FindObjectOfType<SpawnPointRegistry>();

        if (setup != null)
        {
            if (reg != null)
            {
                Vector3 pos = reg.GetNextSpawnPosition(setup.team.Value);
                // ��Ʈ�ѷ��� ���� ���� �� ��ġ�� �ű�� �浹�� �� �߻�.
                transform.position = pos;
            }
        }

        // ü�� ȸ��.
        hp.Value = maxHealth;

        // �̵� �ٽ� ���.
        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}
