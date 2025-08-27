// EchoServerMono.cs
// ����: TCP ���� ����. 127.0.0.1:<��Ʈ>���� ��� �� �� �� ���� �� ���� ���� �״�� ������.
// ��Ģ: ��� if�� �߰�ȣ, �Ҹ����� ��Ȯ ��(==true/==false), ����/�̺�Ʈ/async �̻��.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EchoServerMono : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField serverPortInput;
    public TMP_Text serverStatusText;
    public TMP_Text serverLogText;

    [Header("Options")]
    public int defaultPort = 7777;
    public int maxLogLines = 200;

    // ���� ����(����)
    private TcpListener listener;
    private Thread listenerThread;
    private bool serverRunning = false;

    // ���� ���� Ŭ���̾�Ʈ(1��)
    private TcpClient currentClient;
    private StreamReader reader;
    private StreamWriter writer;
    private Thread clientThread;

    // ���������� UI �ݿ��� ť
    private List<string> logQueue = new List<string>();
    private object logLock = new object();

    void Update()
    {
        // ���� �����忡�� ���� �α׸� ���� �����忡�� UI�� �ݿ�
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
                AppendServerLog(pending[i]);
            }
        }
    }

    void OnApplicationQuit()
    {
        StopServerButton();
    }

    // ---------- UI ��ư ----------

    public void StartServerButton()
    {
        if (serverRunning == true)
        {
            return;
        }

        int port = defaultPort;

        if (serverPortInput != null)
        {
            int parsed;
            bool ok = int.TryParse(serverPortInput.text, out parsed);
            if (ok == true)
            {
                port = parsed;
            }
        }

        try
        {
            IPAddress ip = IPAddress.Loopback; // 127.0.0.1
            listener = new TcpListener(ip, port);
            listener.Start();
            serverRunning = true;

            AppendFromThread("[SERVER] Started on 127.0.0.1:" + port.ToString());
            SetServerStatus("Running");

            listenerThread = new Thread(ListenLoop);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }
        catch (Exception ex)
        {
            AppendServerLog("[SERVER] ERROR: " + ex.Message);
            SetServerStatus("Error");
        }
    }

    public void StopServerButton()
    {
        if (serverRunning == false)
        {
            return;
        }

        serverRunning = false;

        if (listener != null)
        {
            try { listener.Stop(); } catch { }
            listener = null;
        }

        CloseClient();

        if (listenerThread != null)
        {
            try { listenerThread.Join(200); } catch { }
            listenerThread = null;
        }

        SetServerStatus("Stopped");
        AppendServerLog("[SERVER] Stopped.");
    }

    // ---------- ���� ����(���� ������) ----------

    private void ListenLoop()
    {
        AppendFromThread("[SERVER] Waiting for client...");

        while (serverRunning == true)
        {
            try
            {
                // 1) ���� ����(���ŷ). Stop() ȣ�� �� SocketException �߻� �� catch���� Ż��
                TcpClient client = listener.AcceptTcpClient();

                if (serverRunning == false)
                {
                    try { client.Close(); } catch { }
                    break;
                }

                AppendFromThread("[SERVER] Client connected.");

                // ���� Ŭ�� �־����� �ݱ�(�ܼ�ȭ�� ���� ���� 1��)
                CloseClient();

                currentClient = client;

                NetworkStream ns = currentClient.GetStream();
                reader = new StreamReader(ns, Encoding.UTF8);
                writer = new StreamWriter(ns, Encoding.UTF8);
                writer.AutoFlush = true;

                // ���� �λ�
                SafeWriteLine("WELCOME TCP ECHO. Type text and press Enter. Use /quit to exit.");

                // 2) Ŭ�� �б� ����(���� ������)
                clientThread = new Thread(ClientLoop);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch (SocketException)
            {
                // listener.Stop()���� ��� ���
                break;
            }
            catch (Exception ex)
            {
                AppendFromThread("[SERVER] Accept ERROR: " + ex.Message);
                break;
            }
        }
    }

    private void ClientLoop()
    {
        bool running = true;

        while (running == true && serverRunning == true)
        {
            try
            {
                // �� ���� ����(��밡 ���� ������ null)
                string line = reader.ReadLine();

                if (line == null)
                {
                    AppendFromThread("[SERVER] Client disconnected.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line) == true)
                {
                    // �� ���� ����
                    continue;
                }

                if (line.Trim().Equals("/quit", StringComparison.OrdinalIgnoreCase) == true)
                {
                    SafeWriteLine("BYE");
                    AppendFromThread("[SERVER] Client requested quit.");
                    running = false;
                    continue;
                }

                string echo = "ECHO: " + line;
                SafeWriteLine(echo);
                AppendFromThread("[SERVER] " + echo);
            }
            catch (IOException)
            {
                AppendFromThread("[SERVER] Connection closed.");
                break;
            }
            catch (Exception ex)
            {
                AppendFromThread("[SERVER] ERROR: " + ex.Message);
                break;
            }
        }

        CloseClient();
    }

    // ---------- ����� ----------

    private void SafeWriteLine(string msg)
    {
        try
        {
            if (writer != null)
            {
                writer.WriteLine(msg);
            }
        }
        catch { }
    }

    private void CloseClient()
    {
        if (clientThread != null)
        {
            try { clientThread.Join(100); } catch { }
            clientThread = null;
        }

        if (reader != null) { try { reader.Close(); } catch { } reader = null; }
        if (writer != null) { try { writer.Close(); } catch { } writer = null; }
        if (currentClient != null) { try { currentClient.Close(); } catch { } currentClient = null; }
    }

    private void AppendServerLog(string line)
    {
        if (serverLogText != null)
        {
            serverLogText.text += line + "\n";

            string[] lines = serverLogText.text.Split('\n');
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
                serverLogText.text = trimmed;
            }
        }
    }

    private void SetServerStatus(string status)
    {
        if (serverStatusText != null)
        {
            serverStatusText.text = "Server: " + status;
        }
    }

    // �����忡�� �α׸� �����ϰ� ť��
    private void AppendFromThread(string s)
    {
        lock (logLock)
        {
            logQueue.Add(s);
        }
    }
}
