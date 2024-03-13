using Newtonsoft.Json;
using RealScan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace RealScanUICSharp
{
    public partial class MainForm : Form
    {
        int m_result = 0;
        int m_errorcode = 0;

        int m_overlayHandle;
        int deviceHandle = 0;

        bool m_automatic;
        bool m_advancedContrast;
        int m_manualContrast;
        int m_rollDirection, m_rollTime, m_rollProfile;
        int m_mode, m_option;
        bool m_contrastEnhancement, m_noiseReduction;
        int m_reductionLevel;

        int m_keyCode;

        string m_errorMsg = " ";

        public bool AutoCalibrate = false;

        bool m_prevStopped = true;

        bool m_bCaptureModeSelected = false;
        int m_captureMode = 0;
        int m_minCount = 0;
        int m_captureDir = 0;
        int m_slapType = 0;
        int m_fingerCount = 0;

        int m_nCustomSegWidth = 0;
        int m_nCustomSegHeight = 0;
        byte[] blob = null;

        enum PrevMode
        {
            callbackDraw
        }

        enum callbackMode
        {
            saveNseg,
            // seqCheck
        }

        private PrevMode _selectedPrevMode;
        RSPreviewDataCallback previewCallback;

        private Thread autoCaptureThread = null;
        delegate void afterAutoCaptureCallback(int captureResult);

        int capturedImageWidth;
        int capturedImageHeight;
        IntPtr capturedImageData;

        bool ResetLCDEnable = false;

        bool DisplayLCDEnable = false;

        public int FingerColorSelectedIndex = 0;

        public int FingerLEDSelectedIndex = 0;

        public int BeepPatternSelectedIndex = 0;

        public int StatusLEDColorSelectedIndex = 0;

        public int KeyMaskSelectedIndex = 0;

        public int KEYCALLBACKSelectedIndex = 0;

        public string CALLBACKPRINTText = "";

        public int ModeLEDSelectedIndex = 0;

        public bool InitDeviceEnabled = false;
        public bool ExitDeviceEnabled = false;

        public int CallbackSelectedIndex = 0;

        public string ImageSize = "";

        public int DeviceListSelectedIndex = -1;

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        //[Conditional("DEBUG")]
        private void AllocateConsole()
        {
            AllocConsole();
        }

        //[Conditional("DEBUG")]
        private void DeallocateConsole()
        {
            FreeConsole();
        }

        WebSocketServerMain webSocketServer;

        public MainForm()
        {
            InitializeComponent();
            StartWebSocketServer();
            this.Text = "Escaner Fiscalia Web";
            Icon icon = Icon.ExtractAssociatedIcon("fgebc.ico");
            this.Icon = icon;

            this.Load += new EventHandler(MainForm_Load);
            this.FormClosed += Exit_DeviceHandler;

            CallbackSelectedIndex = 0;
            int FingerLEDSelectedIndex = 0;
            int FingerColorSelectedIndex = 0;
            int ModeLEDSelectedIndex = 0;
            int BeepPatternSelectedIndex = 0;
            int StatusLEDColorSelectedIndex = 0;
            int KeyMaskSelectedIndex = 0;
            int KEYCALLBACKSelectedIndex = 0;
            CaptureMode.SelectedIndex = 0;
            AutoScroll = true;

            previewCallback = new RSPreviewDataCallback(previewDataCallback);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitSDKAndDevice();
        }

        private void StartWebSocketServer()
        {
            webSocketServer = new WebSocketServerMain(this);
            Task.Run(() => webSocketServer.Start());
        }

        private void Init_SDK()
        {
            int numOfDevice = 0;
            _ = new RSSDKInfo();

            m_result = RealScanSDK.RS_InitSDK(null, 0, ref numOfDevice);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }


            MsgPanel.Text = "El SDK se inicializó correctamente";

            if (numOfDevice > 0)
            {
                DeviceListSelectedIndex = 0;
            }
        }

        private int CheckDevicesAvailable()
        {
            int numOfDevice = 0;
            int result = RealScanSDK.RS_InitSDK(null, 0, ref numOfDevice);
            if (result == RealScanSDK.RS_SUCCESS && numOfDevice > 0)
            {
                return numOfDevice;
            }
            return 0;
        }

        private async void InitSDKAndDevice()
        {
            Init_SDK();

            using (var esperaForm = new EsperaForm())
            {
                esperaForm.Show();
                esperaForm.Invoke(new Action(() => esperaForm.Text = "Connectando..."));
                esperaForm.UpdateStatusText("Por favor, espera mientras se establece la conexión con el dispositivo...");

                bool deviceConnected = false;
                await Task.Run(async () =>
                {
                    while (!deviceConnected)
                    {
                        int numOfDevice = CheckDevicesAvailable();

                        if (numOfDevice > 0)
                        {
                            deviceConnected = true;
                            DeviceListSelectedIndex = 0;
                        }
                        else
                        {
                            esperaForm.Invoke(new Action(() => esperaForm.Text = "Esperando dispositivo..."));
                            esperaForm.UpdateStatusText("No se detecta ningún dispositivo. Por favor, conecta un dispositivo...");
                            await Task.Delay(1000);
                        }
                    }
                });

                esperaForm.Invoke(new Action(() => esperaForm.Text = "Connectando..."));
                esperaForm.UpdateStatusText("Dispositivo conectado. Finalizando la inicialización...");

                await Init_Device();

                esperaForm.Invoke((MethodInvoker)delegate
                {
                    esperaForm.Close();
                });
            }
        }


        private async Task Init_Device()
        {
            await Task.Run(() =>
            {
                RSDeviceInfo deviceInfo = new RSDeviceInfo();

                m_result = RealScanSDK.RS_InitDevice(DeviceListSelectedIndex, ref deviceHandle);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    this.Invoke(new Action(() =>
                    {
                        MsgPanel.Text = m_errorMsg;
                    }));
                    return;
                }

                AutoCalibrate = true;
                m_prevStopped = true;

                m_result = RealScanSDK.RS_GetDeviceInfo(deviceHandle, ref deviceInfo);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    this.Invoke(new Action(() =>
                    {
                        MsgPanel.Text = m_errorMsg;
                    }));
                    return;
                }

                this.Invoke(new Action(() =>
                {
                    DeviceInfo.Text = System.Text.Encoding.ASCII.GetString(deviceInfo.productName);
                    InitDeviceEnabled = false;
                    ExitDeviceEnabled = true;

                    if (deviceInfo.deviceType != RealScanSDK.RS_DEVICE_REALSCAN_F)
                    {
                        ResetLCDEnable = false;
                        DisplayLCDEnable = false;
                    }

                    MsgPanel.Text = "El dispositivo se ha inicializado correctamente";
                }));
            });
        }

        private void Exit_DeviceHandler(object sender, FormClosedEventArgs e)
        {
            Exit_Device();
        }

        private void Exit_Device()
        {
            if (!m_prevStopped)
            {

                m_prevStopped = true;
            }

            m_result = RealScanSDK.RS_ExitDevice(deviceHandle);

            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }
            CaptureMode.SelectedIndex = 0;
            CallbackSelectedIndex = 0;

            DeviceInfo.Text = "";
            ImageSize = "";
            StartCapture.Enabled = false;
            StopCapture.Enabled = false;
            ResetLCDEnable = false;
            DisplayLCDEnable = false;

            ResetLCDEnable = true;
            DisplayLCDEnable = true;

            MsgPanel.Text = "El dispisitivo se desconectó correctamente";
        }

        public void ChangeCaptureMode(int modeIndex)
        {
            if (modeIndex >= 0 && modeIndex < CaptureMode.Items.Count)
            {
                CaptureMode.SelectedIndex = modeIndex;
                CaptureMode_SelectedIndexChanged(null, null);
            }
        }

        public void MarkCheckMissingFinger(MissingFingers missingFingers)
        {
            for (int i = 0; i < leftFingers.Items.Count; i++)
            {
                leftFingers.SetItemChecked(i, false);
            }
            for (int i = 0; i < rightFingers.Items.Count; i++)
            {
                rightFingers.SetItemChecked(i, false);
            }

            if (missingFingers.LeftFinger != null)
            {
                foreach (var index in missingFingers.LeftFinger)
                {
                    if (index >= 0 && index < leftFingers.Items.Count)
                    {
                        leftFingers.SetItemChecked(index, true);
                    }
                }
            }

            if (missingFingers.RightFinger != null)
            {
                foreach (var index in missingFingers.RightFinger)
                {
                    if (index >= 0 && index < rightFingers.Items.Count)
                    {
                        rightFingers.SetItemChecked(index, true);
                    }
                }
            }
        }

        private void CaptureMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            int[] tmpCaptureModes = new int[4] {
                RealScanSDK.RS_CAPTURE_DISABLED,
                RealScanSDK.RS_CAPTURE_FLAT_LEFT_FOUR_FINGERS,
                RealScanSDK.RS_CAPTURE_FLAT_RIGHT_FOUR_FINGERS,
                RealScanSDK.RS_CAPTURE_FLAT_TWO_FINGERS
            };

            for (int i = 0; i < tmpCaptureModes.Length; i++)
            {
                if (i == CaptureMode.SelectedIndex)
                {
                    m_captureMode = tmpCaptureModes[i];
                    break;
                }
            }

            if (m_captureMode == RealScanSDK.RS_CAPTURE_DISABLED)
            {
                MsgPanel.Text = "Por favor, seleccione un modo de captura.";
                StartCapture.Enabled = false;
                return;
            }
            else
            {
                switch (m_captureMode)
                {
                    case RealScanSDK.RS_CAPTURE_FLAT_LEFT_FOUR_FINGERS:
                        MsgPanel.Text = "4 Dedos de Mano Izquierda seleccionados.";
                        StartCapture.Enabled = true;
                        break;
                    case RealScanSDK.RS_CAPTURE_FLAT_RIGHT_FOUR_FINGERS:
                        MsgPanel.Text = "4 Dedos de Mano Derecha seleccionados.";
                        StartCapture.Enabled = true;
                        break;
                    case RealScanSDK.RS_CAPTURE_FLAT_TWO_FINGERS:
                        MsgPanel.Text = "2 Dedos Pulgares seleccionados.";
                        StartCapture.Enabled = true;
                        break;
                    default:
                        MsgPanel.Text = "Por favor, seleccione un modo de captura.";
                        StartCapture.Enabled = false;
                        break;
                }
            }

            switch (m_captureMode)
            {
                case RealScanSDK.RS_CAPTURE_FLAT_TWO_FINGERS:
                    m_slapType = RealScanSDK.RS_SLAP_TWO_FINGER;
                    m_fingerCount = 2;
                    break;
                case RealScanSDK.RS_CAPTURE_FLAT_LEFT_FOUR_FINGERS:
                    m_slapType = RealScanSDK.RS_SLAP_LEFT_FOUR;
                    m_fingerCount = 4;
                    break;
                case RealScanSDK.RS_CAPTURE_FLAT_RIGHT_FOUR_FINGERS:
                    m_slapType = RealScanSDK.RS_SLAP_RIGHT_FOUR;
                    m_fingerCount = 4;
                    break;

                default:
                    break;
            }

            int[] nCaptDir = new int[1] { RealScanSDK.RS_CAPTURE_DIRECTION_DEFAULT };

            m_result = RealScanSDK.RS_SetCaptureModeWithDir(deviceHandle, m_captureMode, nCaptDir[m_captureDir], 0, true);

            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                CaptureMode.SelectedIndex = 0;
                m_bCaptureModeSelected = false;
                return;
            }

            ImageSize = "";

            if (CaptureMode.SelectedIndex == 0) return;

            int imageWidth = 0;
            int imageHeight = 0;

            m_result = RealScanSDK.RS_GetImageSize(deviceHandle, ref imageWidth, ref imageHeight);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                m_bCaptureModeSelected = false;
                return;
            }

            m_bCaptureModeSelected = true;
            ImageSize = imageWidth.ToString() + "x" + imageHeight.ToString();
            StartCapture.Enabled = true;

            _selectedPrevMode = PrevMode.callbackDraw;
            SetPreview();
        }

        private void previewCallbackInt(int errorCode, byte[] imageData, int imageWidth, int imageHeight)
        {
            log("previewCallbackInt called...");
            int nWidth = imageWidth;
            int nHeight = imageHeight;
            int nPitch = nWidth % 4;

            if (nWidth % 4 != 0) nWidth -= nWidth % 4;

            byte[] bData = new byte[nWidth * nHeight];

            for (int i = 0; i < nHeight; i++)
            {
                for (int j = 0; j < nWidth; j++)
                    bData[i * nWidth + j] = imageData[i * imageWidth + j];
            }

            Bitmap Canvas = new Bitmap(nWidth, nHeight, PixelFormat.Format8bppIndexed);
            BitmapData CanvasData = Canvas.LockBits(new Rectangle(0, 0, nWidth, nHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            Marshal.Copy(bData, 0, CanvasData.Scan0, bData.Length);

            Canvas.UnlockBits(CanvasData);

            ColorPalette grayscalePalette = Canvas.Palette;
            for (int i = 0; i < 256; i++)
                grayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
            Canvas.Palette = grayscalePalette;

            PreviewWindow.Image = Canvas;

            log("previewCallbackInt done...");
        }

        private void previewDataCallback(int deviceHandle, int errorCode, IntPtr imageData, int imageWidth, int imageHeight)
        {
            log("previewDataCallback called....");
            if (imageData != null)
            {
                int prevImageWidth = imageWidth;
                int prevImageHeight = imageHeight;
                byte[] prevImageData = new byte[imageWidth * imageHeight];
                Marshal.Copy(imageData, prevImageData, 0, imageWidth * imageHeight);

                RSRawPreviewCallback callback = new RSRawPreviewCallback(previewCallbackInt);
                Invoke(callback, errorCode, prevImageData, prevImageWidth, prevImageHeight);
            }
            log("previewDataCallback done....");
        }

        private void SegmentSaveImageCaptureProcess(IntPtr imageData, int imageWidth, int imageHeight, int numOfFingers, RSSlapInfoArray slapInfo, IntPtr[] ImageBuffer, int[] ImageWidth, int[] ImageHeight)
        {
            BlobsImages blobsImages = new BlobsImages() { Blobs = new Dictionary<string, ImageBlob>() };

            string mainBlobKey = "";

            switch (CaptureMode.SelectedIndex)
            {
                case 1:
                    mainBlobKey = "mano_izquierda";
                    break;
                case 2:
                    mainBlobKey = "mano_derecha";
                    break;
                case 3:
                    mainBlobKey = "pulgares";
                    break;
                default:
                    break;
            }

            blobsImages.Blobs.Add(mainBlobKey, new ImageBlob
            {
                ImageData = getBlobCapture(imageData, imageWidth, imageHeight),
                ImageWidth = imageWidth,
                ImageHeight = imageHeight
            });


            for (int i = 0; i < numOfFingers; i++)
            {

                string fingerKey = "dedo_" + slapInfo.RSSlapInfoA[i].fingerType;
                blobsImages.Blobs.Add(fingerKey, new ImageBlob
                {
                    ImageData = getBlobCapture(ImageBuffer[i], ImageWidth[i], ImageHeight[i]),
                    ImageWidth = ImageWidth[i],
                    ImageHeight = ImageHeight[i]
                });
            }

            string json = JsonConvert.SerializeObject(blobsImages.Blobs);
            webSocketServer.SendMessageToLocalClient(json);
        }

        private byte[] getBlobCapture(IntPtr imageData, int imageWidth, int imageHeight)
        {
            int size = imageWidth * imageHeight;
            byte[] prevImageData = new byte[size];
            Marshal.Copy(imageData, prevImageData, 0, imageWidth * imageHeight);
            return prevImageData;
        }

        private void SegmentCaptureProcess(IntPtr imageData, int imageWidth, int imageHeight, int deviceHandle, ref RSSlapInfoArray slapInfo,
                                           ref int numOfFingers, ref IntPtr[] ImageBuffer, ref int[] ImageWidth, ref int[] ImageHeight)
        {
            RSSlapInfoArray slapInfoA = new RSSlapInfoArray();
            IntPtr slapInfoArray;

            int captureMode = 0;
            int captureOption = 0;
            int slapType = 1;

            for (int i = 0; i < 4; i++)
            {
                ImageBuffer[i] = (IntPtr)0;
                ImageWidth[i] = 0;
                ImageHeight[i] = 0;
            }

            int _size = Marshal.SizeOf(typeof(RSSlapInfoArray));
            slapInfoArray = Marshal.AllocHGlobal(_size);
            Marshal.StructureToPtr(slapInfoA, slapInfoArray, true);

            int fingerType = 0;
            int[] missingFingerArray = new int[] { 0, 0, 0, 0 };

            int n = 0;
            if (m_captureDir != RealScanSDK.RS_CAPTURE_DIRECTION_DEFAULT)
            {
                int captureDir = RealScanSDK.RS_CAPTURE_DIRECTION_DEFAULT;
                m_result = RealScanSDK.RS_GetCaptureModeWithDir(deviceHandle, ref captureMode, ref captureDir, ref captureOption);
            }
            else
            {
                m_result = RealScanSDK.RS_GetCaptureMode(deviceHandle, ref captureMode, ref captureOption);
            }

            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            switch (captureMode)
            {
                case RealScanSDK.RS_CAPTURE_FLAT_TWO_FINGERS:
                    slapType = RealScanSDK.RS_SLAP_TWO_FINGER;
                    break;
                case RealScanSDK.RS_CAPTURE_FLAT_LEFT_FOUR_FINGERS:
                    slapType = RealScanSDK.RS_SLAP_LEFT_FOUR;
                    for (int i = 0; i < 4; i++)
                    {
                        if (leftFingers.GetItemChecked(i))
                        {
                            missingFingerArray[n++] = RealScanSDK.RS_FGP_LEFT_LITTLE - i;
                        }
                    }
                    fingerType = RealScanSDK.RS_FGP_LEFT_LITTLE;
                    break;
                case RealScanSDK.RS_CAPTURE_FLAT_RIGHT_FOUR_FINGERS:
                    slapType = RealScanSDK.RS_SLAP_RIGHT_FOUR;
                    for (int i = 0; i < 4; i++)
                    {
                       if (rightFingers.GetItemChecked(i))
                            missingFingerArray[n++] = i + RealScanSDK.RS_FGP_RIGHT_INDEX;
                    }
                    fingerType = RealScanSDK.RS_FGP_RIGHT_INDEX;
                    break;
                default:
                    MsgPanel.Text = "Cannot segment in this mode";
                    return;
            }

            m_result = RealScanSDK.RS_Segment4(imageData, imageWidth, imageHeight, slapType, ref numOfFingers, ref slapInfoArray, ref ImageBuffer[0], ref ImageWidth[0],
                                                ref ImageHeight[0], ref ImageBuffer[1], ref ImageWidth[1], ref ImageHeight[1], ref ImageBuffer[2], ref ImageWidth[2],
                                                ref ImageHeight[2], ref ImageBuffer[3], ref ImageWidth[3], ref ImageHeight[3]);

            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            MsgPanel.Text = "Quality:";

            slapInfoA = (RSSlapInfoArray)Marshal.PtrToStructure(slapInfoArray, typeof(RSSlapInfoArray));
            if (slapInfoArray != (IntPtr)0)
            {
                RealScanSDK.RS_FreeImageData(slapInfoArray);
            }

            int overlayHandle = -1;
            int j = 0;

            for (int i = 0; i < numOfFingers; i++)
            {
                if (slapInfoA.RSSlapInfoA[i].fingerType == RealScanSDK.RS_FGP_UNKNOWN)
                {
                    if (slapType == RealScanSDK.RS_SLAP_LEFT_FOUR)
                    {
                        
                        while (fingerType == missingFingerArray[j])
                        {
                            fingerType--;
                            j++;
                        }

                        slapInfoA.RSSlapInfoA[i].fingerType = fingerType--;
                    }
                    else if (slapType == RealScanSDK.RS_SLAP_RIGHT_FOUR)
                    {
                        while (fingerType == missingFingerArray[j])
                        {
                            fingerType++;
                            j++;
                        }

                        slapInfoA.RSSlapInfoA[i].fingerType = fingerType++;
                    }
                }

                slapInfo = slapInfoA;

                RealScan.RSOverlayQuadrangle quad = new RSOverlayQuadrangle();
                quad.pos = new RSPoint[4];
                quad.color = 0x00ff0000;

                RSRect rect = new RSRect();
                RealScanSDK.GetClientRect(PreviewWindow.Handle, ref rect);

                quad.pos[0].x = slapInfoA.RSSlapInfoA[i].fingerPosition[0].x * rect.right / imageWidth;
                quad.pos[0].y = slapInfoA.RSSlapInfoA[i].fingerPosition[0].y * rect.bottom / imageHeight;
                quad.pos[1].x = slapInfoA.RSSlapInfoA[i].fingerPosition[1].x * rect.right / imageWidth;
                quad.pos[1].y = slapInfoA.RSSlapInfoA[i].fingerPosition[1].y * rect.bottom / imageHeight;
                quad.pos[2].x = slapInfoA.RSSlapInfoA[i].fingerPosition[3].x * rect.right / imageWidth;
                quad.pos[2].y = slapInfoA.RSSlapInfoA[i].fingerPosition[3].y * rect.bottom / imageHeight;
                quad.pos[3].x = slapInfoA.RSSlapInfoA[i].fingerPosition[2].x * rect.right / imageWidth;
                quad.pos[3].y = slapInfoA.RSSlapInfoA[i].fingerPosition[2].y * rect.bottom / imageHeight;

                m_result = RealScanSDK.RS_AddOverlayQuadrangle(deviceHandle, ref quad, ref overlayHandle);
                m_result = RealScanSDK.RS_ShowOverlay(overlayHandle, true);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    MsgPanel.Text = "Cannot overlay for quadrangle" + m_result;
                    return;
                }
            }

            for (int i = 0; i < numOfFingers; i++)
            {
                MsgPanel.Text += "[" + slapInfoA.RSSlapInfoA[i].fingerType + ":" + slapInfoA.RSSlapInfoA[i].imageQuality + "] ";
            }
        }

        private void StartCapture_Click(object sender, EventArgs e)
        {
            CaptureMode_SelectedIndexChanged(null, null);

            if (!m_bCaptureModeSelected)
            {
                MsgPanel.Text = "El modo de captura no se ha seleccionado correctamente";
                return;
            }

            MsgPanel.Text = "Coloque los dedos en el sensor";

            this.autoCaptureThread = new Thread(new ThreadStart(this.DoautoCapture));
            StartCapture.Enabled = false;
            StopCapture.Enabled = true;
            this.autoCaptureThread.Start();
        }

        private void StopCapture_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_AbortCapture(deviceHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }
            StartCapture.Enabled = true;
            StopCapture.Enabled = false;
        }


        private void DoautoCapture()
        {
            const int maxRetries = 3;
            int retryCount = 0;
            bool captureSuccess = false;

            while (!captureSuccess && retryCount < maxRetries)
            {
                m_result = RealScanSDK.RS_RemoveAllOverlay(deviceHandle);

                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    ShowMessageToUser("Error al remover overlays. Por favor, reinicie el dispositivo y vuelva a intentarlo.");
                    break;
                }

                m_result = RealScanSDK.RS_TakeImageData(deviceHandle, 10000, ref capturedImageData, ref capturedImageWidth, ref capturedImageHeight);

                if (m_result == RealScanSDK.RS_SUCCESS)
                {
                    captureSuccess = true;
                }
                else if (m_result == RealScanSDK.RS_ERR_SENSOR_DIRTY)
                {
                    ShowMessageToUser("El sensor está sucio. Por favor, límpielo antes de intentar nuevamente.");
                    break;
                }
                else if (m_result == RealScanSDK.RS_ERR_FINGER_EXIST)
                {
                    ShowMessageToUser("Por favor, retire su dedo del sensor antes de intentar la captura nuevamente.");
                    break;
                }
                else if (m_result == RealScanSDK.RS_ERR_CAPTURE_ABORTED)
                {
                    ShowMessageToUser("Captura cancelada. Puede intentar de nuevo.");
                    return;
                }
                else
                {
                    retryCount++;
                    ShowMessageToUser($"Error en la captura. Reintentando... {retryCount}/{maxRetries}");
                    Thread.Sleep(1000);
                }
            }

            if (captureSuccess)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    captureProcess(m_result);
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MsgPanel.Text = "No se pudo completar la captura. Intenta de nuevo.";
                });
            }
        }


        private void ShowMessageToUser(string message)
        {
            this.Invoke((MethodInvoker)delegate
            {
                MsgPanel.Text = message;
            });
        }

        private void captureProcess(int captureResult)
        {
            if (captureResult != RealScanSDK.RS_SUCCESS)
            {
                MsgPanel.Text = m_errorMsg + " Por favor, verifique el dispositivo y los dedos, luego intente de nuevo.";
                StartCapture.Enabled = true;
                StopCapture.Enabled = false;
                return;
            }

            if ((CallbackSelectedIndex == (int)callbackMode.saveNseg))
            {

                int numOfFingers = 0;
                IntPtr[] ImageBuffer = new IntPtr[4];
                int[] ImageWidth = new int[4];
                int[] ImageHeight = new int[4];
                RSSlapInfoArray slapInfoA = new RSSlapInfoArray();

                SegmentCaptureProcess(capturedImageData, capturedImageWidth, capturedImageHeight, deviceHandle, ref slapInfoA, ref numOfFingers, ref ImageBuffer, ref ImageWidth, ref ImageHeight);

                if (CallbackSelectedIndex == (int)callbackMode.saveNseg)
                {
                    SegmentSaveImageCaptureProcess(capturedImageData, capturedImageWidth, capturedImageHeight, numOfFingers, slapInfoA, ImageBuffer, ImageWidth, ImageHeight);
                }
                for (int i = 0; i < 4; i++)
                {
                    if (ImageBuffer[i] != (IntPtr)0)
                    {
                        RealScanSDK.RS_FreeImageData(ImageBuffer[i]);
                    }
                }
            }

            if (capturedImageData != (IntPtr)0)
            {
                RealScanSDK.RS_FreeImageData(capturedImageData);
            }

            StartCapture.Enabled = true;
            StopCapture.Enabled = false;
        }

        private void ResetLCD()
        {
            m_result = RealScanSDK.RS_ResetLCD(deviceHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void keypadCallback(int deviceHandle, uint keyCode)
        {
            if (KEYCALLBACKSelectedIndex == 1)
            {
                CALLBACKPRINTText = "Key Code Read:" + keyCode;
            }
            else if (KEYCALLBACKSelectedIndex == 2)
            {
                if (keyCode == 0x20)
                {
                    RealScanSDK.RS_Beep(deviceHandle, RealScanSDK.RS_BEEP_PATTERN_1);
                }
                else
                {
                    RealScanSDK.RS_Beep(deviceHandle, RealScanSDK.RS_BEEP_PATTERN_2);
                }
            }
            this.Update();
        }

        private void keypadCallbackInt(int deviceHandle, uint keyCode)
        {
            RSKeypadCallbackInt callback = new RSKeypadCallbackInt(keypadCallback);
            Invoke(callback, deviceHandle, keyCode);
        }

        private void ActiveKey_Click(object sender, EventArgs e)
        {
            uint m_keyMAsk = 0;

            RSKeypadCallback keypadCallback = new RSKeypadCallback(keypadCallbackInt);
            uint[] keymaskbuffer = new uint[9] { RealScanSDK.RS_REALSCANF_NO_KEY, RealScanSDK.RS_REALSCANF_UP_KEY, RealScanSDK.RS_REALSCANF_DOWN_KEY,
                                                 RealScanSDK.RS_REALSCANF_LEFT_KEY, RealScanSDK.RS_REALSCANF_RIGHT_KEY, RealScanSDK.RS_REALSCANF_PLAY_KEY,
                                                 RealScanSDK.RS_REALSCANF_STOP_KEY, RealScanSDK.RS_REALSCANF_FOOTSWITCH, RealScanSDK.RS_REALSCANF_ALL_KEYS };
            for (int i = 0; i < 9; i++)
            {
                if (i == KeyMaskSelectedIndex)
                {
                    m_keyMAsk = keymaskbuffer[i];
                }
            }
            m_result = RealScanSDK.RS_SetActiveKey(deviceHandle, m_keyMAsk);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            RealScanSDK.RS_RegisterKeypadCallback(deviceHandle, keypadCallback);
        }

        private void SetPreview()
        {
            switch (this._selectedPrevMode)
            {
                case PrevMode.callbackDraw:
                    m_result = RealScanSDK.RS_RegisterPreviewCallback(deviceHandle, previewCallback);
                    if (m_result != RealScanSDK.RS_SUCCESS)
                    {
                        RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                        MsgPanel.Text = m_errorMsg;
                    }
                    break;
                default:
                    break;
            }
        }

        private void ModeLEDOff()
        {
            m_result = RealScanSDK.RS_SetModeLED(deviceHandle, RealScanSDK.RS_LED_OFF, false);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        public void log(string msg)
        {
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}


