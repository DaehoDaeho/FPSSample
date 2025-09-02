// ------------------------------------------------------
// ����:
//   - ���� �������� ��ġ Ÿ�̸�/�� ���� ����.
//   - �÷��̾� óġ ���� �޾� �� ���� ����, K/D ����, Kill Feed ����.
//   - �¸� ����: matchTimeSeconds�� 0�� �Ǹ� ���� ��(���� �޽���).
// �ʵ� ����:
//   - teamAScore / teamBScore: �� ����(���� ����, ��� �б�).
//   - matchTimeSeconds: ���� �ð�(���� ����, ��� �б�).
//   - allowFriendlyFire: ���� �� ���� ��� ����(����; HitscanShooter�� ��ġ ��õ).
// C# ���� ����:
//   - ClientRpc: ������ "��� Ŭ���̾�Ʈ"���� �����Ű�� �Լ�(�޽���/�ؽ�Ʈ ���� ��).
// ------------------------------------------------------
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unity.Netcode.NetworkObject))]
public class GameModeServer : NetworkBehaviour
{
    [Header("Match Settings")]
    public float matchLengthSeconds = 180.0f; // 3��
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

    [Header("UI Targets (Ŭ�󿡼� ä��)")]
    public KillFeedUI killFeedUI; // Ŭ�� ���� ����(Find�� ä�� ����)
    public ScoreboardUI scoreboardUI;

    private bool running = false;

    // Ŭ���̾�Ʈ ���� ť: UI�� ���� ���� �� ���� ������ �ӽ� ����.
    private Queue<string> pendingFeedLines = new Queue<string>();
    private bool uiBecameReady = false;

    void Start()
    {
        if (IsServer == true)
        {
            // ��ġ ���� �ʱ�ȭ.
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

                    // ���� �º� ��� ����.
                    string result = GetResultText();
                    SendKillFeedClientRpc(result);
                }
            }
        }

        // Ŭ�󿡼��� UI ���۷����� �����ϰ� ä���� ��(������ ����)
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

            // UI�� ���� �غ�Ǿ�����, �����ߴ� ������ �� ���� �ݿ�.
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

    // ���� ����: ų �߻� ����(ų �� ���, ���� ����� ClientId�� �ĺ�)
    public void ServerReportKill(ulong killerClientId, ulong victimClientId)
    {
        if (IsServer == false)
        {
            return;
        }

        // K/D ����.
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

        // �� ���� ����(���� ��Ģ: "ų �� ��" ���� +1)
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

        // Kill Feed ����.
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

    // ��� Ŭ���̾�Ʈ�� KillFeedUI�� �� �� �߰�.
    [ClientRpc]
    private void SendKillFeedClientRpc(string line, ClientRpcParams clientRpcParams = default)
    {
        if (killFeedUI != null)
        {
            killFeedUI.AddLine(line);
        }
    }
}
