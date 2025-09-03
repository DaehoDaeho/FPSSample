// ------------------------------------------------------
// 역할:
//   - 일정 주기마다 핑 측정: 클라가 서버에 시각을 보내고, 서버가 그대로 돌려주면 RTT 계산.
//   - UI Text에 ms 단위로 표시.
// 사용법:
//   - HUD Canvas에 Text 하나 만들고 이 스크립트의 pingText에 연결.
// ------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PingUI : NetworkBehaviour
{
    public TMP_Text pingText;
    public float interval = 1.0f;

    private float timer = 0.0f;

    void Update()
    {
        if (IsClient == false)
        {
            return;
        }
        timer = timer + Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0.0f;
            float t = Time.realtimeSinceStartup;
            SendPingServerRpc(t);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPingServerRpc(float clientSendTime, ServerRpcParams rpcParams = default)
    {
        // 서버는 받은 시간을 그대로 반사.
        ReturnPongClientRpc(clientSendTime, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ReturnPongClientRpc(float clientSendTime, ulong clientId = 0, ClientRpcParams clientRpcParams = default)
    {
        // 모든 클라에 브로드캐스트되지만, "보낸 쪽"에서만 측정하도록 한다.
        if (IsClient == true)
        {
            float now = Time.realtimeSinceStartup;
            float rtt = (now - clientSendTime) * 1000.0f; // ms

            if (pingText != null)
            {
                pingText.text = "PING " + rtt.ToString("F0") + " ms";
            }
        }
    }
}
