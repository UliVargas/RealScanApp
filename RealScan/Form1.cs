using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using RealScan;
using System.Threading;
using Npgsql;
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
        bool m_prevStopped = true;

        bool m_bCaptureModeSelected = false;
        int m_captureMode = 0;
        int m_minCount = 0;
        int m_captureDir = 0;
        int m_slapType = 0;
        int m_fingerCount = 0;

        int m_nCustomSegWidth = 0;
        int m_nCustomSegHeight = 0;

        enum PrevMode
        {
            callbackDraw
        }

        enum callbackMode
        {
            none,
            saveNseg,
            // seqCheck
        }

        private PrevMode _selectedPrevMode;
        RSPreviewDataCallback previewCallback;
        RSAdvPreviewCallback advPreviewCallback;
        CaptureDataCallback rawCaptureCallback;
        AdvCaptureDataCallback advCaptureCallback;
        
        private Thread autoCaptureThread = null;
        delegate void afterAutoCaptureCallback(int captureResult);

        int capturedImageWidth;
        int capturedImageHeight;
        IntPtr capturedImageData;

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

        // WebSocketServerMain webSocketServer = new WebSocketServerMain();

        public MainForm()
        {
            InitializeComponent();
            this.Text = "RealScan";
            // InitializeWebSocketServer();

            this.Load += new EventHandler(MainForm_Load);
            this.FormClosed += Exit_DeviceHandler;

            Callback.SelectedIndex = 0;
            Finger_LED.SelectedIndex = 0;
            FingerColor.SelectedIndex = 0;
            Mode_LED.SelectedIndex = 0;
            TestType.SelectedIndex = 0;
            BeepPattern.SelectedIndex = 0;
            StatusLEDColor.SelectedIndex = 0;
            AutoSensitivity.SelectedIndex = 0;
            cbLFDLevel.SelectedIndex = 0;
            RollProfiles.SelectedIndex = 1;
            RollDirections.SelectedIndex = 2;
            SelectTextAlign.SelectedIndex = 0;
            SelectTextColor.SelectedIndex = 0;
            SelectTextSize.SelectedIndex = 0;
            SelectLineColor.SelectedIndex = 0;
            SelectCrossColor.SelectedIndex = 0;
            SelectQuadColor.SelectedIndex = 0;
            KeyMask.SelectedIndex = 0;
            KEYCALLBACK.SelectedIndex = 0;
            OverlayType.SelectedIndex = 0;
            OverlayColor.SelectedIndex = 0;
            CaptureMode.SelectedIndex = 0;
            AutoScroll = true;

            rawCaptureCallback = new CaptureDataCallback(captureDataCallback);
            
            previewCallback = new RSPreviewDataCallback(previewDataCallback);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Ininit_SDK();
        }
        private void InitializeWebSocketServer()
        {
            // webSocketServer.Start();
        }

        private void Ininit_SDK()
        {
            int numOfDevice = 0;
            RSSDKInfo sdkInfo = new RSSDKInfo();

            m_result = RealScanSDK.RS_InitSDK(null, 0, ref numOfDevice);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }

            m_result = RealScanSDK.RS_GetSDKInfo(ref sdkInfo);
            if (m_result == RealScanSDK.RS_SUCCESS)
            {
                SDKInfo.Text = System.Text.Encoding.ASCII.GetString(sdkInfo.version);
                SDKInfo.Text += " ";
                SDKInfo.Text += System.Text.Encoding.ASCII.GetString(sdkInfo.buildDate);
            }

            DeviceList.Items.Clear();

            MsgPanel.Text = "El SDK se inicializó correctamente";

            for (int i = 0; i < numOfDevice; i++)
            {
                String deviceName = "Device ";
                deviceName += i;
                DeviceList.Items.Add(deviceName);
            }

            if (numOfDevice > 0)
            {
                DeviceList.SelectedIndex = 0;
            }

            Init_Device();

        }

        private void Init_Device()
        {
            RSDeviceInfo deviceInfo = new RSDeviceInfo();

            m_result = RealScanSDK.RS_InitDevice(DeviceList.SelectedIndex, ref deviceHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }

            AutoCalibrate.Checked = true;
            
            m_prevStopped = true;

            TimeoutTextBox.Text = Convert.ToString(0);
            ReductionLevel.Text = Convert.ToString(100);

            m_result = RealScanSDK.RS_GetDeviceInfo(deviceHandle, ref deviceInfo);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }

            DeviceInfo.Text = System.Text.Encoding.ASCII.GetString(deviceInfo.productName);
            DeviceID.Text = System.Text.Encoding.ASCII.GetString(deviceInfo.deviceID);
            FirmwareInfo.Text = System.Text.Encoding.ASCII.GetString(deviceInfo.firmwareVersion);
            FirmwareInfo.Text = System.Text.Encoding.ASCII.GetString(deviceInfo.firmwareVersion);
            Hardwareinfo.Text = System.Text.Encoding.ASCII.GetString(deviceInfo.HardwareVersion);

            InitDevice.Enabled = false;
            ExitDevice.Enabled = true;

            if (deviceInfo.deviceType != RealScanSDK.RS_DEVICE_REALSCAN_F)
            {
                ResetLCD.Enabled = false;
                DisplayLCD.Enabled = false;
            }

            MsgPanel.Text = "El dispositivo se ha inicializado correctamente";
        }

        private void exitDevice_Click(object sender, EventArgs e)
        {
            Exit_Device();
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
            Callback.SelectedIndex = 0;

            DeviceInfo.Text = "";
            FirmwareInfo.Text = "";
            DeviceID.Text = "";
            FirmwareInfo.Text = "";
            Hardwareinfo.Text = "";
            ImageSize.Text = "";
            InitDevice.Enabled = true;
            ExitDevice.Enabled = false;
            StartCapture.Enabled = false;
            StopCapture.Enabled = false;
            ResetLCD.Enabled = false;
            DisplayLCD.Enabled = false;

            ResetLCD.Enabled = true;
            DisplayLCD.Enabled = true;

            MsgPanel.Text = "El dispisitivo se desconectó correctamente";
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

            int[] nCaptDir = new int[1] { RealScanSDK.RS_CAPTURE_DIRECTION_DEFAULT};

            m_result = RealScanSDK.RS_SetCaptureModeWithDir(deviceHandle, m_captureMode, nCaptDir[m_captureDir], 0, true);

            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                CaptureMode.SelectedIndex = 0;
                m_bCaptureModeSelected = false;
                return;
            }

            ImageSize.Text = "";

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
            ImageSize.Text = imageWidth.ToString() + "x" + imageHeight.ToString();
            StartCapture.Enabled = true;

            _selectedPrevMode = PrevMode.callbackDraw;
            SetPreview();
        }

        private void previewCallbackInt(int errorCode, byte[] imageData, int imageWidth, int imageHeight)
        {
            log("previewCallbackInt called...");
            int nWidth = imageWidth;
            int nHeight = imageHeight;
            int nPitch = nWidth%4;

            if (nWidth % 4 != 0) nWidth -= nWidth % 4;
            
            byte[] bData = new byte[nWidth*nHeight];

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

		private int GetSlapType(int nCaptureMode, ref int pnSlapType)
        {
            switch (nCaptureMode)
            {
                case RealScanSDK.RS_CAPTURE_FLAT_TWO_FINGERS:
                    break;
                case RealScanSDK.RS_CAPTURE_FLAT_LEFT_FOUR_FINGERS:
                    break;
                case RealScanSDK.RS_CAPTURE_FLAT_RIGHT_FOUR_FINGERS:
                    break;
                default:
                    MsgPanel.Text = "Cannot segment in this mode";
                    return - 1;
            }
            return 0;
        }        

private void captureCallbackInt(int errorCode, IntPtr imageData, int imageWidth, int imageHeight)
        {
            int nRetVal = RealScanSDK.RS_ERR_UNKNOWN;
            m_result = errorCode;
            if (errorCode != RealScanSDK.RS_SUCCESS && errorCode != RealScanSDK.RS_WRN_TOO_POOR_QUALITY && errorCode != RealScanSDK.RS_WRN_BAD_SCAN
                && errorCode != RealScanSDK.RS_ERR_SEGMENT_FEWER_FINGER && errorCode != RealScanSDK.RS_ERR_SEGMENT_WRONG_HAND)
            {
                RealScanSDK.RS_GetErrString(errorCode, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                StartCapture.Enabled = true;
                StopCapture.Enabled = false;
                return;
            }

            int nLFDLevel = 0;
            RealScanSDK.RS_GetLFDLevel(deviceHandle,ref nLFDLevel);
            if(nLFDLevel == 0)
            {
                MsgPanel.Text = "La imagen se capturó correctamente";
            }
            else
            {
                RSLFDResult sLFDResult = new RSLFDResult();
                nRetVal = RealScanSDK.RS_GetLFDResult(deviceHandle, ref sLFDResult);
                if(nRetVal != RealScanSDK.RS_SUCCESS)
                {
                    MsgPanel.Text = "La imagen se capturó correctamente";
                }
                else
                {
                    int nFinalLFDResult = RealScanSDK.RS_LFD_LIVE;
                    String strScores = "";
                    for (int i = 0; i < sLFDResult.nNumofFinger; i++)
                    {
                        String strScore = "";
                        if (sLFDResult.arsLFDInfo[i].nResult == RealScanSDK.RS_LFD_FAKE) nFinalLFDResult = RealScanSDK.RS_LFD_FAKE;
                        strScore = String.Format("{0}({1}) ", sLFDResult.arsLFDInfo[i].nResult == RealScanSDK.RS_LFD_LIVE ? "L" : "F",
                            sLFDResult.arsLFDInfo[i].nScore);
                        strScores += strScore;
                    }
                    MsgPanel.Text = String.Format("{0} {1}", nFinalLFDResult == RealScanSDK.RS_LFD_LIVE ? "[LIVE FINGER]" :  "[FAKE FINGER]", strScores);
                }
            }

            if ((Callback.SelectedIndex == (int)callbackMode.saveNseg) && CaptureMode.SelectedIndex != RealScanSDK.RS_CAPTURE_ROLL_FINGER)
            {
                int nMinFinger = 4;
                nRetVal = RealScanSDK.RS_GetMinimumFinger(deviceHandle, ref nMinFinger);
                if(nMinFinger == 4)
                {
                    int numOfFingers = 0;
                    IntPtr[] ImageBuffer = new IntPtr[4];
                    int[] ImageWidth = new int[4];
                    int[] ImageHeight = new int[4];
                    RSSlapInfoArray slapInfoA = new RSSlapInfoArray();

                    SegmentCaptureProcess(imageData, imageWidth, imageHeight, deviceHandle, ref slapInfoA, ref numOfFingers, ref ImageBuffer, ref ImageWidth, ref ImageHeight);

                    if (Callback.SelectedIndex == (int)callbackMode.saveNseg)
                    {
                        SegmentSaveImageCaptureProcess(imageData, imageWidth, imageHeight, numOfFingers, slapInfoA, ImageBuffer, ImageWidth, ImageHeight);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (ImageBuffer[i] != (IntPtr)0)
                        {
                            RealScanSDK.RS_FreeImageData(ImageBuffer[i]);
                        }
                    }
                }
                else
                {
                    int nCaptureMode = 0;
                    int nCaptureOption = 0;
                    nRetVal = RealScanSDK.RS_GetCaptureMode(deviceHandle, ref nCaptureMode, ref nCaptureOption);
                    if (nRetVal != RealScanSDK.RS_SUCCESS) return;

                    int nSlapType = 0;
                    nRetVal = GetSlapType(nCaptureMode, ref nSlapType);
                    if(nRetVal == RealScanSDK.RS_SUCCESS)
                    {
                        RealScanSDK.RSSegmentInfo sSegmentInfo = new RealScanSDK.RSSegmentInfo();
                        sSegmentInfo.arsFingerInfo = new RealScanSDK.RSFingerInfo[4];
                        for (int i = 0; i < 4; i++)
                        {
                            sSegmentInfo.arsFingerInfo[i].arsPoint = new RSPoint[4];
                            sSegmentInfo.arsFingerInfo[i].nWidth = 1000;
                            sSegmentInfo.arsFingerInfo[i].nHeight = 1000;
                        }

                        byte[] pbyFinImg1 = new byte[1000*1000];
                        byte[] pbyFinImg2 = new byte[1000*1000];
                        byte[] pbyFinImg3 = new byte[1000*1000];
                        byte[] pbyFinImg4 = new byte[1000*1000];
                        
                        RealScanSDK.RSMissingInfo sMissingInfo = new RealScanSDK.RSMissingInfo();
                        if(nSlapType == RealScanSDK.RS_SLAP_LEFT_FOUR)
                        {
                            if (leftFingers.GetItemChecked(0)) sMissingInfo.nFirstfinger = 1;
                            if (leftFingers.GetItemChecked(1)) sMissingInfo.nSecondfinger = 1;
                            if (leftFingers.GetItemChecked(2)) sMissingInfo.nThirdfinger = 1;
                            if (leftFingers.GetItemChecked(3)) sMissingInfo.nFourthfinger = 1;                            
                        }
                        else
                        {
                            if (rightFingers.GetItemChecked(0)) sMissingInfo.nFirstfinger = 1;
                            if (rightFingers.GetItemChecked(1)) sMissingInfo.nSecondfinger = 1;
                            if (rightFingers.GetItemChecked(2)) sMissingInfo.nThirdfinger = 1;
                            if (rightFingers.GetItemChecked(3)) sMissingInfo.nFourthfinger = 1;
                        }
                        nRetVal = RealScanSDK.RS_SegmentExMissingFinger(imageData, imageWidth, imageHeight, nSlapType, ref sSegmentInfo,
                        pbyFinImg1, pbyFinImg3, pbyFinImg3, pbyFinImg4, sMissingInfo);

                        if (nRetVal == RealScanSDK.RS_SUCCESS)
                        {
                            MsgPanel.Text = "Quality:";
                            for(int i=0; i< sSegmentInfo.nFingerCnt; i++)
                            {
                                MsgPanel.Text += "[" + sSegmentInfo.arsFingerInfo[i].nFingerType + ":" + sSegmentInfo.arsFingerInfo[i].nImageQuality + "] ";
                            }
                        }
                        else
                        {
                            RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                            MsgPanel.Text = m_errorMsg;
                        }
                    }
                }
            }

            StartCapture.Enabled = true;
            StopCapture.Enabled = false;
        }

        private void captureDataCallback(int deviceHandle, int errorCode, IntPtr imageData, int imageWidth, int imageHeight)
        {
            if (imageData != null)
            {
                RSRawCaptureCallback callback = new RSRawCaptureCallback(captureCallbackInt);
                Invoke(callback, errorCode, imageData, imageWidth, imageHeight);
            }
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
            ConvertIntPtrToBase64String(imageData, imageWidth, imageHeight);

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = ".:\\";
            saveDialog.FilterIndex = 1;

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                RealScanSDK.RS_SaveBitmap(imageData, imageWidth, imageHeight, saveDialog.FileName + ".bmp");
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }

                // ConvertIntPtrToBase64String(ImageBuffer[0], ImageWidth[0], ImageHeight[0]);

                for (int i = 0; i < numOfFingers; i++)
                {
                    // TODO: Agregar guardado a la base de datos
                    if (setSegmentSize.Checked)
                        RealScanSDK.RS_SaveBitmap(ImageBuffer[i], m_nCustomSegWidth, m_nCustomSegHeight, saveDialog.FileName + "_" + slapInfo.RSSlapInfoA[i].fingerType + ".bmp");
                    else
                        RealScanSDK.RS_SaveBitmap(ImageBuffer[i], ImageWidth[i], ImageHeight[i], saveDialog.FileName + "_" + slapInfo.RSSlapInfoA[i].fingerType + ".bmp");
                    if (m_result != RealScanSDK.RS_SUCCESS)
                    {
                        RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                        MsgPanel.Text = m_errorMsg;
                    }
                }
            }
            saveDialog.Dispose();
        }

        private void ConvertIntPtrToBase64String(IntPtr imageData, int imageWidth, int imageHeight)
        {
            string cadenaConexion = "Host=roundhouse.proxy.rlwy.net;Username=postgres;Password=A6C2Dg512G35D2C141ADef1d6Dg-*bgA;Database=railway;PORT=29265";


            try
            {

                int qualityScore = 0;
                m_result = RealScanSDK.RS_GetQualityScore(imageData, imageWidth, imageHeight, ref qualityScore);

                AllocateConsole();
                Console.WriteLine(m_result);

                if (m_result == RealScanSDK.RS_SUCCESS)
                {
                    AllocateConsole();
                    Console.WriteLine($"Score: {qualityScore}");
                }

                int prevImageWidth = imageWidth;
                int prevImageHeight = imageHeight;
                int size = imageWidth * imageHeight;
                byte[] prevImageData = new byte[size];
                Marshal.Copy(imageData, prevImageData, 0, imageWidth * imageHeight);


                using var conexion = new NpgsqlConnection(cadenaConexion);
                conexion.Open();
                MessageBox.Show("Conexión exitosa");

                string query = "INSERT INTO huellas(id, huella, width, height) VALUES(@id, @huella, @width, @height)";

                using var cmd = new NpgsqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("@huella", prevImageData);
                cmd.Parameters.AddWithValue("@width", prevImageWidth);
                cmd.Parameters.AddWithValue("@height", prevImageHeight);
                int filasAfectadas = cmd.ExecuteNonQuery();

                if (filasAfectadas > 0)
                {
                    MessageBox.Show("Registro insertado correctamente.");
                }
                else
                {
                    MessageBox.Show("No se insertó el registro.");
                }
                return;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar a la base de datos: " + ex.Message);
            }
            
        }

        public enum ImageFormat
        {
            Unknown,
            JPEG,
            PNG,
            GIF,
            BMP,
            TIFF
        }

        public static ImageFormat GetImageFormat(byte[] imageData)
        {
            // JPEG
            if (imageData.Length >= 3 && imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                return ImageFormat.JPEG;

            // PNG
            if (imageData.Length >= 8 && imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47 &&
                imageData[4] == 0x0D && imageData[5] == 0x0A && imageData[6] == 0x1A && imageData[7] == 0x0A)
                return ImageFormat.PNG;

            // GIF
            if (imageData.Length >= 6 && imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46 && imageData[3] == 0x38)
                return ImageFormat.GIF;

            // BMP
            if (imageData.Length >= 2 && imageData[0] == 0x42 && imageData[1] == 0x4D)
                return ImageFormat.BMP;

            // TIFF (little endian)
            if (imageData.Length >= 4 && imageData[0] == 0x49 && imageData[1] == 0x49 && imageData[2] == 0x2A && imageData[3] == 0x00)
                return ImageFormat.TIFF;

            // TIFF (big endian)
            if (imageData.Length >= 4 && imageData[0] == 0x4D && imageData[1] == 0x4D && imageData[2] == 0x00 && imageData[3] == 0x2A)
                return ImageFormat.TIFF;

            // Desconocido
            return ImageFormat.Unknown;
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
                            missingFingerArray[n++] = RealScanSDK.RS_FGP_LEFT_LITTLE - i;
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

            if (setSegmentSize.Checked)
            {
                m_nCustomSegWidth = Convert.ToInt32(SegWidth.Text);
                m_nCustomSegHeight = Convert.ToInt32(SegHeight.Text);

                m_result = RealScanSDK.RS_Segment4WithSize(imageData, imageWidth, imageHeight, slapType, ref numOfFingers, ref slapInfoArray,
                                                            ref ImageBuffer[0], ref ImageBuffer[1], ref ImageBuffer[2], ref ImageBuffer[3],
                                                            m_nCustomSegWidth, m_nCustomSegHeight);
            }
            else
            {
                m_result = RealScanSDK.RS_Segment4(imageData, imageWidth, imageHeight, slapType, ref numOfFingers, ref slapInfoArray, ref ImageBuffer[0], ref ImageWidth[0],
                                                 ref ImageHeight[0], ref ImageBuffer[1], ref ImageWidth[1], ref ImageHeight[1], ref ImageBuffer[2], ref ImageWidth[2],
                                                 ref ImageHeight[2], ref ImageBuffer[3], ref ImageWidth[3], ref ImageHeight[3]);
            }

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
                MsgPanel.Text = "Capture mode isn't selected successfully";
                return;
            }
            
            MsgPanel.Text = "Place fingers on the sensor";

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

        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }
   
        

        private void DoautoCapture()
        {
            m_result = RealScanSDK.RS_RemoveAllOverlay(deviceHandle);
            
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                if (MsgPanel.InvokeRequired)
                {
                    afterAutoCaptureCallback callback = new afterAutoCaptureCallback(captureProcess);
                    try
                    {
                        this.Invoke(callback, m_result);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }   
                return;
            }

            m_result = RealScanSDK.RS_TakeImageData(deviceHandle, 10000, ref capturedImageData, ref capturedImageWidth, ref capturedImageHeight);

            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                if (capturedImageData != (IntPtr)0)
                {
                    RealScanSDK.RS_FreeImageData(capturedImageData);
                }
            }

            if (MsgPanel.InvokeRequired)
            {
                afterAutoCaptureCallback callback = new afterAutoCaptureCallback(captureProcess);

                try
                {
                    this.Invoke(callback, m_result);
                }
                catch
                {
                }
            }
            else
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            return;
        }

        private void captureProcess(int captureResult)
        {
            if ((Callback.SelectedIndex == (int)callbackMode.saveNseg) && CaptureMode.SelectedIndex != RealScanSDK.RS_CAPTURE_ROLL_FINGER)
            {
                if (CaptureMode.SelectedIndex == 1 || CaptureMode.SelectedIndex == 6 || CaptureMode.SelectedIndex == 7)
                {
                    MsgPanel.Text = "The command is not supported.";
                }
                else
                {
                    int numOfFingers = 0;
                    IntPtr[] ImageBuffer = new IntPtr[4];
                    int[] ImageWidth = new int[4];
                    int[] ImageHeight = new int[4];
                    RSSlapInfoArray slapInfoA = new RSSlapInfoArray();

                    

                    SegmentCaptureProcess(capturedImageData, capturedImageWidth, capturedImageHeight, deviceHandle, ref slapInfoA, ref numOfFingers, ref ImageBuffer, ref ImageWidth, ref ImageHeight);
                    
                    if (Callback.SelectedIndex == (int)callbackMode.saveNseg)
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
            }

            if (capturedImageData != (IntPtr)0)
            {
                RealScanSDK.RS_FreeImageData(capturedImageData);
            }

            StartCapture.Enabled = true;
            StopCapture.Enabled = false;
        }

        private void ResetLCD_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_ResetLCD(deviceHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void DisplayLCD_Click(object sender, EventArgs e)
        {
            Stream temp;
            OpenFileDialog openDialog = new OpenFileDialog();
            byte[] inputImage = new byte[RealScanSDK.RS_LCD_WIDTH_MAX * RealScanSDK.RS_LCD_HEIGHT_MAX];
            IntPtr outputImage = (IntPtr)0;

            openDialog.InitialDirectory = "c:\\";
            openDialog.Filter = "r08 files (*.r08)|*.r08";
            openDialog.FilterIndex = 1;
            openDialog.RestoreDirectory = true;

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                temp = openDialog.OpenFile();
                if (temp != null)
                {
                    temp.Read(inputImage, 0, RealScanSDK.RS_LCD_WIDTH_MAX * RealScanSDK.RS_LCD_HEIGHT_MAX);

                    m_errorcode = RealScanSDK.RS_MakeLCDData(inputImage, inputImage, inputImage, RealScanSDK.RS_LCD_WIDTH_MAX, RealScanSDK.RS_LCD_HEIGHT_MAX, ref outputImage);
                    if (m_result == RealScanSDK.RS_SUCCESS)
                    {
                        m_result = RealScanSDK.RS_DisplayLCD(deviceHandle, outputImage, RealScanSDK.RS_LCD_DATA_SIZE_MAX, 0, 0,
                                                             RealScanSDK.RS_LCD_WIDTH_MAX, RealScanSDK.RS_LCD_HEIGHT_MAX);

                        if (m_result == RealScanSDK.RS_SUCCESS)
                        {
                            MsgPanel.Text = "Display LCD is successfully done!!";
                        }
                        else
                        {
                            RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                            MsgPanel.Text = m_errorMsg;
                        }
                    }
                    else
                    {
                        RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                        MsgPanel.Text = m_errorMsg;
                    }
                    temp.Close();
                }
            }
        }

        private void FingerLEDON_Click(object sender, EventArgs e)
        {
            int m_FingerColor = 0x00;
            int m_FingerLED = 0x00;
            int[] Fingerbuffer = new int[11] { RealScanSDK.RS_FINGER_ALL, RealScanSDK.RS_FINGER_LEFT_LITTLE, RealScanSDK.RS_FINGER_LEFT_RING, 
                                                    RealScanSDK.RS_FINGER_LEFT_MIDDLE, RealScanSDK.RS_FINGER_LEFT_INDEX, RealScanSDK.RS_FINGER_LEFT_THUMB,
                                                    RealScanSDK.RS_FINGER_RIGHT_RING, RealScanSDK.RS_FINGER_RIGHT_LITTLE, RealScanSDK.RS_FINGER_TWO_THUMB,
                                                    RealScanSDK.RS_FINGER_LEFT_FOUR, RealScanSDK.RS_FINGER_RIGHT_FOUR };
            for (int i = 0; i < 3; i++)
            {
                if (i == Finger_LED.SelectedIndex)
                {
                    m_FingerLED = Fingerbuffer[i];
                }
            }

            int[] FingerbufferColor = new int[3] { RealScanSDK.RS_LED_GREEN, RealScanSDK.RS_LED_RED, RealScanSDK.RS_LED_YELLOW };

            for (int i = 0; i < 3; i++)
            {
                if (i == FingerColor.SelectedIndex)
                {
                    m_FingerColor = FingerbufferColor[i];
                }
            }

            m_result = RealScanSDK.RS_SetFingerLED(deviceHandle, m_FingerLED, m_FingerColor);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void FingerLEDOFF_Click(object sender, EventArgs e)
        {
            int m_FingerLED = Finger_LED.SelectedIndex;
            int[] Fingerbuffer = new int[11] { RealScanSDK.RS_FINGER_ALL, RealScanSDK.RS_FINGER_LEFT_LITTLE, RealScanSDK.RS_FINGER_LEFT_RING, 
                                                    RealScanSDK.RS_FINGER_LEFT_MIDDLE, RealScanSDK.RS_FINGER_LEFT_INDEX, RealScanSDK.RS_FINGER_LEFT_THUMB,
                                                    RealScanSDK.RS_FINGER_RIGHT_RING, RealScanSDK.RS_FINGER_RIGHT_LITTLE, RealScanSDK.RS_FINGER_TWO_THUMB,
                                                    RealScanSDK.RS_FINGER_LEFT_FOUR, RealScanSDK.RS_FINGER_RIGHT_FOUR };
            for (int i = 0; i < 3; i++)
            {
                if (i == Finger_LED.SelectedIndex)
                {
                    m_FingerLED = Fingerbuffer[i];
                }
            }

            m_result = RealScanSDK.RS_SetFingerLED(deviceHandle, m_FingerLED, RealScanSDK.RS_LED_OFF);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ModeLEDOff_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_SetModeLED(deviceHandle, RealScanSDK.RS_LED_OFF, false);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void TestRun_Click(object sender, EventArgs e)
        {
            int TestTypes = 0;
            int[] TestTypeBuffer = new int[2] { RealScanSDK.RS_SELFTEST_ILLUMINATION, RealScanSDK.RS_SELFTEST_DIRT };

            for (int i = 0; i < 2; i++)
            {
                if (i == TestType.SelectedIndex)
                {
                    TestTypes = TestTypeBuffer[i];
                }
            }

            m_result = RealScanSDK.RS_SelfTest(deviceHandle, TestTypes);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            else
            {
                MsgPanel.Text = "Device OK ";
            }
        }

        private void ManualCalibration_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_Calibrate(deviceHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            else
            {
                MsgPanel.Text = "Calibration succeed.";
            }
        }

        private void Emitbeep_Click(object sender, EventArgs e)
        {
            int Beeptype = 0;
            byte[] Beepbuffer = new byte[3] { RealScanSDK.RS_BEEP_PATTERN_NONE, RealScanSDK.RS_BEEP_PATTERN_1, RealScanSDK.RS_BEEP_PATTERN_2 };

            for (int i = 0; i < 3; i++)
            {
                if (i == BeepPattern.SelectedIndex)
                {
                    Beeptype = Beepbuffer[i];
                }
            }

            m_result = RealScanSDK.RS_Beep(deviceHandle, Beeptype);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void PlayWav_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            DialogResult dialogResult = openFileDialog.ShowDialog();
            string filename = " ";

            if (dialogResult == DialogResult.OK)
            {
                filename = openFileDialog.FileName;
                if (filename == String.Empty)
                {
                    return;
                }
            }

            m_result = RealScanSDK.RS_PlayWav(deviceHandle, filename);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ReadRollOption_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_GetRollFingerOption(deviceHandle, ref m_rollDirection, ref m_rollTime, ref  m_rollProfile);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            RollDirections.SelectedIndex = m_rollDirection;
            TimeoutTextBox.Text = Convert.ToString(m_rollTime);
            RollProfiles.SelectedIndex = m_rollProfile - 1;
        }

        private void ShowTextOverlay_Click(object sender, EventArgs e)
        {
            RealScan.RSOverlayText text = new RSOverlayText();

            if (m_TextX.Text == "" || m_TextY.Text == "")
            {
                MessageBox.Show("Please set all the text boxes filled");
                
                return;
            }
            
            text.pos.x = Convert.ToInt32(m_TextX.Text);
            text.pos.y = Convert.ToInt32(m_TextY.Text);
            text.alignment = SelectTextAlign.SelectedIndex;
            int[] FontSizebuffer = new int[9] { 8, 10, 12, 14, 16, 18, 20, 24, 28 };
            ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };

            for (int i = 0; i < 4; i++)
            {
                if (i == SelectTextColor.SelectedIndex)
                {
                    text.color = ColorBuffer[i];
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (i == SelectTextSize.SelectedIndex)
                {
                    text.fontSize = FontSizebuffer[i];
                }
            }

            int strLength = m_Text.Text.Length;

            byte[] textbuffer = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(m_Text.Text.ToCharArray()));
            int len = m_Text.Text.Length;
            byte[] textbuffer2 = new byte[128];
            textbuffer2[len] = 0;
            System.Buffer.BlockCopy(textbuffer, 0, textbuffer2, 0, len);
            text.text = textbuffer2;

            Font GetFontInfo = new Font("System", text.fontSize);
            int FontSize = Convert.ToInt32(GetFontInfo.Size);
            int FontLength = GetFontInfo.Name.Length;
            string GetFontNameBuffer = GetFontInfo.Name;
            byte[] Fontbuffer = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(GetFontNameBuffer));
            byte[] FontNames = new byte[32];
            System.Buffer.BlockCopy(Fontbuffer, 0, FontNames, 0, FontLength);
            text.fontName = FontNames;

            m_result = RealScanSDK.RS_AddOverlayText(deviceHandle, ref text, ref m_overlayHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            m_result = RealScanSDK.RS_ShowOverlay(m_overlayHandle, true);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void StatusLEDOn_Click(object sender, EventArgs e)
        {
            int m_StatusLEDColors = 0x00;
            int[] FingerbufferColor = new int[3] { RealScanSDK.RS_LED_GREEN, RealScanSDK.RS_LED_RED, RealScanSDK.RS_LED_YELLOW };

            for (int i = 0; i < 3; i++)
            {
                if (i == StatusLEDColor.SelectedIndex)
                {
                    m_StatusLEDColors = FingerbufferColor[i];
                }
            }

            m_result = RealScanSDK.RS_SetStatusLED(deviceHandle, m_StatusLEDColors);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void WriteOtherOption_Click(object sender, EventArgs e)
        {
            bool m_AutoCalibrate = AutoCalibrate.Checked;
            bool m_AdvContrast = AdvancedContrastEnhancement.Checked;
            bool m_ContrastEnhancement = ContrastEnhancement.Checked;
            bool m_NoiseReduction = NoiseReduction.Checked;
            int m_AutoSensitivity = 0x00;
            int m_ReductionLevel = 0;

            int[] SensitivityBuffer = new int[4] {RealScanSDK.RS_AUTO_SENSITIVITY_NORMAL,RealScanSDK.RS_AUTO_SENSITIVITY_HIGH,
                                                  RealScanSDK.RS_AUTO_SENSITIVITY_HIGHER,RealScanSDK. RS_AUTO_SENSITIVITY_DISABLED};
            for (int i = 0; i < 4; i++)
            {
                if (i == AutoSensitivity.SelectedIndex)
                {
                    m_AutoSensitivity = SensitivityBuffer[i];
                }
            }

            try
            {
                m_ReductionLevel = Convert.ToInt32(ReductionLevel.Text);
            }
            catch (InvalidCastException)
            {
                return;
            }

            m_result = RealScanSDK.RS_SetCaptureMode(deviceHandle, m_mode, m_AutoSensitivity, true);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            m_result = RealScanSDK.RS_SetAutomaticCalibrate(deviceHandle, m_AutoCalibrate);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            m_result = RealScanSDK.RS_SetAdvancedContrastEnhancement(deviceHandle, m_AdvContrast);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            m_result = RealScanSDK.RS_SetPostProcessingEx(deviceHandle, m_ContrastEnhancement, m_NoiseReduction, m_ReductionLevel);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ReadOtherOption_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_GetCaptureMode(deviceHandle, ref m_mode, ref m_option);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            AutoSensitivity.SelectedIndex = m_option;

            m_result = RealScanSDK.RS_GetAutomaticCalibrate(deviceHandle, ref m_automatic);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            AutoCalibrate.Checked = m_automatic;

            m_result = RealScanSDK.RS_GetAdvancedContrastEnhancement(deviceHandle, ref m_advancedContrast);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            AdvancedContrastEnhancement.Checked = m_advancedContrast;

            m_result = RealScanSDK.RS_GetPostProcessingEx(deviceHandle, ref m_contrastEnhancement, ref m_noiseReduction, ref m_reductionLevel);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            ContrastEnhancement.Checked = m_contrastEnhancement;
            NoiseReduction.Checked = m_noiseReduction;
            ReductionLevel.Text = Convert.ToString(m_reductionLevel);
        }

        private void DeviceTab_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics TabText = e.Graphics;
            Font TabTextFont = new Font(e.Font, FontStyle.Underline);

            StringFormat StringStyle = new StringFormat();
            StringStyle.Alignment = StringAlignment.Center;
            StringStyle.LineAlignment = StringAlignment.Center;

            TabText.DrawString("Device", TabTextFont, Brushes.Black, this.DeviceTab.GetTabRect(0), StringStyle);
            TabText.DrawString("I/O", TabTextFont, Brushes.Black, this.DeviceTab.GetTabRect(1), StringStyle);
            TabText.DrawString("Option", TabTextFont, Brushes.Black, this.DeviceTab.GetTabRect(2), StringStyle);
            TabText.DrawString("Overlay", TabTextFont, Brushes.Black, this.DeviceTab.GetTabRect(3), StringStyle);
            TabText.DrawString("Callback Option", TabTextFont, Brushes.Black, this.DeviceTab.GetTabRect(4), StringStyle);
        }

        private void WriteRollOption_Click(object sender, EventArgs e)
        {
            int RollProfile = 0x00;
            int RollDirection = 0x00;
            int RollTimeout = Convert.ToInt32(TimeoutTextBox.Text);

            int[] RollProfileBuffer = new int[3] { RealScanSDK.RS_ROLL_PROFILE_LOW, RealScanSDK.RS_ROLL_PROFILE_NORMAL, RealScanSDK.RS_ROLL_PROFILE_HIGH };
            for (int i = 0; i < 3; i++)
            {
                if (i == RollProfiles.SelectedIndex)
                {
                    RollProfile = RollProfileBuffer[i];
                }
            }

            int[] RollDirectionsBuffer = new int[4] { RealScanSDK.RS_ROLL_DIR_L2R, RealScanSDK.RS_ROLL_DIR_R2L, RealScanSDK.RS_ROLL_DIR_AUTO, RealScanSDK.RS_ROLL_DIR_AUTO_M };
            for (int i = 0; i < 3; i++)
            {
                if (i == RollDirections.SelectedIndex)
                {
                    RollDirection = RollDirectionsBuffer[i];
                }
            }
            m_result = RealScanSDK.RS_SetRollFingerOption(deviceHandle, RollDirection, RollTimeout, RollProfile);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ReadKeycallback_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_GetKeyStatus(deviceHandle, ref m_keyCode);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                KEYSTATUS.Text = " ";
                return;
            }
            KEYSTATUS.Text = Convert.ToString(m_keyCode);
        }

        private void keypadCallback(int deviceHandle, uint keyCode)
        {
            if (KEYCALLBACK.SelectedIndex == 1)
            {
                CALLBACKPRINT.Text = "Key Code Read:" + keyCode;
            }
            else if (KEYCALLBACK.SelectedIndex == 2)
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
                if (i == KeyMask.SelectedIndex)
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

        private void ShowLineOverlay_Click(object sender, EventArgs e)
        {
            RealScan.RSOverlayLine line = new RSOverlayLine();

            if (m_LineX1.Text == "" || m_LineY1.Text == "" || m_LineX2.Text == "" || m_LineY2.Text == "" || m_LineWidth.Text == "")
            {
                MessageBox.Show("Please set all the text boxes filled");

                return;
            }

            line.startPos.x = Convert.ToInt32(m_LineX1.Text);
            line.startPos.y = Convert.ToInt32(m_LineY1.Text);
            line.endPos.x = Convert.ToInt32(m_LineX2.Text);
            line.endPos.y = Convert.ToInt32(m_LineY2.Text);
            line.width = Convert.ToInt32(m_LineWidth.Text);

            ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };
            for (int i = 0; i < 4; i++)
            {
                if (i == SelectLineColor.SelectedIndex)
                {
                    line.color = ColorBuffer[i];
                }
            }

            m_result = RealScanSDK.RS_AddOverlayLine(deviceHandle, ref line, ref m_overlayHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }

            m_result = RealScanSDK.RS_ShowOverlay(m_overlayHandle, true);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ShowQuardangle_Click(object sender, EventArgs e)
        {
            int overlayHandle = -1;
            RealScan.RSOverlayQuadrangle quad = new RSOverlayQuadrangle();
            quad.pos = new RSPoint[4];

            if (m_QuadX1.Text == "" || m_QuadY1.Text == "" || m_QuadX2.Text == "" || m_QuadY2.Text == "" || 
                m_QuadX3.Text == "" || m_QuadY3.Text == "" || m_QuadX4.Text == "" || m_QuadY4.Text == "" ||
                m_QuadWidth.Text == "")
            {
                MessageBox.Show("Please set all the text boxes filled");

                return;
            }

            quad.pos[0].x = Convert.ToInt32(m_QuadX1.Text);
            quad.pos[0].y = Convert.ToInt32(m_QuadY1.Text);
            quad.pos[1].x = Convert.ToInt32(m_QuadX2.Text);
            quad.pos[1].y = Convert.ToInt32(m_QuadY2.Text);
            quad.pos[2].x = Convert.ToInt32(m_QuadX3.Text);
            quad.pos[2].y = Convert.ToInt32(m_QuadY3.Text);
            quad.pos[3].x = Convert.ToInt32(m_QuadX4.Text);
            quad.pos[3].y = Convert.ToInt32(m_QuadY4.Text);

            quad.width = Convert.ToInt32(m_QuadWidth.Text);

            ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };
            for (int i = 0; i < 4; i++)
            {
                if (i == SelectQuadColor.SelectedIndex)
                {
                    quad.color = ColorBuffer[i];
                }
            }

            m_result = RealScanSDK.RS_AddOverlayQuadrangle(deviceHandle, ref quad, ref overlayHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            m_result = RealScanSDK.RS_ShowOverlay(overlayHandle, true);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ShowCrossOverlay_Click(object sender, EventArgs e)
        {
            RealScan.RSOverlayCross cross = new RSOverlayCross();

            if (m_CrossX.Text == "" || m_CrossY.Text == "" || m_CrossDX.Text == "" || m_CrossDY.Text == "" || m_CrossWidth.Text == "")
            {
                MessageBox.Show("Please set all the text boxes filled");

                return;
            }

            cross.centerPos.x = Convert.ToInt32(m_CrossX.Text);
            cross.centerPos.y = Convert.ToInt32(m_CrossY.Text);

            ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };
            for (int i = 0; i < 4; i++)
            {
                if (i == SelectCrossColor.SelectedIndex)
                {
                    cross.color = ColorBuffer[i];
                }
            }

            cross.rangeX = Convert.ToInt32(m_CrossDX.Text);
            cross.rangeY = Convert.ToInt32(m_CrossDY.Text);
            cross.width = Convert.ToInt32(m_CrossWidth.Text);

            m_result = RealScanSDK.RS_AddOverlayCross(deviceHandle, ref cross, ref m_overlayHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            m_result = RealScanSDK.RS_ShowOverlay(m_overlayHandle, true);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void HideAll_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_ShowAllOverlay(deviceHandle, false);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ShowAll_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_ShowAllOverlay(deviceHandle, true);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ClearAll_Click(object sender, EventArgs e)
        {
            m_result = RealScanSDK.RS_RemoveAllOverlay(deviceHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            MsgPanel.Text = "Clear the Preview Window";
        }

        private void cbLFDLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nRetVal = RealScanSDK.RS_SetLFDLevel(deviceHandle, cbLFDLevel.SelectedIndex);
            if (nRetVal != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(nRetVal, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }

        }

        private void SetLFDLevel_Click(object sender, EventArgs e)
        {
            int nRetVal = RealScanSDK.RS_ERR_UNKNOWN;
            nRetVal = RealScanSDK.RS_SetLFDLevel(deviceHandle, cbLFDLevel.SelectedIndex);
            if (nRetVal != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(nRetVal, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }
        }

        private void ReadLFDLevel_Click(object sender, EventArgs e)
        {
            int nRetVal = RealScanSDK.RS_ERR_UNKNOWN;
            int nLFDLevel = 0;
            nRetVal = RealScanSDK.RS_GetLFDLevel(deviceHandle, ref nLFDLevel);
            if (nRetVal != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(nRetVal, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }

            cbLFDLevel.SelectedIndex = nLFDLevel;
        }

        private void DrawOverlay_Click(object sender, EventArgs e)
        {
            RSRect rect = new RSRect();
            RealScanSDK.GetClientRect(PreviewWindow.Handle, ref rect);

            if (OverlayType.SelectedIndex == 0)
            {
                RealScan.RSOverlayText text = new RSOverlayText();
                text.pos.x = 0;
                text.pos.y = 0;
                text.alignment = RealScanSDK.RS_TEXT_ALIGN_LEFT;
                text.fontSize = 24;

                ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };
                for (int i = 0; i < 4; i++)
                {
                    if (i == OverlayColor.SelectedIndex)
                    {
                        text.color = ColorBuffer[i];
                    }
                }

                string ShowText = "Test Message";
                byte[] textbuffer2 = new byte[128];
                textbuffer2 = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(ShowText));
                byte[] textbuffer3 = new byte[128];
                int Length = ShowText.Length;
                System.Buffer.BlockCopy(textbuffer2, 0, textbuffer3, 0, Length);
                text.text = textbuffer3;

                Font GetFontInfo = new Font("System", 24);
                int FontLength = GetFontInfo.Name.Length;
                string GetFontNameBuffer = GetFontInfo.Name;
                byte[] Fontbuffer = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, Encoding.Unicode.GetBytes(GetFontNameBuffer));
                byte[] FontNames = new byte[32];
                System.Buffer.BlockCopy(Fontbuffer, 0, FontNames, 0, FontLength);
                text.fontName = FontNames;

                m_result = RealScanSDK.RS_AddOverlayText(deviceHandle, ref text, ref m_overlayHandle);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
                m_result = RealScanSDK.RS_ShowOverlay(m_overlayHandle, true);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
            }
            else if (OverlayType.SelectedIndex == 1)
            {
                RealScan.RSOverlayCross cross = new RSOverlayCross();
                cross.centerPos.x = (rect.right - rect.left) / 2;
                cross.centerPos.y = (rect.bottom - rect.top) / 2;
                cross.rangeX = 10;
                cross.rangeY = 10;
                cross.width = 5;

                ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };
                for (int i = 0; i < 4; i++)
                {
                    if (i == OverlayColor.SelectedIndex)
                    {
                        cross.color = ColorBuffer[i];
                    }
                }

                m_result = RealScanSDK.RS_AddOverlayCross(deviceHandle, ref cross, ref m_overlayHandle);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
                m_result = RealScanSDK.RS_ShowOverlay(m_overlayHandle, true);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
            }
            else if (OverlayType.SelectedIndex == 2)
            {
                RealScan.RSOverlayLine line = new RSOverlayLine();

                line.startPos.x = rect.left;
                line.startPos.y = (rect.bottom - rect.top) / 2;
                line.endPos.x = rect.right;
                line.endPos.y = (rect.bottom - rect.top) / 2;
                line.width = 5;

                ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };
                for (int i = 0; i < 4; i++)
                {
                    if (i == OverlayColor.SelectedIndex)
                    {
                        line.color = ColorBuffer[i];
                    }
                }

                m_result = RealScanSDK.RS_AddOverlayLine(deviceHandle, ref line, ref m_overlayHandle);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
                m_result = RealScanSDK.RS_ShowOverlay(m_overlayHandle, true);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
            }
            else if (OverlayType.SelectedIndex == 3)
            {
                int overlayHandle = -1;
                RealScan.RSOverlayQuadrangle quad = new RSOverlayQuadrangle();
                quad.pos = new RSPoint[4];

                quad.pos[0].x = rect.left;
                quad.pos[0].y = rect.top;
                quad.pos[1].x = rect.right - 1;
                quad.pos[1].y = rect.top;
                quad.pos[2].x = rect.right - 1;
                quad.pos[2].y = rect.bottom - 1;
                quad.pos[3].x = rect.left;
                quad.pos[3].y = rect.bottom - 1;

                quad.width = 5;

                ulong[] ColorBuffer = new ulong[4] { 0x00000000, 0x000000ff, 0x0000ff00, 0x00ff0000 };
                for (int i = 0; i < 4; i++)
                {
                    if (i == OverlayColor.SelectedIndex)
                    {
                        quad.color = ColorBuffer[i];
                    }
                }

                m_result = RealScanSDK.RS_AddOverlayQuadrangle(deviceHandle, ref quad, ref overlayHandle);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
                m_result = RealScanSDK.RS_ShowOverlay(overlayHandle, true);
                if (m_result != RealScanSDK.RS_SUCCESS)
                {
                    RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                    MsgPanel.Text = m_errorMsg;
                }
            }
        }

        private void ClearAllOverlays_Click(object sender, EventArgs e)
        {
            int result = RealScanSDK.RS_RemoveAllOverlay(deviceHandle);
            if (result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void StatusLEDOff_Click(object sender, EventArgs e)
        {
            int result = RealScanSDK.RS_SetStatusLED(deviceHandle, RealScanSDK.RS_LED_OFF);

            if (result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void ModeLEDOn_Click(object sender, EventArgs e)
        {
            int LEDModeType = 0;
            int[] LEDModeTypeBuffer = new int[6]  {RealScanSDK.RS_LED_MODE_ALL,RealScanSDK.RS_LED_MODE_LEFT_FINGER4,RealScanSDK.RS_LED_MODE_RIGHT_FINGER4,
                                                      RealScanSDK.RS_LED_MODE_TWO_THUMB,RealScanSDK.RS_LED_MODE_ROLL,RealScanSDK.RS_LED_POWER};
            for (int i = 0; i < 6; i++)
            {
                if (i == Mode_LED.SelectedIndex)
                {
                    LEDModeType = LEDModeTypeBuffer[i];
                }
            }
            m_result = RealScanSDK.RS_SetModeLED(deviceHandle, LEDModeType, true);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
        }

        private void selMinFingerCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_minCount = 4 - selMinFingerCount.SelectedIndex;

            m_result = RealScanSDK.RS_SetMinimumFinger(deviceHandle, m_minCount);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
            }
            else
                MsgPanel.Text = "Setting the minimum finger count is done successfully";
        }

        private void selCaptureDir_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_captureDir = selCaptureDir.SelectedIndex;
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

        public void log(string msg)
        {
            CheckForIllegalCrossThreadCalls = false;
            logBox.AppendText("[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + msg + "\n");
        }
    }
}


