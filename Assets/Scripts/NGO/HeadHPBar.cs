// ------------------------------------------------------
// ����:
//   - �Ӹ� �� ���� ���� Canvas�� Slider�� Text�� �̿��� HP�� �ð�ȭ�Ѵ�.
//   - NetworkHealth.hp�� Everyone �б� �����̹Ƿ�, ��� Ŭ�󿡼� ������ ��ġ�� ���δ�.
// ����:
//   - Player �������� NameTagCanvas(���� ����) �Ʒ��� Slider�� Text�� �ΰ�, �� ��ũ��Ʈ�� ����.
//   - �ν����Ϳ��� health/slider/hpText�� ����.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class HeadHPBar : MonoBehaviour
{
    public NetworkHealth health;
    public Slider slider;
    public Text hpText;

    void Update()
    {
        if (health == null)
        {
            return;
        }
        if (slider == null)
        {
            return;
        }

        int cur = health.hp.Value;
        int max = health.maxHealth;

        if (max <= 0)
        {
            max = 1;
        }

        float value = (float)cur / (float)max;

        slider.value = value;

        if (hpText != null)
        {
            hpText.text = cur.ToString() + " / " + max.ToString();
        }
    }
}
