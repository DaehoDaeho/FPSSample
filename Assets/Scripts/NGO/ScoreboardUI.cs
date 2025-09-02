// ------------------------------------------------------
// ����:
//   - Tab Ű�� ������ ���� Scoreboard Canvas�� ���̰� �ϰ�, �÷��̾� K/D/��/�� ������ �����Ѵ�.
//   - ����/Ŭ�� ������� "���� ���� ������Ʈ"�� �о� ǥ��(��ΰ� ���� �� ����).
// ����:
//   - Scoreboard Canvas�� �� ��ũ��Ʈ�� �����ϰ�, Text�� scoreboardText�� ����.
//   - Canvas�� ������ �� ��Ȱ��(Inspector)�� �ΰ�, ��ũ��Ʈ�� ����Ѵ�.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ScoreboardUI : MonoBehaviour
{
    public GameObject panel;      // Scoreboard Canvas�� ��Ʈ ������Ʈ.
    public TMP_Text scoreboardText;   // ���� ǥ�ÿ� Text.

    private GameModeServer gm;    // �� ����/Ÿ�̸� �б�.

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    void Update()
    {
        // Tab Ű ������ ���ȸ� ǥ��.
        bool show = false;
        if (Input.GetKey(KeyCode.Tab) == true)
        {
            show = true;
        }

        if (panel != null)
        {
            panel.SetActive(show);
        }

        if (show == true)
        {
            RefreshNow();
        }
    }

    private void RefreshNow()
    {
        if (gm == null)
        {
            gm = GameObject.FindObjectOfType<GameModeServer>();
        }

        int aScore = 0;
        int bScore = 0;
        float timeLeft = 0.0f;

        if (gm != null)
        {
            aScore = gm.teamAScore.Value;
            bScore = gm.teamBScore.Value;
            timeLeft = gm.matchTimeSeconds.Value;
        }

        // �÷��̾� ��� ����.
        NetworkPlayerStats[] all = GameObject.FindObjectsOfType<NetworkPlayerStats>();

        // ǥ�� ���ڿ� ����.
        string s = "";
        s = s + "TIME: " + timeLeft.ToString("F0") + "s\n";
        s = s + "TEAM A: " + aScore.ToString() + "   TEAM B: " + bScore.ToString() + "\n";
        s = s + "--------------------------------------\n";
        s = s + "Player           Team   K   D\n";

        for (int i = 0; i < all.Length; i++)
        {
            NetworkPlayerStats st = all[i];
            if (st != null)
            {
                string name = "P" + st.OwnerClientId.ToString();
                if (st.displayName.Value.Length > 0)
                {
                    name = st.displayName.Value.ToString();
                }

                int team = 0;
                NetworkPlayerServerSetup setup = st.GetComponent<NetworkPlayerServerSetup>();
                if (setup != null)
                {
                    team = setup.team.Value;
                }

                string line = name.PadRight(15);
                line = line + "   " + (team == 0 ? "A" : "B");
                line = line + "    " + st.kills.Value.ToString().PadLeft(2);
                line = line + "  " + st.deaths.Value.ToString().PadLeft(2);
                s = s + line + "\n";
            }
        }

        if (scoreboardText != null)
        {
            scoreboardText.text = s;
        }
    }
}
