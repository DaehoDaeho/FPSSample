// EchoServerMultiMono.cs
// -----------------------------------------------------
// 목적:
//   - 여러 클라이언트 동시 접속 가능한 TCP 에코 서버.
//   - 각 클라이언트가 보낸 한 줄(line) 텍스트를
//     1) 전체 방송(broadcast)하거나
//     2) 보낸 사람에게만 회신(개별 에코)하도록 동작 모드를 선택.
// 설계 핵심: 리스너 스레드(accept), 세션 스레드(클라별 ReadLine), 세션 목록 lock 보호,
//           UI 갱신은 메인 스레드에서만(큐 사용), UTF-8, 라인 기반 프로토콜.
// 규칙: 모든 if는 블록, 불리언 명확 비교, 람다/이벤트/async 미사용.
//
// 주의: WebGL 미지원(Standalone 전용). 외부 PC 접속은 allowExternalClients==true + 방화벽 허용 필요.
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
    public bool broadcastToAll = true;      // true: 전체 브로드캐스팅, false: 보낸 사람에게만 회신
    public bool allowExternalClients = false; // true: 0.0.0.0 바인딩(LAN 허용)

    // 런타임 상태
    private TcpListener listener;
    private Thread listenerThread;
    private bool serverRunning = false;

    // 세션 목록
    private List<ClientSession> sessions = new List<ClientSession>();
    private object sessionsLock = new object();
    private int nextClientId = 1;

    // 로그 큐(스레드→메인)
    private List<string> logQueue = new List<string>();
    private object logLock = new object();

    void Update()
    {
        // 스레드에서 쌓인 로그를 메인 스레드에서 UI에 반영
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

    // ----- 시작/정지 버튼 -----

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

        // 모든 세션 종료
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

    // ----- 리스닝 루프(접속 수락) -----

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

    // ----- 세션 관리 / 송신 유틸 (접근 수준을 private로 낮춤) -----

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

    // ----- UI/로그 도우미 -----

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

    // ----- 세션 클래스(클라이언트 1명 담당) -----

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
                server.RemoveSession(this); // private이어도 중첩 타입이므로 접근 가능
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
                        server.BroadcastLine(echo); // private이어도 접근 가능(중첩 타입)
                    }
                    else
                    {
                        server.SendToOne(this, echo); // private이어도 접근 가능
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
                // 송신 오류는 조용히 무시(상태는 수신 측에서 정리)
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
