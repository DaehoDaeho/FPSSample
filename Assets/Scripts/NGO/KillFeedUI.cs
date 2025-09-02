// ------------------------------------------------------
// 역할:
//   - 화면 우측 상단 등 지정 위치에 "누가 누구를 처치"했는지 한 줄씩 쌓는다.
//   - 서버에서 ClientRpc로 라인이 오면 AddLine으로 추가.
// 사용법:
//   - HUD Canvas에 Text(또는 TMP_Text)를 "feedText"로 연결.
//   - GameModeServer가 ClientRpc로 호출하면 모든 클라에서 보인다.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillFeedUI : MonoBehaviour
{
    public TMP_Text feedText;          // 누적 텍스트 표시용.
    public int maxLines = 6;       // 최대 줄 수.
    private string[] lines;        // 순환 버퍼.
    private int lineCount = 0;

    void Awake()
    {
        lines = new string[maxLines];
        for (int i = 0; i < maxLines; i++)
        {
            lines[i] = "";
        }
        Refresh();
    }

    public void AddLine(string s)
    {
        // 가장 오래된 줄을 밀어내고, 마지막에 추가.
        for (int i = 0; i < maxLines - 1; i++)
        {
            lines[i] = lines[i + 1];
        }
        lines[maxLines - 1] = s;
        if (lineCount < maxLines)
        {
            lineCount = lineCount + 1;
        }
        Refresh();
    }

    private void Refresh()
    {
        if (feedText == null)
        {
            return;
        }
        string all = "";
        for (int i = 0; i < lineCount; i++)
        {
            all = all + lines[i] + "\n";
        }
        feedText.text = all;
    }
}
