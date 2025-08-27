// EchoClientMono.cs
// ����: ����(127.0.0.1:<��Ʈ>)�� ���� �� �Է�â�� ���ڿ��� ������ �� ���� ������ �α׿� ǥ��.
// ��Ģ: ��� if�� �߰�ȣ, �Ҹ��� ��Ȯ ��, ����/�̺�Ʈ/async �̻��, UTF-8 ���.

using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using TMPro;

public class EchoClientMono : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField ipInput;          // �⺻ 127.0.0.1
    public TMP_InputField portInput;        // �⺻ 7777
    public TMP_InputField messageInput;     // ���� ���ڿ�
    public TMP_Text clientStatusText;
    public TMP_Text clientLogText;

    [Header("Options")]
    public int defaultPort = 7777;
    public string defaultIp = "127.0.0.1";
    public int maxLogLines = 200;

    // ���� ����
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;
    private Thread recvThread;
    private bool connected = false;

    // ���� �α� ť
    private List<string> logQueue = new List<string>();
    private object logLock = new object();

    void Start()
    {
        if (ipInput != null && string.IsNullOrEmpty(ipInput.text) == true) { ipInput.text = defaultIp; }
        if (portInput != null && string.IsNullOrEmpty(portInput.text) == true) { portInput.text = defaultPort.ToString(); }
        SetStatus("Offline");
    }

    void Update()
    {
        // ���� �����尡 �׾Ƶ� �α׸� ���� �����忡�� �ݿ�
        List<string> pending = null;

        lock (logLock)
        {
            if (logQueue.Count > 0)
            {
                pending = new List<string>(logQueue);
                logQueue.Clear();
            }
        }

        if (pending != null)
        {
            for (int i = 0; i < pending.Count; i++)
            {
                AppendClientLog(pending[i]);
            }
        }
    }

    void OnApplicationQuit()
    {
        DisconnectButton();
    }

    // ---------- UI ��ư ----------

    public void ConnectButton()
    {
        if (connected == true)
        {
            return;
        }

        string ip = defaultIp;
        int port = defaultPort;

        if (ipInput != null && string.IsNullOrEmpty(ipInput.text) == false)
        {
            ip = ipInput.text.Trim();
        }

        if (portInput != null)
        {
            int parsed;
            bool ok = int.TryParse(portInput.text, out parsed);
            if (ok == true)
            {
                port = parsed;
            }
        }

        try
        {
            client = new TcpClient();
            client.Connect(ip, port);

            NetworkStream ns = client.GetStream();
            reader = new StreamReader(ns, Encoding.UTF8);
            writer = new StreamWriter(ns, Encoding.UTF8);
            writer.AutoFlush = true;

            connected = true;
            SetStatus("Connected " + ip + ":" + port.ToString());
            AppendClientLog("[CLIENT] Connected.");

            // ���� ������ ����
            recvThread = new Thread(RecvLoop);
            recvThread.IsBackground = true;
            recvThread.Start();
        }
        catch (Exception ex)
        {
            AppendClientLog("[CLIENT] ERROR: " + ex.Message);
            SetStatus("Error");
        }
    }

    public void DisconnectButton()
    {
        if (connected == false)
        {
            return;
        }

        connected = false;

        if (recvThread != null)
        {
            try { recvThread.Join(200); } catch { }
            recvThread = null;
        }

        if (reader != null) { try { reader.Close(); } catch { } reader = null; }
        if (writer != null) { try { writer.Close(); } catch { } writer = null; }
        if (client != null) { try { client.Close(); } catch { } client = null; }

        SetStatus("Offline");
        AppendClientLog("[CLIENT] Disconnected.");
    }

    public void SendButton()
    {
        if (connected == false)
        {
            return;
        }

        if (writer == null)
        {
            return;
        }

        string msg = "";
        if (messageInput != null)
        {
            msg = messageInput.text;
        }

        if (string.IsNullOrWhiteSpace(msg) == true)
        {
            return;
        }

        try
        {
            writer.WriteLine(msg);
            AppendClientLog("[YOU] " + msg);

            if (messageInput != null)
            {
                messageInput.text = "";
            }
        }
        catch (Exception ex)
        {
            AppendClientLog("[CLIENT] SEND ERROR: " + ex.Message);
        }
    }

    // ---------- ���� ����(���� ������) ----------

    private void RecvLoop()
    {
        try
        {
            string first = reader.ReadLine();
            if (first != null)
            {
                AppendFromThread("[SERVER] " + first);
            }

            bool running = true;

            while (running == true && connected == true)
            {
                string line = reader.ReadLine();

                if (line == null)
                {
                    AppendFromThread("[CLIENT] Server closed.");
                    break;
                }

                AppendFromThread("[SERVER] " + line);

                if (line.Trim().Equals("BYE", StringComparison.OrdinalIgnoreCase) == true)
                {
                    running = false;
                }
            }
        }
        catch (IOException)
        {
            AppendFromThread("[CLIENT] Connection closed.");
        }
        catch (Exception ex)
        {
            AppendFromThread("[CLIENT] ERROR: " + ex.Message);
        }

        connected = false;
        AppendFromThread("[STATUS] Offline");
    }

    // ---------- �����(UI) ----------

    private void AppendClientLog(string line)
    {
        if (clientLogText != null)
        {
            clientLogText.text += line + "\n";

            string[] lines = clientLogText.text.Split('\n');
            if (lines.Length > maxLogLines)
            {
                int start = lines.Length - maxLogLines;
                string trimmed = "";
                for (int i = start; i < lines.Length; i++)
                {
                    if (lines[i].Length > 0)
                    {
                        trimmed += lines[i] + "\n";
                    }
                }
                clientLogText.text = trimmed;
            }
        }
    }

    private void SetStatus(string s)
    {
        if (clientStatusText != null)
        {
            clientStatusText.text = "Client: " + s;
        }
    }

    private void AppendFromThread(string s)
    {
        lock (logLock)
        {
            logQueue.Add(s);
        }
    }
}
