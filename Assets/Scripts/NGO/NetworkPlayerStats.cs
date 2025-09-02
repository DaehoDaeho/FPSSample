// ------------------------------------------------------
// 역할:
//   - 각 플레이어의 킬/데스/표시이름을 서버가 소유하고, 모두가 읽을 수 있도록 동기화한다.
//   - 표시이름은 없으면 "P{ClientId}"로 대체한다.
// 사용법:
//   - Player 프리팹에 부착.
//   - GameModeServer/Scoreboard/KillFeed가 이 값을 읽는다.
// C# 개념 설명:
//   - NetworkVariable<T>: 네트워크에서 자동 동기화되는 변수.
//     * ReadPermission: Everyone = 모두 읽기 가능
//     * WritePermission: Server  = 서버만 쓰기 가능(치트 방지)
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;
using Unity.Collections; // FixedString 사용.

public class NetworkPlayerStats : NetworkBehaviour
{
    // Everyone 읽기 / Server 쓰기.
    public NetworkVariable<int> kills =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    public NetworkVariable<int> deaths =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    // 이름은 길이가 제한된 FixedString 사용(문자열 동기화 비용/성능 고려)
    public NetworkVariable<FixedString64Bytes> displayName =
        new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone,
                                                   NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // 서버에서만 이름 초기화. 없다면 "P{ClientId}"로.
        if (IsServer == true)
        {
            if (displayName.Value.Length == 0)
            {
                string autoName = "P" + OwnerClientId.ToString();
                displayName.Value = autoName;
            }
        }
    }

    // 서버 전용: 킬/데스 증가.
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

    // 서버 전용: 이름 설정(닉네임 시스템이 있다면 여기서 세팅)
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
