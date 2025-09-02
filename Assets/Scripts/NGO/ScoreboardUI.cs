// ------------------------------------------------------
// 역할:
//   - Tab 키를 누르는 동안 Scoreboard Canvas를 보이게 하고, 플레이어 K/D/팀/팀 점수를 갱신한다.
//   - 서버/클라 관계없이 "현재 씬의 컴포넌트"를 읽어 표시(모두가 같은 값 공유).
// 사용법:
//   - Scoreboard Canvas에 이 스크립트를 부착하고, Text를 scoreboardText로 연결.
//   - Canvas는 시작할 때 비활성(Inspector)로 두고, 스크립트가 토글한다.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ScoreboardUI : MonoBehaviour
{
    public GameObject panel;      // Scoreboard Canvas의 루트 오브젝트.
    public TMP_Text scoreboardText;   // 내용 표시용 Text.

    private GameModeServer gm;    // 팀 점수/타이머 읽기.

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    void Update()
    {
        // Tab 키 누르는 동안만 표시.
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

        // 플레이어 목록 수집.
        NetworkPlayerStats[] all = GameObject.FindObjectsOfType<NetworkPlayerStats>();

        // 표시 문자열 구성.
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
