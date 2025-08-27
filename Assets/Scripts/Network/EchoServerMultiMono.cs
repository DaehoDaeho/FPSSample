// EchoServerMultiMono.cs
// -----------------------------------------------------
// ����:
//   - ���� Ŭ���̾�Ʈ ���� ���� ������ TCP ���� ����.
//   - �� Ŭ���̾�Ʈ�� ���� �� ��(line) �ؽ�Ʈ��
//     1) ��ü ���(broadcast)�ϰų�
//     2) ���� ������Ը� ȸ��(���� ����)�ϵ��� ���� ��带 ����.
// ���� �ٽ�: ������ ������(accept), ���� ������(Ŭ�� ReadLine), ���� ��� lock ��ȣ,
//           UI ������ ���� �����忡����(ť ���), UTF-8, ���� ��� ��������.
// ��Ģ: ��� if�� ���, �Ҹ��� ��Ȯ ��, ����/�̺�Ʈ/async �̻��.
//
// ����: WebGL ������(Standalone ����). �ܺ� PC ������ allowExternalClients==true + ��ȭ�� ��� �ʿ�.
// -----------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using TMPro;

public class EchoServerMultiMono : MonoBehaviour
{
    [Header("UI (Server Panel)")]
    public TMP_InputField serverPortInput;
    public TMP_Text serverStatusText;
    public TMP_Text serverLogText;

    [Header("Server Options")]
    public int defaultPort = 7777;
    public int maxLogLines = 300;
    public bool broadcastToAll = true;      // true: ��ü ��ε�ĳ����, false: ���� ������Ը� ȸ��
    public bool allowExternalClients = false; // true: 0.0.0.0 ���ε�(LAN ���)

    // ��Ÿ�� ����
    private TcpListener listener;
    private Thread listenerThread;
    private bool serverRunning = false;

    // ���� ���
    private List<ClientSession> sessions = new List<ClientSession>();
    private object sessionsLock = new object();
    private int nextClientId = 1;

    // �α� ť(����������)
    private List<string> logQueue = new List<string>();
    private object logLock = new object();

    void Update()
    {
        // �����忡�� ���� �α׸� ���� �����忡�� UI�� �ݿ�
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

    // ----- ����/���� ��ư -----

    public void StartServerButton()
    {
        if (serverRunning == true)
        {
            return;
        }

        int port = defaultPort;

        if (serverPortInput != null)
        {
            int parsed = defaultPort;
            bool ok = int.TryParse(serverPortInput.text, out parsed);
            if (ok == true)
            {
                port = parsed;
            }
        }

        try
        {
            IPAddress bindIp = IPAddress.Loopback; // 127.0.0.1
            if (allowExternalClients == true)
            {
                bindIp = IPAddress.Any; // 0.0.0.0
            }

            listener = new TcpListener(bindIp, port);
            listener.Start();
            serverRunning = true;

            SetServerStatus("Running on " + bindIp.ToString() + ":" + port.ToString());
            EnqueueLog("[SERVER] Started. Waiting for clients...");

            listenerThread = new Thread(new ThreadStart(ListenLoop));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }
        catch (Exception ex)
        {
            SetServerStatus("Error");
            EnqueueLog("[SERVER] ERROR: " + ex.Message);
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

        // ��� ���� ����
        List<ClientSession> copy = null;
        lock (sessionsLock)
        {
            copy = new List<ClientSession>(sessions);
        }
        for (int i = 0; i < copy.Count; i++)
        {
            if (copy[i] != null)
            {
                copy[i].Close();
            }
        }

        if (listenerThread != null)
        {
            try { listenerThread.Join(300); } catch { }
            listenerThread = null;
        }

        lock (sessionsLock)
        {
            sessions.Clear();
        }

        SetServerStatus("Stopped");
        EnqueueLog("[SERVER] Stopped.");
    }

    // ----- ������ ����(���� ����) -----

    private void ListenLoop()
    {
        while (serverRunning == true)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();

                if (serverRunning == false)
                {
                    try { client.Close(); } catch { }
                    break;
                }

                ClientSession sess = new ClientSession();
                sess.server = this;
                sess.client = client;
                sess.id = nextClientId;
                nextClientId = nextClientId + 1;

                lock (sessionsLock)
                {
                    sessions.Add(sess);
                }

                EnqueueLog("[SERVER] Client #" + sess.id.ToString() + " connected.");

                sess.StartSession();
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
                EnqueueLog("[SERVER] Accept ERROR: " + ex.Message);
                break;
            }
        }
    }

    // ----- ���� ���� / �۽� ��ƿ (���� ������ private�� ����) -----

    private void RemoveSession(ClientSession s)
    {
        lock (sessionsLock)
        {
            sessions.Remove(s);
        }
        EnqueueLog("[SERVER] Client #" + s.id.ToString() + " removed.");
    }

    private void BroadcastLine(string line)
    {
        List<ClientSession> copy = null;
        lock (sessionsLock)
        {
            copy = new List<ClientSession>(sessions);
        }

        for (int i = 0; i < copy.Count; i++)
        {
            if (copy[i] != null)
            {
                copy[i].SafeWriteLine(line);
            }
        }
    }

    private void SendToOne(ClientSession target, string line)
    {
        if (target != null)
        {
            target.SafeWriteLine(line);
        }
    }

    // ----- UI/�α� ����� -----

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

    private void EnqueueLog(string s)
    {
        lock (logLock)
        {
            logQueue.Add(s);
        }
    }

    // ----- ���� Ŭ����(Ŭ���̾�Ʈ 1�� ���) -----

    private class ClientSession
    {
        public EchoServerMultiMono server;
        public TcpClient client;
        public int id;

        private StreamReader reader;
        private StreamWriter writer;
        private Thread thread;

        public void StartSession()
        {
            try
            {
                NetworkStream ns = client.GetStream();
                reader = new StreamReader(ns, Encoding.UTF8);
                writer = new StreamWriter(ns, Encoding.UTF8);
                writer.AutoFlush = true;

                SafeWriteLine("WELCOME. Your id is #" + id.ToString() + ". Type /quit to exit.");

                thread = new Thread(new ThreadStart(Run));
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                server.EnqueueLog("[SERVER] Session start ERROR(#" + id.ToString() + "): " + ex.Message);
                Close();
                server.RemoveSession(this); // private�̾ ��ø Ÿ���̹Ƿ� ���� ����
            }
        }

        public void Run()
        {
            bool running = true;

            while (running == true)
            {
                try
                {
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        server.EnqueueLog("[SERVER] Client #" + id.ToString() + " disconnected.");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line) == true)
                    {
                        continue;
                    }

                    if (line.Trim().Equals("/quit", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        SafeWriteLine("BYE");
                        server.EnqueueLog("[SERVER] Client #" + id.ToString() + " requested quit.");
                        running = false;
                        continue;
                    }

                    string echo = "ECHO:#" + id.ToString() + " " + line;

                    if (server.broadcastToAll == true)
                    {
                        server.BroadcastLine(echo); // private�̾ ���� ����(��ø Ÿ��)
                    }
                    else
                    {
                        server.SendToOne(this, echo); // private�̾ ���� ����
                    }

                    server.EnqueueLog("[SERVER] " + echo);
                }
                catch (IOException)
                {
                    server.EnqueueLog("[SERVER] IO closed for #" + id.ToString() + ".");
                    break;
                }
                catch (Exception ex)
                {
                    server.EnqueueLog("[SERVER] ERROR(#" + id.ToString() + "): " + ex.Message);
                    break;
                }
            }

            Close();
            server.RemoveSession(this);
        }

        public void SafeWriteLine(string msg)
        {
            try
            {
                if (writer != null)
                {
                    writer.WriteLine(msg);
                }
            }
            catch
            {
                // �۽� ������ ������ ����(���´� ���� ������ ����)
            }
        }

        public void Close()
        {
            if (thread != null)
            {
                try { thread.Join(80); } catch { }
                thread = null;
            }

            if (reader != null) { try { reader.Close(); } catch { } reader = null; }
            if (writer != null) { try { writer.Close(); } catch { } writer = null; }
            if (client != null) { try { client.Close(); } catch { } client = null; }
        }
    }
}
