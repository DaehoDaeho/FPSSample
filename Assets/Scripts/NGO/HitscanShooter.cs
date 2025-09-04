// ------------------------------------------------------
// ����:
//   - Ŭ���̾�Ʈ(Owner)�� ��Ŭ���� �ϸ� ServerRpc�� "�߻� ��û"�� ������.
//   - ������ ShootPivot�� ��ġ/�������� Raycast�� ������ �¾Ҵ��� �����ϰ�, ������ ������ ����.
// ����Ʈ:
//   - Ŭ�� ��ٿ�(fireTimerClient) + ���� ��ٿ�(lastFireTimeServer) ���� ���� ��ġ.
//   - ģ�� ��� ���� �ɼ�(allowFriendlyFire == false�̸� ���� ���� ����).
// ����:
//   - Player �����տ� NetworkPlayerServerSetup(�� ����), NetworkHealth(�ǰ� ���) ����.
//   - ShootPivot�� PitchPivot�� �ڽ�(�ü� ����� ����)���� ��ġ�Ǿ� �־�� ��.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

public class HitscanShooter : NetworkBehaviour
{
    public Transform shootPivot;    // ���� ������(�ݵ�� PitchPivot �ڽ�)
    public float range = 60.0f;     // ��Ÿ�.
    public int damage = 25;         // �� �� ������.

    public float fireCooldown = 0.2f; // ���� ��ٿ�(��)
    private float fireTimerClient = 0.0f; // Ŭ�� ��ٿ�.

    public bool allowFriendlyFire = false; // ���� �� ���� ��� ����.

    private float lastFireTimeServer = -9999.0f; // ���� ��ٿ� Ÿ�ӽ�����.

    void Update()
    {
        // �Է��� Owner�� �޴´�(�� ĳ���͸� ����)
        if (IsOwner == false)
        {
            return;
        }

        // Ŭ�� ��ٿ� ����.
        fireTimerClient = fireTimerClient - Time.deltaTime;
        if (fireTimerClient < 0.0f)
        {
            fireTimerClient = 0.0f;
        }

        // ��Ŭ������ �߻� ��û.
        if (Input.GetMouseButtonDown(0) == true)
        {
            if (fireTimerClient <= 0.0f)
            {
                RequestFireServerRpc();   // �������� "�߻� ����" ��û.
                fireTimerClient = fireCooldown;

                PlayerAnimationRelay relay = GetComponentInChildren<PlayerAnimationRelay>();
                if (relay != null)
                {
                    relay.ServerPlayFire();
                }
            }
        }
    }

    // �� �޼���� "�������� ����"�ȴ�.
    [ServerRpc]
    private void RequestFireServerRpc(ServerRpcParams rpcParams = default)
    {
        // ���� ��ٿ� üũ(���� ����)
        if (Time.time < lastFireTimeServer + fireCooldown)
        {
            return;
        }
        lastFireTimeServer = Time.time;

        // ShootPivot�� ������ �߻� �Ұ�.
        if (shootPivot == null)
        {
            return;
        }

        Vector3 origin = shootPivot.position;
        Vector3 dir = shootPivot.forward;

        RaycastHit hit;
        bool hitSomething = Physics.Raycast(origin, dir, out hit, range, ~0, QueryTriggerInteraction.Ignore);

        if (hitSomething == true)
        {
            NetworkHealth targetHealth = hit.collider.GetComponentInParent<NetworkHealth>();
            if (targetHealth != null)
            {
                // ģ�� ��� ���� üũ.
                if (allowFriendlyFire == false)
                {
                    NetworkPlayerServerSetup mySetup = GetComponent<NetworkPlayerServerSetup>();
                    NetworkPlayerServerSetup otherSetup = targetHealth.GetComponent<NetworkPlayerServerSetup>();

                    if (mySetup != null)
                    {
                        if (otherSetup != null)
                        {
                            if (mySetup.team.Value == otherSetup.team.Value)
                            {
                                return;
                            }
                        }
                    }
                }

                // ������ ����(OwnerClientId)�� �����Ͽ� ������ ����.
                targetHealth.ApplyDamageServer(damage, OwnerClientId);
            }
        }
    }
}
