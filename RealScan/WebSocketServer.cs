using Fleck;
using System;
using System.Runtime.InteropServices;

public class WebSocketServerMain
{
    private WebSocketServer server;

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_RESTORE = 9;
    public void Start ()
    {
        FleckLog.Level = LogLevel.Debug;
        server = new WebSocketServer("ws://127.0.0.1:8181");
        server.Start(socket =>
        {
            socket.OnOpen = () => Console.WriteLine("WebSocket Opened");
            socket.OnClose = () => Console.WriteLine("WebSocket Closed");
            socket.OnMessage = message =>
            {

                if (message == "openDesktopApp")
                {
                    IntPtr hWnd = FindWindow(null, "RealScan");

                    if (hWnd != IntPtr.Zero)
                    {
                        // Restore to windows if it minimize
                        SetForegroundWindow(hWnd);
                        // Get window within front
                        ShowWindow(hWnd, SW_RESTORE);
                    }
                }
            };

        });
    }    
}