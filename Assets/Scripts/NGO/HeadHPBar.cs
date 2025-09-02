// ------------------------------------------------------
// 역할:
//   - 머리 위 월드 공간 Canvas의 Slider와 Text를 이용해 HP를 시각화한다.
//   - NetworkHealth.hp는 Everyone 읽기 권한이므로, 모든 클라에서 동일한 수치가 보인다.
// 사용법:
//   - Player 프리팹의 NameTagCanvas(월드 공간) 아래에 Slider와 Text를 두고, 이 스크립트를 부착.
//   - 인스펙터에서 health/slider/hpText를 연결.
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
