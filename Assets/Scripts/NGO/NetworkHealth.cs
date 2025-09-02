// NetworkHealth.cs
// ------------------------------------------------------
// 역할:
//   - 서버가 체력(hp)을 소유한다(서버만 쓰기). 모든 클라가 hp를 자동으로 공유받는다.
//   - hp가 0이 되면 일정 시간 동안 "사망 상태"로 만들고, 이후 스폰 포인트로 리스폰한다.
// 주의:
//   - 이동 충돌을 막기 위해 CharacterController를 잠시 껐다가(respawn 전후) 다시 켠다.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class NetworkHealth : NetworkBehaviour
{
    public int maxHealth = 100;  // 최대 체력.
    public float respawnDelay = 3.0f; // 리스폰까지 대기 시간(초)

    // Everyone 읽기, Server만 쓰기: 서버가 값을 바꾸면 모두에게 자동 반영됨.
    public NetworkVariable<int> hp =
        new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    private float respawnTimer = 0.0f;   // 리스폰 카운트다운.
    private bool waitingRespawn = false; // 사망 상태 여부.
    private CharacterController controller;

    private ulong lastAttackerClientId = 0; // 마지막으로 데미지를 준 플레이어(서버만 사용)

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        // 서버에서 체력을 초기화.
        if (IsServer == true)
        {
            hp.Value = maxHealth;
        }
    }

    void Update()
    {
        // 리스폰 카운트다운은 서버에서만 진행.
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

    // 서버에서만 호출해야 함: 데미지 적용.
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

    // 기존 ApplyDamageServer(int) 대신 "가해자 정보 포함" 오버로드를 추가.
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
            // 사망 처리 직전에 GameMode에 "킬 발생" 보고.
            GameModeServer gm = GameObject.FindObjectOfType<GameModeServer>();
            if (gm != null)
            {
                // 자기 자신을 죽였다면(낙사 등) 가해자/피해자가 같을 수 있음 -> 그대로 보고.
                gm.ServerReportKill(lastAttackerClientId, OwnerClientId);
            }

            StartRespawnCountdown();
        }
    }

    private void StartRespawnCountdown()
    {
        waitingRespawn = true;
        respawnTimer = respawnDelay;

        // 사망 상태에서는 이동/충돌을 막기 위해 컨트롤러 비활성화.
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    private void DoRespawn()
    {
        waitingRespawn = false;

        // 팀 스폰 포인트로 이동.
        NetworkPlayerServerSetup setup = GetComponent<NetworkPlayerServerSetup>();
        SpawnPointRegistry reg = GameObject.FindObjectOfType<SpawnPointRegistry>();

        if (setup != null)
        {
            if (reg != null)
            {
                Vector3 pos = reg.GetNextSpawnPosition(setup.team.Value);
                // 컨트롤러가 꺼져 있을 때 위치를 옮기면 충돌이 덜 발생.
                transform.position = pos;
            }
        }

        // 체력 회복.
        hp.Value = maxHealth;

        // 이동 다시 허용.
        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}
