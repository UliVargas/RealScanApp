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

        public int DeviceListSelectedIndex = 0;

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


            MsgPanel.Text = "El SDK se inicializó correctamente";

            if (numOfDevice > 0)
            {
                DeviceListSelectedIndex = 0;
            }

            Init_Device();

        }

        private void Init_Device()
        {
            RSDeviceInfo deviceInfo = new RSDeviceInfo();

            m_result = RealScanSDK.RS_InitDevice(DeviceListSelectedIndex, ref deviceHandle);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }

            AutoCalibrate = true;

            m_prevStopped = true;


            m_result = RealScanSDK.RS_GetDeviceInfo(deviceHandle, ref deviceInfo);
            if (m_result != RealScanSDK.RS_SUCCESS)
            {
                RealScanSDK.RS_GetErrString(m_result, ref m_errorMsg);
                MsgPanel.Text = m_errorMsg;
                return;
            }

            DeviceInfo.Text = System.Text.Encoding.ASCII.GetString(deviceInfo.productName);

            InitDeviceEnabled = false;
            ExitDeviceEnabled = true;

            if (deviceInfo.deviceType != RealScanSDK.RS_DEVICE_REALSCAN_F)
            {
                ResetLCDEnable = false;
                DisplayLCDEnable = false;
            }

            MsgPanel.Text = "El dispositivo se ha inicializado correctamente";
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
                    return -1;
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
            RealScanSDK.RS_GetLFDLevel(deviceHandle, ref nLFDLevel);
            if (nLFDLevel == 0)
            {
                MsgPanel.Text = "La imagen se capturó correctamente";
            }
            else
            {
                RSLFDResult sLFDResult = new RSLFDResult();
                nRetVal = RealScanSDK.RS_GetLFDResult(deviceHandle, ref sLFDResult);
                if (nRetVal != RealScanSDK.RS_SUCCESS)
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
                    MsgPanel.Text = String.Format("{0} {1}", nFinalLFDResult == RealScanSDK.RS_LFD_LIVE ? "[LIVE FINGER]" : "[FAKE FINGER]", strScores);
                }
            }

            AllocateConsole();
            Console.WriteLine(CallbackSelectedIndex);

            if ((CallbackSelectedIndex == (int)callbackMode.saveNseg))
            {
                int nMinFinger = 4;
                nRetVal = RealScanSDK.RS_GetMinimumFinger(deviceHandle, ref nMinFinger);
                if (nMinFinger == 4)
                {
                    int numOfFingers = 0;
                    IntPtr[] ImageBuffer = new IntPtr[4];
                    int[] ImageWidth = new int[4];
                    int[] ImageHeight = new int[4];
                    RSSlapInfoArray slapInfoA = new RSSlapInfoArray();

                    SegmentCaptureProcess(imageData, imageWidth, imageHeight, deviceHandle, ref slapInfoA, ref numOfFingers, ref ImageBuffer, ref ImageWidth, ref ImageHeight);

                    if (CallbackSelectedIndex == (int)callbackMode.saveNseg)
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
                    if (nRetVal == RealScanSDK.RS_SUCCESS)
                    {
                        RealScanSDK.RSSegmentInfo sSegmentInfo = new RealScanSDK.RSSegmentInfo();
                        sSegmentInfo.arsFingerInfo = new RealScanSDK.RSFingerInfo[4];
                        for (int i = 0; i < 4; i++)
                        {
                            sSegmentInfo.arsFingerInfo[i].arsPoint = new RSPoint[4];
                            sSegmentInfo.arsFingerInfo[i].nWidth = 1000;
                            sSegmentInfo.arsFingerInfo[i].nHeight = 1000;
                        }

                        byte[] pbyFinImg1 = new byte[1000 * 1000];
                        byte[] pbyFinImg2 = new byte[1000 * 1000];
                        byte[] pbyFinImg3 = new byte[1000 * 1000];
                        byte[] pbyFinImg4 = new byte[1000 * 1000];

                        RealScanSDK.RSMissingInfo sMissingInfo = new RealScanSDK.RSMissingInfo();
                        if (nSlapType == RealScanSDK.RS_SLAP_LEFT_FOUR)
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
                            for (int i = 0; i < sSegmentInfo.nFingerCnt; i++)
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
            
            getBlobCapture(imageData, imageWidth, imageHeight, ref blob);

            AllocateConsole();
            Console.WriteLine(blob[0]);

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

                    
                       RealScanSDK.RS_SaveBitmap(ImageBuffer[i], m_nCustomSegWidth, m_nCustomSegHeight, saveDialog.FileName + "_" + slapInfo.RSSlapInfoA[i].fingerType + ".bmp");
                    
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

        private void getBlobCapture(IntPtr imageData, int imageWidth, int imageHeight, ref byte[] blob)
        {
            int size = imageWidth * imageHeight;
            byte[] prevImageData = new byte[size];
            Marshal.Copy(imageData, prevImageData, 0, imageWidth * imageHeight);
            blob = prevImageData;
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

        private void DisplayLCD()
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

        public void log(string msg)
        {
            CheckForIllegalCrossThreadCalls = false;
        }   
    }
}


