// NetworkUI_NGO.cs
// ----------------------------------------------------
// 역할:
//   - Host/Client/Shutdown 버튼으로 NGO 연결을 제어한다.
//   - 현재 역할(Host/Client/Server/Offline)과 접속자 수를 Text에 표시한다.
// ----------------------------------------------------

using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI_NGO : MonoBehaviour
{
    public TMP_Text statusText;
    public TMP_Text playersText;
    public float refreshInterval = 0.5f;

    private float timer = 0f;

    void Start()
    {
        UpdateStatusNow();
    }

    void Update()
    {
        timer = timer + Time.deltaTime;

        if (timer >= refreshInterval)
        {
            UpdateStatusNow();
            timer = 0f;
        }
    }

    // ----- 인스펙터 버튼 연결용 -----

    public void OnClickHost()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }

        bool ok = NetworkManager.Singleton.StartHost();
        if (ok == true)
        {
            UpdateStatusNow();
        }
    }

    public void OnClickClient()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }

        bool ok = NetworkManager.Singleton.StartClient();
        if (ok == true)
        {
            UpdateStatusNow();
        }
    }

    public void OnClickShutdown()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            NetworkManager.Singleton.Shutdown();
        }

        UpdateStatusNow();
    }

    // ----- 내부 도우미 -----

    private void UpdateStatusNow()
    {
        string role = "Offline";

        if (NetworkManager.Singleton.IsHost == true)
        {
            role = "Host (Server+Client)";
        }
        else
        {
            if (NetworkManager.Singleton.IsServer == true)
            {
                role = "Server";
            }
            else
            {
                if (NetworkManager.Singleton.IsClient == true)
                {
                    role = "Client";
                }
            }
        }

        if (statusText != null)
        {
            statusText.text = "Status: " + role;
        }

        int count = 0;

        if (NetworkManager.Singleton.IsListening == true)
        {
            count = NetworkManager.Singleton.ConnectedClientsList.Count;
        }

        if (playersText != null)
        {
            playersText.text = "Players: " + count.ToString();
        }
    }
}
