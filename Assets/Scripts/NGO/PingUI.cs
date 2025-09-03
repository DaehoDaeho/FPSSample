// ------------------------------------------------------
// ����:
//   - ���� �ֱ⸶�� �� ����: Ŭ�� ������ �ð��� ������, ������ �״�� �����ָ� RTT ���.
//   - UI Text�� ms ������ ǥ��.
// ����:
//   - HUD Canvas�� Text �ϳ� ����� �� ��ũ��Ʈ�� pingText�� ����.
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
        // ������ ���� �ð��� �״�� �ݻ�.
        ReturnPongClientRpc(clientSendTime, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ReturnPongClientRpc(float clientSendTime, ulong clientId = 0, ClientRpcParams clientRpcParams = default)
    {
        // ��� Ŭ�� ��ε�ĳ��Ʈ������, "���� ��"������ �����ϵ��� �Ѵ�.
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
