// EchoServerMono.cs
// 역할: TCP 에코 서버. 127.0.0.1:<포트>에서 대기 → 한 명 접속 → 받은 줄을 그대로 돌려줌.
// 규칙: 모든 if는 중괄호, 불리언은 명확 비교(==true/==false), 람다/이벤트/async 미사용.

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

    // 내부 상태(서버)
    private TcpListener listener;
    private Thread listenerThread;
    private bool serverRunning = false;

    // 현재 접속 클라이언트(1명만)
    private TcpClient currentClient;
    private StreamReader reader;
    private StreamWriter writer;
    private Thread clientThread;

    // 스레드→메인 UI 반영용 큐
    private List<string> logQueue = new List<string>();
    private object logLock = new object();

    void Update()
    {
        // 수신 스레드에서 쌓인 로그를 메인 스레드에서 UI에 반영
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

    // ---------- UI 버튼 ----------

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

    // ---------- 서버 루프(전용 스레드) ----------

    private void ListenLoop()
    {
        AppendFromThread("[SERVER] Waiting for client...");

        while (serverRunning == true)
        {
            try
            {
                // 1) 접속 수락(블로킹). Stop() 호출 시 SocketException 발생 → catch에서 탈출
                TcpClient client = listener.AcceptTcpClient();

                if (serverRunning == false)
                {
                    try { client.Close(); } catch { }
                    break;
                }

                AppendFromThread("[SERVER] Client connected.");

                // 기존 클라가 있었으면 닫기(단순화를 위해 동시 1명만)
                CloseClient();

                currentClient = client;

                NetworkStream ns = currentClient.GetStream();
                reader = new StreamReader(ns, Encoding.UTF8);
                writer = new StreamWriter(ns, Encoding.UTF8);
                writer.AutoFlush = true;

                // 접속 인사
                SafeWriteLine("WELCOME TCP ECHO. Type text and press Enter. Use /quit to exit.");

                // 2) 클라 읽기 루프(전용 스레드)
                clientThread = new Thread(ClientLoop);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch (SocketException)
            {
                // listener.Stop()으로 깨어난 경우
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
                // 줄 단위 수신(상대가 연결 끊으면 null)
                string line = reader.ReadLine();

                if (line == null)
                {
                    AppendFromThread("[SERVER] Client disconnected.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line) == true)
                {
                    // 빈 줄은 무시
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

    // ---------- 도우미 ----------

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

    // 스레드에서 로그를 안전하게 큐잉
    private void AppendFromThread(string s)
    {
        lock (logLock)
        {
            logQueue.Add(s);
        }
    }
}
