// ------------------------------------------------------
// ����:
//   - ���� ���� ĵ����(Text)�� ���� HP�� ���ڷ� ǥ���Ѵ�.
//   - ��Ʈ��ũ �̺�Ʈ�� ���� ���� �ʰ�, �� ������ NetworkVariable ���� �о� �´�.
// ����:
//   - ���� Player ������Ʈ�� NetworkHealth�� �־�� �Ѵ�.
//   - NameTagCanvas �Ʒ��� Text(�Ǵ� TextMeshProUGUI)�� �־�� �Ѵ�.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeadHPText : MonoBehaviour
{
    public NetworkHealth health;   // ���� ������Ʈ�� NetworkHealth ����.
    public TMP_Text hpText;            // ���� ���� ĵ������ Text.

    void Update()
    {
        if (health == null)
        {
            return;
        }
        if (hpText == null)
        {
            return;
        }

        // Everyone �б� �����̶� ��� Ŭ�󿡼� ���� ��ġ�� ���δ�.
        hpText.text = "HP " + health.hp.Value.ToString();
    }
}
