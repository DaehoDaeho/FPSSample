// ------------------------------------------------------
// 역할:
//   - 월드 공간 캔버스(Text)에 현재 HP를 숫자로 표시한다.
//   - 네트워크 이벤트를 따로 받지 않고, 매 프레임 NetworkVariable 값을 읽어 온다.
// 의존:
//   - 같은 Player 오브젝트에 NetworkHealth가 있어야 한다.
//   - NameTagCanvas 아래에 Text(또는 TextMeshProUGUI)가 있어야 한다.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeadHPText : MonoBehaviour
{
    public NetworkHealth health;   // 같은 오브젝트의 NetworkHealth 참조.
    public TMP_Text hpText;            // 월드 공간 캔버스의 Text.

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

        // Everyone 읽기 권한이라 모든 클라에서 동일 수치가 보인다.
        hpText.text = "HP " + health.hp.Value.ToString();
    }
}
