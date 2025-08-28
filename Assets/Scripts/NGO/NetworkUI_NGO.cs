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

        if (NetworkManager.Singleton != null)
        {
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
        }

        bool connected = false;

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsClient == true)
            {
                connected = NetworkManager.Singleton.IsConnectedClient;
            }
        }

        if (statusText != null)
        {
            if (connected == true)
            {
                statusText.text = "Status: " + role + " (Connected)";
            }
            else
            {
                statusText.text = "Status: " + role + " (Not Connected)";
            }
        }

        int players = 0;

        if (NetworkManager.Singleton != null)
        {
            // ★ 서버/호스트에서만 서버 전용 컬렉션 사용
            if (NetworkManager.Singleton.IsServer == true)
            {
                players = NetworkManager.Singleton.ConnectedClientsList.Count;
            }
            else
            {
                // ★ 클라이언트에서는 스폰된 Player 오브젝트 개수를 세서 표시
                if (NetworkManager.Singleton.IsClient == true && connected == true)
                {
                    players = CountPlayersClientSide();
                }
            }
        }

        if (playersText != null)
        {
            playersText.text = "Players: " + players.ToString();
        }
    }

    private int CountPlayersClientSide()
    {
        if (NetworkManager.Singleton == null)
        {
            return 0;
        }

        if (NetworkManager.Singleton.SpawnManager == null)
        {
            return 0;
        }

        int count = 0;

        // SpawnedObjects: Dictionary<ulong, NetworkObject>
        foreach (var kv in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject no = kv.Value;

            if (no != null)
            {
                if (no.IsPlayerObject == true)
                {
                    count = count + 1;
                }
            }
        }

        return count;
    }
}
