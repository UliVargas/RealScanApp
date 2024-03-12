using Fleck;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RealScanUICSharp;
using System.Windows.Forms;
using RealScan;

public class WebSocketServerMain
{
    private MainForm mainForm;

    private WebSocketServer server;
    private List<IWebSocketConnection> allSockets;
    private IWebSocketConnection localSocket = null;

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_RESTORE = 9;

    public WebSocketServerMain(MainForm form)
    {
        this.mainForm = form;
        allSockets = new List<IWebSocketConnection>();
        Start();
    }

    public void Start()
    {
        FleckLog.Level = LogLevel.Debug;
        server = new WebSocketServer("ws://127.0.0.1:8181");
        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                Console.WriteLine($"WebSocket Opened from {socket.ConnectionInfo.ClientIpAddress}");
                allSockets.Add(socket);
                // Verifica si el cliente es local
                if (socket.ConnectionInfo.ClientIpAddress == "127.0.0.1")
                {
                    localSocket = socket;
                }
            };
            socket.OnClose = () =>
            {
                Console.WriteLine("WebSocket Closed");
                allSockets.Remove(socket);
                if (socket == localSocket)
                {
                    localSocket = null;
                }
            };
            socket.OnMessage = message =>
            {
                var messageObject = JsonConvert.DeserializeObject<JObject>(message);
                var action = messageObject["action"].Value<string>();

                if(action == "StartCapture")
                {
                    int modeIndex = messageObject["parameters"]["captureMode"].Value<int>();
                    List<int> missingFingers = messageObject["parameters"]["missingFingers"].ToObject<List<int>>();

                    this.mainForm.Invoke((MethodInvoker)delegate {
                        this.mainForm.ChangeCaptureMode(modeIndex);
                    });
                    IntPtr hWndOpenApp = FindWindow(null, "Escaner Fiscalia Web");
                    if (hWndOpenApp != IntPtr.Zero)
                    {
                        SetForegroundWindow(hWndOpenApp);
                        ShowWindow(hWndOpenApp, SW_RESTORE);
                    }
                }
                 
            };
        });
    }

    public void SendMessageToLocalClient(string message)
    {
        localSocket?.Send(message);
    }
}
