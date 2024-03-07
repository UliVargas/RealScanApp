using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace RealScan
{
    //
    //  Structure Declarations
    //

    //
    //  Related Device Structure
    //
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           // Device Information
    public struct RSDeviceInfo
    {
        public int deviceType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] productName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] deviceID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] firmwareVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] HardwareVersion;
        //public int lastInputTime;
        //public int lastScanTime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public int[] reserved;
     };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //SDK Information
    public struct RSSDKInfo 
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] product;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] version;
       
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] buildDate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] reserved;
    };
    //
    //  Related Overlay Structure
    //
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]            //Make OverlayText
    public struct RSOverlayText
    {
        public RSPoint pos;
        public int alignment;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] text;
        public int fontSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] fontName;
        public ulong color;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] reserved;
        //unsigned alignment;
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //Make OverlayCross
    public struct RSOverlayCross
    {

        public RSPoint centerPos;
        public int rangeX;
        public int rangeY;
        public ulong color;
        public int width;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] reserved;
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //Make OverlayLine
    public struct RSOverlayLine
    {
        public RSPoint startPos;
        public RSPoint endPos;
        public ulong color;
        public int width;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] reserved;
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //Make OverlayQuadrangle
    public struct RSOverlayQuadrangle
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public RSPoint[] pos;
        public ulong color;
        public int width;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] reserved;
    };
    //
    //  Related Misc. Structure
    //
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RSPoint
    {
        public int x;
        public int y;
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RSRect
    {
        public int x;
        public int y;
        public int right;
        public int bottom;
        public int left;
        public int top;
    };
    //
    //  Related Slap Structure
    //
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //Slap Information     
    public struct RSSlapInfo
    {
        public int fingerType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public RSPoint[] fingerPosition;
        public int imageQuality;
        public int rotation;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] reserved;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RSSlapInfoArray
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public RSSlapInfo[] RSSlapInfoA;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //Frame Information
    public struct RSFrameInfo
    {
        public IntPtr pImage;
        public IntPtr pQualityMap;
        public int iWidth;
        public int iHeight;
        public int iNFIQ;
        public int iQuality;
        public int iLiveness;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //Frame Information
    public struct RSLFDInfo
    {
        public int nResult;
        public int nScore;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]           //Frame Information
    public struct RSLFDResult
    {
        public int nNumofFinger;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public RSLFDInfo[] arsLFDInfo;
    };

    //
    // Delegates
    //
    public delegate void CaptureDataCallback(int deviceHandle, int errorCode, IntPtr imageData, int imageWidth, int imageHeight);
    public delegate void AdvCaptureDataCallback(int deviceHandle, int errorCode, ref RSFrameInfo frameInfo);
    
    public delegate void RSRawCaptureCallback(int errorCode, IntPtr imageData, int imageWidth, int imageHeight);
    public delegate void RSAdvCaptureCallback(int errorCode, IntPtr imageData, int imageWidth, int imageHeight, int lfdScore);
   
    public delegate void RSPreviewDataCallback(int deviceHandle, int errorCode, IntPtr imageData, int imageWidth, int imageHeight);
    public delegate void RSAdvPreviewCallback(int deviceHandle, int errorCode, IntPtr imageData, int imageWidth, int imageHeight, int quality, int status);
    public delegate void RSRawPreviewCallback(int errorCode, byte[] imageData, int imageWidth, int imageHeight);
    public delegate void RSRawAdvPreviewCallback(int errorCode, byte[] imageData, int imageWidth, int imageHeight, int quality, int status);

    public delegate void RSKeypadCallback(int deviceHandle, uint keyCode);
    public delegate void RSKeypadCallbackInt(int deviceHandle, uint keyCode);

    //
    // API Declarations
    //    

    class RealScanSDK
    {
        //
        // Constant Definitions
        //
        public const int RS_SUCCESS = 0;
        public const int RS_ERR_UNKNOWN = -1;
        public const int RS_ERR_NOT_SUPPORT = -2;
        public const int RS_ERR_SMALL_BUFFER = -4;
        public const int RS_ERR_NO_DATA = -5;


        //
        // Generic errors
        //
        public const int RS_ERR_SDK_UNINITIALIZED			    = -10;
        public const int RS_ERR_SDK_ALREADY_INITIALIZED		    = -11;
        public const int RS_ERR_INSUFFICIENT_HARDWARE		    = -12;
	    public const int RS_ERR_INVALID_PARAM				    = -13;
		public const int RS_ERR_MEM_FULL					    = -14;
		public const int RS_ERR_NOT_YET_SUPPORTED			    = -15;
		public const int RS_ERR_CANNOT_OPEN_FILE			    = -16;
		public const int RS_ERR_CANNOT_READ_FILE			    = -17;
		public const int RS_ERR_CANNOT_WRITE_FILE			    = -18;

	    //
	    // Device related errors
	    //
	    public const int RS_ERR_NO_DEVICE					    = -100;
	    public const int RS_ERR_INVALID_DEVICE_INDEX		    = -101;
	    public const int RS_ERR_INVALID_HANDLE				    = -102;
	    public const int RS_ERR_CANNOT_INIT_DEVICE			    = -103;
		public const int RS_ERR_UNSUPPORTED_DEVICE			    = -104;
		public const int RS_ERR_CANNOT_GET_USB_DEVICE		    = -105;
		public const int RS_ERR_DEVICE_ALREADY_INITIALIZED 	    = -106;
		public const int RS_ERR_CANNOT_OPEN_DEVICE			    = -107;
		public const int RS_ERR_CANNOT_WRITE_USB			    = -108;
	    public const int RS_ERR_WRITE_USB_TIMEOUT			    = -109;
	    public const int RS_ERR_CANNOT_READ_USB				    = -110;
	    public const int RS_ERR_READ_USB_TIMEOUT			    = -111;
	    public const int RS_ERR_INVALID_CAMERA_MODE			    = -112;
		public const int RS_ERR_UNSUPPORTED_WAV_FORMAT		    = -113;
		public const int RS_ERR_UNSUPPORTED_COMMAND			    = -114;
		public const int RS_ERR_SENSOR_DIRTY				    = -115;
		public const int RS_ERR_FINGER_EXIST				    = -116;
		public const int RS_ERR_TOO_STRONG_LIGHT			    = -117;
	    public const int RS_ERR_INVALID_DEVICE_CONNECTION	    = -124;

	    //
	    // Capture related errors
	    //
	    public const int RS_ERR_INVALID_CAPTURE_MODE		    = -200;
	    public const int RS_ERR_CAPTURE_DISABLED			    = -201;
	    public const int RS_ERR_CAPTURE_TIMEOUT				    = -202;
	    public const int RS_ERR_CAPTURE_ABORTED				    = -203;
		public const int RS_ERR_CAPTURE_CAMERA				    = -204;
		public const int RS_ERR_CAPTURE_NO_PREVIEW			    = -205;
		public const int RS_ERR_ROLL_PART_LIFT				    = -206;
		public const int RS_ERR_ROLL_DIRTY					    = -207;
		public const int RS_ERR_ROLL_TOO_FAST				    = -208;
	    public const int RS_ERR_ROLL_SHIFTED				    = -209;
	    public const int RS_ERR_ROLL_DRY					    = -210;
	    public const int RS_ERR_ROLL_WRONG_DIR				    = -211;
	    public const int RS_ERR_CAPTURE_IS_RUNNING			    = -212;
		public const int RS_ERR_CAPTURE_IS_NOT_RUNNING		    = -213;
		public const int RS_ERR_ROLL_TOO_SHORT				    = -214;
		public const int RS_ERR_CANNOT_SEGMENT				    = -215;
		public const int RS_ERR_CANNOT_GET_QUALITY			    = -216;
		public const int RS_ERR_SEGMENT_FEWER_FINGER		    = -217;
	    public const int RS_ERR_SEGMENT_WRONG_HAND			    = -218;
	    public const int RS_ERR_CANNOT_EXTRACT_TEMPLATE		    = -219;
	    public const int RS_ERR_NO_MATCH					    = -220;
	    public const int RS_ERR_ROLL_NO_FINGER				    = -221;
		public const int RS_ERR_CAPTURE_FINGERS_MOVING		    = -222;
		public const int RS_ERR_CAPTURE_NOTENOUGH_FINGERS	    = -223;
		public const int RS_ERR_CAPTURE_TOOSMALL_FINGERSAREA    = -224;
		public const int RS_ERR_CAPTURE_TOOWEAK_PRINT		    = -225;
		public const int RS_ERR_CAPTURE_TOOSTRONG_PRINT		    = -226;
	    public const int RS_ERR_CAPTURE_TOOMUCH_PRESSURE	    = -227;
	    public const int RS_ERR_CAPTURE_NOTENOUGH_FRAMES	    = -228;
	    public const int RS_ERR_CAPTURE_WRONG_MINFINGER		    = -229;
		public const int RS_ERR_ABNORMAL_CAPTURE_SIZE           = -230;

	    //
	    // Viewer related errors
	    //
	    public const int RS_ERR_CANNOT_SET_VIEW_WINDOW		    = -300;
	    public const int RS_ERR_NO_MORE_OVERLAY				    = -301;
	    public const int RS_ERR_INVALID_OVERLAY_HANDLE		    = -302;
	    public const int RS_ERR_CANNOT_SHOW_OVERLAY			    = -303;
		public const int RS_ERR_CANNOT_SHOW_PREVIEW			    = -304;
                                                                  
        //                                                                
        // IO related errors                                              
        //                                                                
	    public const int RS_ERR_CANNOT_SET_KEYPAD_CALLBACK	    = -400;
	    public const int RS_ERR_CANNOT_MAKE_LCD_DATA			= -401;
	    public const int RS_ERR_INVALID_LCD_DATA				= -402;
 
        //
        // Warning
        //
		public const int RS_WRN_STRONG_LIGHT					= 117;
		public const int RS_WRN_FAKE_FINGER					    = 133;
	    public const int RS_WRN_TOO_POOR_QUALITY				= 134;
	    public const int RS_WRN_BAD_SCAN						= 601;
	                                              
	    public const int RS_BAD_IMAGE						    = 701;
		public const int RS_FINGER_ON_LEFT_SIDE				    = 702;
		public const int RS_FINGER_ON_RIGHT_SIDE			    = 703;
		public const int RS_FINGER_ON_TOP_SIDE				    = 704;
		public const int RS_FINGER_ON_BOTTOM_SIDE			    = 705;
		public const int RS_FINGER_ON_TOP_LEFT_SIDE			    = 706;
	    public const int RS_FINGER_ON_TOP_RIGHT_SIDE		    = 707;
	    public const int RS_FINGER_ON_BOTTOM_LEFT_SIDE		    = 708;
	    public const int RS_FINGER_ON_BOTTOM_RIGHT_SIDE		    = 709;
	                                             
		public const int RS_PALM_ON_LEFT_SIDE				    = 712;
		public const int RS_PALM_ON_RIGHT_SIDE				    = 713;
		public const int RS_PALM_ON_TOP_SIDE				    = 714;
		public const int RS_PALM_ON_BOTTOM_SIDE				    = 715;
		public const int RS_PALM_ON_TOP_LEFT_SIDE			    = 716;
	    public const int RS_PALM_ON_TOP_RIGHT_SIDE			    = 717;
	    public const int RS_PALM_ON_BOTTOM_LEFT_SIDE		    = 718;
	    public const int RS_PALM_ON_BOTTOM_RIGHT_SIDE		    = 719;
                                                
		public const int RS_ERR_PALM_EXIST					    = 720;


        public const int RS_LFD_LIVE = 0;
        public const int RS_LFD_FAKE = 1;


        //
        // Device APIs
        //
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_InitSDK")]
        public static extern int RS_InitSDK(byte[] configFileName, int option, ref int numOfDevice);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_InitDevice")]
        public static extern int RS_InitDevice(int deviceIndex, ref int deviceHandle);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_ExitDevice")]
        public static extern int RS_ExitDevice(int deviceHandle);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_ExitAllDevices")]
        public static extern int RS_ExitAllDevices();

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetDeviceInfo")]
        public static extern int RS_GetDeviceInfo(int deviceHandle, ref RSDeviceInfo deviceInfo);

        //
        // Capture APIs
        //
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetCaptureMode")]
        public static extern int RS_SetCaptureMode(int deviceHandle, int captureMode, int captureOption, bool withModeLED);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetCaptureDir")]
        public static extern int RS_SetCaptureDir(int deviceHandle, int captureDirection);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetCaptureModeWithDir")]
        public static extern int RS_SetCaptureModeWithDir(int deviceHandle, int captureMode, int captureDirection, int captureOption, bool withModeLED);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetManualCaptureMode")]
        public static extern int RS_SetManualCaptureMode(int deviceHandle, int imageX, int imageY, int imageWidth, int imageHeight, int captureOption, bool isFlat);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetCaptureMode")]
        public static extern int RS_GetCaptureMode(int deviceHandle, ref int captureMode, ref int captureOption);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetCaptureModeWithDir")]
        public static extern int RS_GetCaptureModeWithDir(int deviceHandle, ref int captureMode, ref int captureDirection, ref int captureOption);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetCaptureDir")]
        public static extern int RS_GetCaptureDir(int deviceHandle, ref int captureDirection);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetImageSize")]
        public static extern int RS_GetImageSize(int deviceHandle, ref int imageWidth, ref int imageHeight);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_StartCapture")]
        public static extern int RS_StartCapture(int deviceHandle, bool autoCapture, int timeout);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_AbortCapture")]
        public static extern int RS_AbortCapture(int deviceHandle);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_RegisterCaptureDataCallback")]
        public static extern int RS_RegisterCaptureDataCallback(int deviceHandle, CaptureDataCallback captureCallback);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_RegisterAdvCaptureDataCallback")]
        public static extern int RS_RegisterAdvCaptureDataCallback(int deviceHandle, AdvCaptureDataCallback captureCallback);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_TakeImageData")]
        public static extern int RS_TakeImageData(int deviceHandle, int timeout, ref IntPtr imageData, ref int imageWidth, ref int imageHeight );

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_TakeImageDataEx")]
        public static extern int RS_TakeImageDataEx( int deviceHandle, int timeout, int fingerIndex, bool withLED, ref IntPtr imageData, ref int imageWidth, ref int imageHeight );
        
        [DllImport("RS_SDK.dll",  CharSet = CharSet.Ansi, EntryPoint = "RS_TakeImageEx")]
        public static extern int RS_TakeImageEx(int deviceHandle, int timeout, int fingerIndex, bool withLED, ref IntPtr image);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_TakeCurrentImageEx")]
        public static extern int RS_TakeCurrentImageEx(int deviceHandle, int timeout, int fingerIndex, bool withLED, ref IntPtr image);
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_TakeCurrentImageDataEx")]
        public static extern int RS_TakeCurrentImageDataEx( int deviceHandle, int timeout, int fingerIndex, bool withLED, ref IntPtr imageData, ref int imageWidth, ref int imageHeight );
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_TakeCurrentImageData")]
        public static extern int RS_TakeCurrentImageData(int deviceHandle, int timeout, ref IntPtr imageData, ref int imageWidth, ref int imageHeight);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi,EntryPoint = "RS_SequenceCheck")]
        public static extern int RS_SequenceCheck(int numOfFinger, IntPtr fingerImageData, ref int fingerImageWidth, ref int fingerImageHeight, IntPtr slapImageData, int slapImageWidth, int slapImageHeight, int slapType, ref int fingerSequenceInSlap, int securityLevel);
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SegmentImages")]
        public static extern int RS_SegmentImages(IntPtr imageData, int imageWidth, int imageHeight, int slapType, ref int numOfFinger, ref IntPtr slapInfoArray_, string outFilename);

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_Segment4")]
        public static extern int RS_Segment4(IntPtr imageData, int imageWidth, int imageHeight, int slapType, ref int numOfFinger, ref IntPtr slapInfo, ref IntPtr fingerImageData1, ref int fingerImageDataWidth1, ref int fingerImageDataHeight1, ref IntPtr fingerImageData2, ref int fingerImageDataWidth2, ref int fingerImageDataHeight2, ref IntPtr fingerImageData3, ref int fingerImageDataWidth3, ref int fingerImageDataHeight3, ref IntPtr fingerImageData4, ref int fingerImageDataWidth4, ref int fingerImageDataHeight4);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_Segment4WithSize")]
        public static extern int RS_Segment4WithSize(IntPtr imageData, int imageWidth, int imageHeight, int slapType, ref int numOfFinger, ref IntPtr slapInfo, ref IntPtr fingerImageData1, ref IntPtr fingerImageData2, ref IntPtr fingerImageData3, ref IntPtr fingerImageData4, int nCropWidth, int nCropHeight);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RSFingerInfo
        {
            public int nFingerType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public RSPoint[] arsPoint;
            public int nImageQuality;
            public int nRotation;
            public int nWidth;
            public int nHeight;
            public int nReserved;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RSSegmentInfo
        {
            public int nFingerCnt;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public RSFingerInfo[] arsFingerInfo;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RSMissingInfo
        {
            public int nFirstfinger;
            public int nSecondfinger;
            public int nThirdfinger;
            public int nFourthfinger;
        };
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SegmentExMissingFinger")]
        public static extern int RS_SegmentExMissingFinger(IntPtr imageData, int imageWidth, int imageHeight, int slapType, ref RSSegmentInfo psSegmentInfo, byte[] pbyFinImg1, byte[] pbyFinImg2, byte[] pbyFinImg3, byte[] pbyFinImg4, RSMissingInfo sMissingInfo);


        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetMinimumFinger")]
        public static extern int RS_SetMinimumFinger(int deviceHandle, int minFingerCount);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetMinimumFinger")]
        public static extern int RS_GetMinimumFinger(int deviceHandle, ref int pnMinFingerCount);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetQualityScore")]
        public static extern int RS_GetQualityScore(IntPtr imageData, int imageWidth, int imageHeight, ref int nistQuality);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SelfTest")]
        public static extern int RS_SelfTest(int deviceHandle, int testType);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_Calibrate")]
        public static extern int RS_Calibrate(int deviceHandle); 
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetLFDStatus")]
        public static extern int RS_SetLFDStatus(int deviceHandle, bool isActivated, float th1, float th2, float th3, float th4);
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetLFDStatus")]
        public static extern int RS_GetLFDStatus(int deviceHandle, ref bool isActivated, ref float th1, ref float th2, ref float th3, ref float th4);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetSegRotateOption")]
        public static extern int RS_SetSegRotateOption(bool isRotating);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetPreProcessing")]
        public static extern int RS_SetPreProcessing(int deviceHandle, int preprocessMode);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetPreProcessing")]
        public static extern int RS_GetPreProcessing(int deviceHandle, ref int preprocessMode); 

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetPostProcessing")]
        public static extern int RS_SetPostProcessing( int deviceHandle, bool contrastEnhancement, bool noiseReduction );

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetPostProcessing")]
        public static extern int RS_GetPostProcessing( int deviceHandle, ref bool contrastEnhancement, ref bool noiseReduction );    

        //
        // VIEW APIs
        //
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetViewWindow")]
        public static extern int RS_SetViewWindow(int deviceHandle, IntPtr windowHandle, RSRect drawRectangle, bool autoContrast);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_StopViewWindow")]
        public static extern int RS_StopViewWindow(int deviceHandle);

       [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_RemoveAllOverlay")]
        public static extern int RS_RemoveAllOverlay(int deviceHandle);
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_RegisterAdvPreviewCallback")]
       public static extern int RS_RegisterAdvPreviewCallback(int deviceHandle, RSAdvPreviewCallback captureCallback);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_RegisterPreviewCallback")]
        public static extern int RS_RegisterPreviewCallback(int deviceHandle, RSPreviewDataCallback captureCallback); 
       

        //
        // IO APIs
        //
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_ResetLCD")]
        public static extern int RS_ResetLCD(int deviceHandle);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_DisplayLCD")]
        public static extern int RS_DisplayLCD(int deviceHandle, IntPtr data, int dataLen, int sx, int sy, int width, int height);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_MakeLCDData")]
        public static extern int RS_MakeLCDData(byte[] inputRData, byte[] inputGData, byte[] inputBData, int inputWidth, int inputHeight, ref IntPtr outputData);
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetFingerLED")]
        public static extern int RS_SetFingerLED(int deviceHandle, int Finger_LED, int ledColor);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetStatusLED")]
        public static extern int RS_SetStatusLED(int deviceHandle, int ledCode );

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetModeLED")]
        public static extern int RS_SetModeLED(int deviceHandle, int Mode_LED, bool isOn);

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_Beep")]
        public static extern int RS_Beep(int deviceHandle, int beepPattern);

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetKeyStatus")]
        public static extern int RS_GetKeyStatus( int deviceHandle,  ref int keyCode );

          [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetActiveKey")]
         public static extern int RS_SetActiveKey(int deviceHandle, uint keyMask);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_PlayWav")]
        public static extern int RS_PlayWav( int deviceHandle,  String wavFile);
                
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_RegisterKeypadCallback")]
          public static extern int RS_RegisterKeypadCallback(int deviceHandle, RSKeypadCallback callback);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetCaptureBeep")]
        public static extern int RS_SetCaptureBeep(int deviceHandle, int startingBeep, int successBeep, int failBeep);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_TurnOnBacklight_RSF")]
        public static extern int RS_TurnOnBacklight_RSF(int deviceHandle, bool isOn);

        // Overlay APIs
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_AddOverlayText")]
        public static extern int RS_AddOverlayText( int deviceHandle, ref RSOverlayText text, ref int overlayhandle);

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_AddOverlayLine")]
        public static extern int RS_AddOverlayLine( int deviceHandle, ref RSOverlayLine line, ref int overlayHandle );

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_AddOverlayQuadrangle")]
         public static extern int RS_AddOverlayQuadrangle(int deviceHandle, ref RSOverlayQuadrangle quadrangle, ref int overlayHandle);

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_ShowOverlay")]
         public static extern int RS_ShowOverlay(int overlayHandle, bool show);

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_AddOverlayCross")]
         public static extern int RS_AddOverlayCross( int deviceHandle, ref RSOverlayCross cross, ref int overlayHandle );

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_ShowAllOverlay")]
         public static extern int RS_ShowAllOverlay( int deviceHandle, bool show );

         [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_RemoveOverlay")]
         public static extern int RS_RemoveOverlay( int overlayHandle );
        
        // Misc APIs
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SaveBitmap")]
        public static extern int RS_SaveBitmap( IntPtr pixelData, int imageWidth, int imageHeight, string filename );

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SaveBitmapMem")]
        public static extern int RS_SaveBitmapMem(IntPtr pixelData, int imageWidth, int imageHeight, byte[] imageData);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SaveBitmap")]
        public static extern int RS_SaveBitmap(IntPtr[] pixelData, int[] imageWidth, int[] imageHeight, string filename);
                
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetErrStringChar")]
        public static extern int RS_GetErrStringChar(int errorCode, byte[] errorMsg);

        public static int RS_GetErrString(int errorCode, ref string errorMsg)
        {
            errorMsg = "";
            byte[] strErrMsg = new byte[1024];
            int rv = RS_GetErrStringChar(errorCode, strErrMsg);
            for (int i = 0; i < 1024; i++)
            {
                errorMsg += (char)strErrMsg[i];
                if (strErrMsg[i] == 0) break;
            }
            return rv;
        }

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetCaptureStatusStringChar")]
        public static extern int RS_GetCaptureStatusStringChar(int statusCode, byte[] statusMsg);

        public static int RS_GetCaptureStatusString(int statusCode, ref string statusMsg)
        {
            statusMsg = "";
            byte[] strStatusMsg = new byte[1024];
            int rv = RS_GetCaptureStatusStringChar(statusCode, strStatusMsg);
            for (int i = 0; i < 1024; i++)
            {
                statusMsg += (char)strStatusMsg[i];
                if (strStatusMsg[i] == 0) break;
            }
            return rv;
        }

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetSDKInfo")]
        public static extern int RS_GetSDKInfo(ref RSSDKInfo sdkInfo);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_FreeImageData")]
        public static extern void RS_FreeImageData(IntPtr imageData);

        // Win32 APIs
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, ref RSRect rect);


        //Option APIs
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetLFDLevel")]
         public static extern int RS_SetLFDLevel(int deviceHandle, int LFDLevel);
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetAutomaticCalibrate")]
        public static extern int RS_SetAutomaticCalibrate( int deviceHandle, bool automatic );

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetAutomaticCalibrate")]
        public static extern int RS_GetAutomaticCalibrate(int deviceHandle, ref bool automatic);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetAutomaticContrast")]
        public static extern int RS_SetAutomaticContrast( int deviceHandle, bool automatic );
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetAutomaticContrast")]
        public static extern int RS_GetAutomaticContrast(int deviceHandle, ref bool automatic);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetAdvancedContrastEnhancement")]
        public static extern int RS_SetAdvancedContrastEnhancement(int deviceHandle, bool enabled);
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetAdvancedContrastEnhancement")]
        public static extern int RS_GetAdvancedContrastEnhancement(int deviceHandle, ref bool enabled);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetRollFingerOption")]
        public static extern int RS_GetRollFingerOption(int deviceHandle, ref int rollDirection, ref int rollTime, ref int rollProfile);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetManualContrast")]
        public static extern int RS_SetManualContrast( int deviceHandle, int contrastLevel );

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetPostProcessingEx")]
        public static extern int RS_SetPostProcessingEx( int deviceHandle, bool contrastEnhancement, bool noiseReduction, int reductionLevel );

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetPostProcessingEx")]
        public static extern int RS_GetPostProcessingEx( int deviceHandle, ref bool contrastEnhancement, ref bool noiseReduction, ref int reductionLevel );

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetLFDLevel")]
        public static extern int RS_GetLFDLevel(int deviceHandle, ref int LFDLevel);

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetLFDResult")]
        public static extern int RS_GetLFDResult(int deviceHandle, ref RSLFDResult sLFDResult);     

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_GetManualContrast")]
         public static extern int RS_GetManualContrast( int deviceHandle, ref int contrastLevel );
        
        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SetRollFingerOption")]
        public static extern int RS_SetRollFingerOption( int deviceHandle, int rollDirection, int rollTime, int rollProfile );

        [DllImport("RS_SDK.dll", CharSet = CharSet.Ansi, EntryPoint = "RS_SequenceCheck")]
        public static extern int RS_SequenceCheck( int numOfFinger, ref IntPtr fingerImageData, ref int fingerImageWidth, ref int fingerImageHeight, 
												IntPtr slapImageData, int slapImageWidth, int slapImageHeight, int slapType, ref int fingerSequenceInSlap, int securityLevel );




        // FingerType

        // Device type
        public const int RS_DEVICE_REALSCAN_10 	    =0x00;
        public const int RS_DEVICE_REALSCAN_10F	    =0x01;
        public const int RS_DEVICE_REALSCAN_D	    =0x10;
        public const int RS_DEVICE_REALSCAN_DF	    =0x11;
        public const int RS_DEVICE_REALSCAN_F	    =0x20;
        public const int RS_DEVICE_REALSCAN_G10	    =0x30;
        public const int RS_DEVICE_REALSCAN_G10F	=0x31;
        public const int RS_DEVICE_REALSCAN_G1 	    =0x32;
        public const int RS_DEVICE_UNKNOWN		    =0xFF;
        
        // Initialization Mode
        //
        public const int RS_INIT_HIDE_INIDLG		=0x01;
        public const int RS_INIT_SHOW_INIDLG		=0x02;
        public const int RS_INIT_FULL               =0x04;  

        //
        // Capture mode
        //
        public const int RS_CAPTURE_DISABLED				= 0x00;
        public const int RS_CAPTURE_ROLL_FINGER             = 0x01;
        public const int RS_CAPTURE_FLAT_SINGLE_FINGER      = 0x02;
        public const int RS_CAPTURE_FLAT_TWO_FINGERS        = 0x03;
        public const int RS_CAPTURE_FLAT_LEFT_FOUR_FINGERS  = 0x04;
        public const int RS_CAPTURE_FLAT_RIGHT_FOUR_FINGERS = 0x05;
        public const int RS_CAPTURE_FLAT_LEFT_PALM          = 0x06;
        public const int RS_CAPTURE_FLAT_RIGHT_PALM         = 0x07;
        public const int RS_CAPTURE_FLAT_SINGLE_FINGER_EX   = 0x12;
        public const int RS_CAPTURE_FLAT_TWO_FINGERS_EX     = 0x13;
              
        public const int RS_CAPTURE_ROLL_FINGER_EX          = 0x30;
        
        public const int RS_CAPTURE_FLAT_LEFT_SIDE_PALM     = 0x40;
        public const int RS_CAPTURE_FLAT_RIGHT_SIDE_PALM    = 0x41;
        public const int RS_CAPTURE_FLAT_LEFT_WRITERS_PALM  = 0x42;
        public const int RS_CAPTURE_FLAT_RIGHT_WRITERS_PALM = 0x43;
        public const int RS_CAPTURE_FLAT_LEFT_UPPER_PALM    = 0x44;
        public const int RS_CAPTURE_FLAT_RIGHT_UPPER_PALM   = 0x45;
        public const int RS_CAPTURE_FLAT_LEFT_LOWER_PALM    = 0x46;
        public const int RS_CAPTURE_FLAT_RIGHT_LOWER_PALM   = 0x47;
        
        public const int RS_CAPTURE_FLAT_MANUAL_INI         = 0xfe;
        public const int RS_CAPTURE_FLAT_MANUAL             = 0xff;

        // Capture Direction
        public const int RS_CAPTURE_DIRECTION_DEFAULT		= 0x00;
        public const int RS_CAPTURE_DIRECTION_LEFT			= 0x01;
        public const int RS_CAPTURE_DIRECTION_RIGHT         = 0x02;

        //
        // Options for capturing flat fingers
        //
        public const int RS_AUTO_SENSITIVITY_NORMAL   = 0x00;
        public const int RS_AUTO_SENSITIVITY_HIGH     = 0x01;
        public const int RS_AUTO_SENSITIVITY_HIGHER   = 0x02;
        public const int RS_AUTO_SENSITIVITY_DISABLED = 0x03; 

        //
        // Roll direction
        //
        public const int RS_ROLL_DIR_L2R        = 0x00;
        public const int RS_ROLL_DIR_R2L        = 0x01;
        public const int RS_ROLL_DIR_AUTO       = 0x02;
        public const int RS_ROLL_DIR_AUTO_M     = 0x03; 

        //
        // Roll profile
        //
        public const int RS_ROLL_PROFILE_LOW    = 0x01;
        public const int RS_ROLL_PROFILE_NORMAL = 0x02;
        public const int RS_ROLL_PROFILE_HIGH   = 0x03;

        //
        // Text alignment
        //
        public const int RS_TEXT_ALIGN_LEFT   = 0x00;
        public const int RS_TEXT_ALIGN_CENTER = 0x01;
        public const int RS_TEXT_ALIGN_RIGHT  = 0x02;

        //
        // Beeper pattern
        //
        public const int RS_BEEP_PATTERN_NONE = 0;
        public const int RS_BEEP_PATTERN_1    = 1;	// 1 short beep
        public const int RS_BEEP_PATTERN_2    = 2;	// 2 short beeps

        //
        // Keypad code
        //
        public const int RS_REALSCAN10_NO_KEY   = 0x00;
        public const int RS_REALSCAN10_PLAY_KEY = 0x20;
        public const int RS_REALSCAN10_STOP_KEY = 0x40;
        public int RS_REALSCAN10_ALL_KEYS       = 0x7F;

        public const int RS_REALSCAND_NO_KEY   = 0x00;
        public const int RS_REALSCAND_KEY_0    = 0x20;
        public const int RS_REALSCAND_ALL_KEYS = 0x7F;

        public const int RS_REALSCANF_NO_KEY     = 0x00;
        public const int RS_REALSCANF_UP_KEY     = 0x01;
        public const int RS_REALSCANF_DOWN_KEY   = 0x02;
        public const int RS_REALSCANF_LEFT_KEY   = 0x04;
        public const int RS_REALSCANF_RIGHT_KEY  = 0x08;
        public const int RS_REALSCANF_PLAY_KEY 	 = 0x20;
        public const int RS_REALSCANF_STOP_KEY	 = 0x40;
        public const int RS_REALSCANF_FOOTSWITCH = 0x80;
        public const int RS_REALSCANF_ALL_KEYS   = 0xFF;
        public const int RS_REALSCANG10_NO_KEY   = 0x00;
        public const int RS_REALSCANG10_PLAY_KEY = 0x20;
        public const int RS_REALSCANG10_STOP_KEY = 0x40;
        public const int RS_REALSCANG10_ALL_KEYS = 0x7F;
        //                              
        // Finger Index                 
        //                              
        public const int RS_FINGER_ALL          = 0x00;
        public const int RS_FINGER_LEFT_LITTLE  = 0x01;
        public const int RS_FINGER_LEFT_RING    = 0x02;
        public const int RS_FINGER_LEFT_MIDDLE  = 0x03;
        public const int RS_FINGER_LEFT_INDEX   = 0x04;
        public const int RS_FINGER_LEFT_THUMB   = 0x05;
        public const int RS_FINGER_RIGHT_THUMB  = 0x06;
        public const int RS_FINGER_RIGHT_INDEX  = 0x07;
        public const int RS_FINGER_RIGHT_MIDDLE = 0x08;
        public const int RS_FINGER_RIGHT_RING   = 0x09;
        public const int RS_FINGER_RIGHT_LITTLE = 0x0A;
        public const int RS_FINGER_TWO_THUMB    = 0x0B;
        public const int RS_FINGER_LEFT_FOUR    = 0x0C;
        public const int RS_FINGER_RIGHT_FOUR   = 0x0D;
        public const int RS_FINGER_TWO_LEFT1    = 0x0E;
        public const int RS_FINGER_TWO_LEFT2    = 0x0F;
        public const int RS_FINGER_TWO_RIGHT2   = 0x10;
        public const int RS_FINGER_TWO_RIGHT1   = 0x11;
        public const int RS_PALM_LEFT           = 0x12;
        public const int RS_PALM_RIGHT          = 0x13; 

        //
        // Finger position for segmentation
        //
        public const int RS_FGP_UNKNOWN           = 0;
        public const int RS_FGP_RIGHT_THUMB       = 1;
        public const int RS_FGP_RIGHT_INDEX       = 2;
        public const int RS_FGP_RIGHT_MIDDLE      = 3;
        public const int RS_FGP_RIGHT_RING        = 4;
        public const int RS_FGP_RIGHT_LITTLE      = 5;
        public const int RS_FGP_LEFT_THUMB        = 6;
        public const int RS_FGP_LEFT_INDEX        = 7;
        public const int RS_FGP_LEFT_MIDDLE       = 8;
        public const int RS_FGP_LEFT_RING         = 9;
        public const int RS_FGP_LEFT_LITTLE       = 10;
        public const int RS_FGP_PLAIN_RIGHT_THUMB = 11;
        public const int RS_FGP_PLAIN_LEFT_THUMB  = 12;
        public const int RS_FGP_PLAIN_RIGHT_FOUR  = 13;
        public const int RS_FGP_PLAIN_LEFT_FOUR   = 14;
        public const int RS_FGP_PLAIN_TWO_THUMBS  = 15;
        public const int RS_FGP_EJI_OR_TIP        = 16;
                                            
        //                                  
        // LED for RealScan-10              
        //
        public const int RS_LED_MODE_ALL           = 0x00;
        public const int RS_LED_MODE_LEFT_FINGER4  = 0x01;
        public const int RS_LED_MODE_RIGHT_FINGER4 = 0x02;
        public const int RS_LED_MODE_TWO_THUMB     = 0x03;
        public const int RS_LED_MODE_ROLL          = 0x04;
        public const int RS_LED_POWER              = 0x05;

        //
        // LED Colors
        //
        public const int RS_LED_OFF				    =0x00;
        public const int RS_LED_GREEN			    =0x01;
        public const int RS_LED_RED				    =0x02;
        public const int RS_LED_YELLOW			    =0x03;

        //
        // LED Status for G1 
        //
        public const int RS_LED_STATUS_OFF		    =0x00;
        public const int RS_LED_STATUS_ON		    =0x01;
        public const int RS_LED_STATUS_BLINK		=0x02;

        //
        // Slap type
        //
        public const int RS_SLAP_LEFT_FOUR			    =1;
        public const int RS_SLAP_RIGHT_FOUR			    =2;
        public const int RS_SLAP_FOUR_FINGER			=3;
        public const int RS_SLAP_TWO_THUMB			    =4;
        public const int RS_SLAP_TWO_FINGER			    =5;
        public const int RS_SLAP_ONE_FINGER			    =6;
        public const int RS_SLAP_ONE_FINGER_ROLL		=7;
                                            
        //                                  
        // Security level                   
        //                                  
        public const int RS_SECURITY_1_TO_100		    =0x01; 
        public const int RS_SECURITY_1_TO_1000		    =0x02; 
        public const int RS_SECURITY_1_TO_10000		    =0x03;
        public const int RS_SECURITY_1_TO_100000		=0x04;
        public const int RS_SECURITY_1_TO_1000000	    =0x05;
        public const int RS_SECURITY_1_TO_10000000	    =0x06;
        public const int RS_SECURITY_1_TO_100000000	    =0x07;
                                            
        //                                  
        // Contrast enhancement
        //
        public const int RS_CONTRAST_ENHANCEMENT_DEFVALUE =0;
        public const int RS_CONTRAST_ENHANCEMENT_MAXVALUE =40;

        //
        // Self test type
        //
         public const int RS_SELFTEST_ILLUMINATION	=1;
         public const int RS_SELFTEST_DIRT			=2;

        //
        // LCD Display
        //
        public const int RS_LCD_WIDTH_MAX			=320; 
        public const int RS_LCD_HEIGHT_MAX			=240; 
        public const int RS_LCD_DATA_SIZE_MAX		=153600; 

        //
        // LFD Level for G1 
        //
        public const int RS_LFD_OFF				    =0;
        public const int RS_LFD_LEVEL_1				=1;
        public const int RS_LFD_LEVEL_2				=2;
        public const int RS_LFD_LEVEL_3				=3;
        public const int RS_LFD_LEVEL_4				=4;
        public const int RS_LFD_LEVEL_5				=5;
        public const int RS_LFD_LEVEL_6				=6;

        //
        // Preprocessing mode
        //
        public const int RS_HIGH_CONTRAST_PREPROCESS       = 0x01;
        public const int RS_HIGH_VISIBILITY_PREPROCESS     = 0x02;
        public const int RS_BALANCED_PREPROCESS            = 0x03;

        // Halo processing
        public const int RS_DIABLE_HALOPROCESS             =0x00;
        public const int RS_ENABLE_HALOPROCESS             =0x01;

    }
}
