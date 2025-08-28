// NetworkUI_NGO.cs
// ----------------------------------------------------
// ����:
//   - Host/Client/Shutdown ��ư���� NGO ������ �����Ѵ�.
//   - ���� ����(Host/Client/Server/Offline)�� ������ ���� Text�� ǥ���Ѵ�.
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

    // ----- �ν����� ��ư ����� -----

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

    // ----- ���� ����� -----
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
            // �� ����/ȣ��Ʈ������ ���� ���� �÷��� ���
            if (NetworkManager.Singleton.IsServer == true)
            {
                players = NetworkManager.Singleton.ConnectedClientsList.Count;
            }
            else
            {
                // �� Ŭ���̾�Ʈ������ ������ Player ������Ʈ ������ ���� ǥ��
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
