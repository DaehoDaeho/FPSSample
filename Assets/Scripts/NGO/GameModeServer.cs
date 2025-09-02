// ------------------------------------------------------
// 역할:
//   - 서버 권한으로 매치 타이머/팀 점수 관리.
//   - 플레이어 처치 보고를 받아 팀 점수 갱신, K/D 갱신, Kill Feed 전파.
//   - 승리 조건: matchTimeSeconds가 0이 되면 점수 비교(간단 메시지).
// 필드 설명:
//   - teamAScore / teamBScore: 팀 점수(서버 쓰기, 모두 읽기).
//   - matchTimeSeconds: 남은 시간(서버 쓰기, 모두 읽기).
//   - allowFriendlyFire: 같은 팀 공격 허용 여부(참고; HitscanShooter와 일치 추천).
// C# 개념 설명:
//   - ClientRpc: 서버가 "모든 클라이언트"에서 실행시키는 함수(메시지/텍스트 전달 등).
// ------------------------------------------------------
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unity.Netcode.NetworkObject))]
public class GameModeServer : NetworkBehaviour
{
    [Header("Match Settings")]
    public float matchLengthSeconds = 180.0f; // 3분
    public bool allowFriendlyFire = false;

    [Header("Networked Score/Time")]
    public NetworkVariable<int> teamAScore =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);
    public NetworkVariable<int> teamBScore =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    public NetworkVariable<float> matchTimeSeconds =
        new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone,
                                      NetworkVariableWritePermission.Server);

    [Header("UI Targets (클라에서 채움)")]
    public KillFeedUI killFeedUI; // 클라 전용 참조(Find로 채움 가능)
    public ScoreboardUI scoreboardUI;

    private bool running = false;

    // 클라이언트 로컬 큐: UI가 아직 없을 때 받은 라인을 임시 보관.
    private Queue<string> pendingFeedLines = new Queue<string>();
    private bool uiBecameReady = false;

    void Start()
    {
        if (IsServer == true)
        {
            // 매치 시작 초기화.
            teamAScore.Value = 0;
            teamBScore.Value = 0;
            matchTimeSeconds.Value = matchLengthSeconds;
            running = true;
        }
    }

    void Update()
    {
        if (IsServer == true)
        {
            if (running == true)
            {
                matchTimeSeconds.Value = matchTimeSeconds.Value - Time.deltaTime;

                if (matchTimeSeconds.Value <= 0.0f)
                {
                    matchTimeSeconds.Value = 0.0f;
                    running = false;

                    // 간단 승부 결과 공지.
                    string result = GetResultText();
                    SendKillFeedClientRpc(result);
                }
            }
        }

        // 클라에서는 UI 레퍼런스를 느슨하게 채워도 됨(없으면 무시)
        if (IsClient == true)
        {
            if (killFeedUI == null)
            {
                killFeedUI = GameObject.FindObjectOfType<KillFeedUI>();
            }
            if (scoreboardUI == null)
            {
                scoreboardUI = GameObject.FindObjectOfType<ScoreboardUI>();
            }

            // UI가 이제 준비되었으면, 보관했던 라인을 한 번에 반영.
            if (killFeedUI != null)
            {
                if (uiBecameReady == false)
                {
                    uiBecameReady = true;
                    FlushPendingFeedLines();
                }
            }
        }
    }

    private void FlushPendingFeedLines()
    {
        if (killFeedUI == null)
        {
            return;
        }

        while (pendingFeedLines.Count > 0)
        {
            string ln = pendingFeedLines.Dequeue();
            killFeedUI.AddLine(ln);
        }
    }

    private string GetResultText()
    {
        if (teamAScore.Value > teamBScore.Value)
        {
            return "[RESULT] Team A Wins";
        }
        else
        {
            if (teamBScore.Value > teamAScore.Value)
            {
                return "[RESULT] Team B Wins";
            }
            else
            {
                return "[RESULT] Draw";
            }
        }
    }

    // 서버 전용: 킬 발생 보고(킬 한 사람, 죽은 사람의 ClientId로 식별)
    public void ServerReportKill(ulong killerClientId, ulong victimClientId)
    {
        if (IsServer == false)
        {
            return;
        }

        // K/D 갱신.
        NetworkPlayerStats killerStats = FindStatsByClientId(killerClientId);
        NetworkPlayerStats victimStats = FindStatsByClientId(victimClientId);

        if (killerStats != null)
        {
            killerStats.ServerAddKill();
        }
        if (victimStats != null)
        {
            victimStats.ServerAddDeath();
        }

        // 팀 점수 갱신(간단 규칙: "킬 한 팀" 점수 +1)
        int killerTeam = GetTeamByClientId(killerClientId);
        if (killerTeam == 0)
        {
            teamAScore.Value = teamAScore.Value + 1;
        }
        else
        {
            if (killerTeam == 1)
            {
                teamBScore.Value = teamBScore.Value + 1;
            }
        }

        // Kill Feed 전파.
        string killerName = GetDisplayNameByClientId(killerClientId);
        string victimName = GetDisplayNameByClientId(victimClientId);
        string line = killerName + "=>" + victimName;
        SendKillFeedClientRpc(line);
    }

    private int GetTeamByClientId(ulong clientId)
    {
        NetworkPlayerServerSetup[] all = GameObject.FindObjectsOfType<NetworkPlayerServerSetup>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null)
            {
                if (all[i].OwnerClientId == clientId)
                {
                    return all[i].team.Value;
                }
            }
        }
        return 0;
    }

    private NetworkPlayerStats FindStatsByClientId(ulong clientId)
    {
        NetworkPlayerStats[] all = GameObject.FindObjectsOfType<NetworkPlayerStats>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null)
            {
                if (all[i].OwnerClientId == clientId)
                {
                    return all[i];
                }
            }
        }
        return null;
    }

    private string GetDisplayNameByClientId(ulong clientId)
    {
        NetworkPlayerStats s = FindStatsByClientId(clientId);
        if (s != null)
        {
            if (s.displayName.Value.Length > 0)
            {
                return s.displayName.Value.ToString();
            }
        }
        return "P" + clientId.ToString();
    }

    // 모든 클라이언트의 KillFeedUI에 한 줄 추가.
    [ClientRpc]
    private void SendKillFeedClientRpc(string line, ClientRpcParams clientRpcParams = default)
    {
        if (killFeedUI != null)
        {
            killFeedUI.AddLine(line);
        }
    }
}
