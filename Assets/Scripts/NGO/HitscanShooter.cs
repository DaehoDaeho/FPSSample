// ------------------------------------------------------
// 역할:
//   - 클라이언트(Owner)가 좌클릭을 하면 ServerRpc로 "발사 요청"을 보낸다.
//   - 서버는 ShootPivot의 위치/방향으로 Raycast를 수행해 맞았는지 판정하고, 맞으면 데미지 적용.
// 포인트:
//   - 클라 쿨다운(fireTimerClient) + 서버 쿨다운(lastFireTimeServer) 이중 안전 장치.
//   - 친선 사격 차단 옵션(allowFriendlyFire == false이면 같은 팀은 무시).
// 의존:
//   - Player 프리팹에 NetworkPlayerServerSetup(팀 정보), NetworkHealth(피격 대상) 존재.
//   - ShootPivot이 PitchPivot의 자식(시선 방향과 동일)으로 배치되어 있어야 함.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

public class HitscanShooter : NetworkBehaviour
{
    public Transform shootPivot;    // 레이 시작점(반드시 PitchPivot 자식)
    public float range = 60.0f;     // 사거리.
    public int damage = 25;         // 한 발 데미지.

    public float fireCooldown = 0.2f; // 연사 쿨다운(초)
    private float fireTimerClient = 0.0f; // 클라 쿨다운.

    public bool allowFriendlyFire = false; // 같은 팀 공격 허용 여부.

    private float lastFireTimeServer = -9999.0f; // 서버 쿨다운 타임스탬프.

    void Update()
    {
        // 입력은 Owner만 받는다(내 캐릭터만 조작)
        if (IsOwner == false)
        {
            return;
        }

        // 클라 쿨다운 감소.
        fireTimerClient = fireTimerClient - Time.deltaTime;
        if (fireTimerClient < 0.0f)
        {
            fireTimerClient = 0.0f;
        }

        // 좌클릭으로 발사 요청.
        if (Input.GetMouseButtonDown(0) == true)
        {
            if (fireTimerClient <= 0.0f)
            {
                RequestFireServerRpc();   // 서버에게 "발사 판정" 요청.
                fireTimerClient = fireCooldown;

                PlayerAnimationRelay relay = GetComponentInChildren<PlayerAnimationRelay>();
                if (relay != null)
                {
                    relay.ServerPlayFire();
                }
            }
        }
    }

    // 이 메서드는 "서버에서 실행"된다.
    [ServerRpc]
    private void RequestFireServerRpc(ServerRpcParams rpcParams = default)
    {
        // 서버 쿨다운 체크(스팸 방지)
        if (Time.time < lastFireTimeServer + fireCooldown)
        {
            return;
        }
        lastFireTimeServer = Time.time;

        // ShootPivot이 없으면 발사 불가.
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
                // 친선 사격 금지 체크.
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

                // 가해자 정보(OwnerClientId)를 포함하여 데미지 적용.
                targetHealth.ApplyDamageServer(damage, OwnerClientId);
            }
        }
    }
}
