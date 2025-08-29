// 역할: Host/Client/Shutdown + 주소/포트 입력 + 상태/인원 표시
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI_NGO_LAN : MonoBehaviour
{
    public TMP_Text statusText;
    public TMP_Text playersText;
    public TMP_InputField addressInput;
    public TMP_InputField portInput;
    public float refreshInterval = 0.5f;

    private float timer = 0.0f;

    void Start()
    {
        if (addressInput != null)
        {
            if (string.IsNullOrEmpty(addressInput.text) == true)
            {
                addressInput.text = "127.0.0.1";
            }
        }
        if (portInput != null)
        {
            if (string.IsNullOrEmpty(portInput.text) == true)
            {
                portInput.text = "7777";
            }
        }
        UpdateStatusNow();
    }

    void Update()
    {
        timer = timer + Time.deltaTime;
        if (timer >= refreshInterval)
        {
            UpdateStatusNow();
            timer = 0.0f;
        }
    }

    public void OnClickHost()
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatus("No NetworkManager");
            return;
        }
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }

        UnityTransport utp = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        if (utp == null)
        {
            SetStatus("No Transport");
            return;
        }

        ushort port = 7777;
        if (portInput != null)
        {
            int parsed = 7777;
            bool ok = int.TryParse(portInput.text, out parsed);
            if (ok == true)
            {
                if (parsed >= 1 && parsed <= 65535)
                {
                    port = (ushort)parsed;
                }
            }
        }

        // 호스트는 포트만 보정(주소는 의미 거의 없음)
        utp.SetConnectionData("127.0.0.1", port);

        bool okHost = NetworkManager.Singleton.StartHost();
        if (okHost == true)
        {
            UpdateStatusNow();
        }
        else
        {
            SetStatus("Host Start Failed");
        }
    }

    public void OnClickClient()
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatus("No NetworkManager");
            return;
        }
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }

        UnityTransport utp = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        if (utp == null)
        {
            SetStatus("No Transport");
            return;
        }

        string addr = "127.0.0.1";
        ushort port = 7777;

        if (addressInput != null)
        {
            if (string.IsNullOrEmpty(addressInput.text) == false)
            {
                addr = addressInput.text.Trim();
            }
        }
        if (portInput != null)
        {
            int parsed = 7777;
            bool ok = int.TryParse(portInput.text, out parsed);
            if (ok == true)
            {
                if (parsed >= 1 && parsed <= 65535)
                {
                    port = (ushort)parsed;
                }
            }
        }

        utp.SetConnectionData(addr, port);

        bool okClient = NetworkManager.Singleton.StartClient();
        if (okClient == true)
        {
            UpdateStatusNow();
        }
        else
        {
            SetStatus("Client Start Failed");
        }
    }

    public void OnClickShutdown()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening == true)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
        UpdateStatusNow();
    }

    private void UpdateStatusNow()
    {
        string role = "Offline";
        bool connected = false;

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost == true)
            {
                role = "Host";
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
            if (NetworkManager.Singleton.IsServer == true)
            {
                players = NetworkManager.Singleton.ConnectedClientsList.Count;
            }
            else
            {
                if (NetworkManager.Singleton.IsClient == true && connected == true)
                {
                    // 클라에서는 스폰된 Player 오브젝트 개수로 집계
                    if (NetworkManager.Singleton.SpawnManager != null)
                    {
                        foreach (var kv in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
                        {
                            if (kv.Value != null)
                            {
                                if (kv.Value.IsPlayerObject == true)
                                {
                                    players = players + 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (playersText != null)
        {
            playersText.text = "Players: " + players.ToString();
        }
    }

    private void SetStatus(string s)
    {
        if (statusText != null)
        {
            statusText.text = "Status: " + s;
        }
    }
}
