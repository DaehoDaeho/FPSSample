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
