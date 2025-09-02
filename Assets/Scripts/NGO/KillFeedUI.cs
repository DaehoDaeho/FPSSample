// ------------------------------------------------------
// ����:
//   - ȭ�� ���� ��� �� ���� ��ġ�� "���� ������ óġ"�ߴ��� �� �پ� �״´�.
//   - �������� ClientRpc�� ������ ���� AddLine���� �߰�.
// ����:
//   - HUD Canvas�� Text(�Ǵ� TMP_Text)�� "feedText"�� ����.
//   - GameModeServer�� ClientRpc�� ȣ���ϸ� ��� Ŭ�󿡼� ���δ�.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillFeedUI : MonoBehaviour
{
    public TMP_Text feedText;          // ���� �ؽ�Ʈ ǥ�ÿ�.
    public int maxLines = 6;       // �ִ� �� ��.
    private string[] lines;        // ��ȯ ����.
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
        // ���� ������ ���� �о��, �������� �߰�.
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
