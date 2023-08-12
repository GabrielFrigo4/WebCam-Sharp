using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.Reflection;
using Size = System.Drawing.Size;

namespace WebCam_Sharp.Camera;
public class WebCam
{
    #region Vars
    public EventHandler<Bitmap>? UpdateFrame;
    public object SyncRoot = new();

    public int CurrentDeviceID { get; private set; } = 0;
    public WebCamDevice CurrentDevice { get => WebCamDevice.Devices[CurrentDeviceID]; }

    private bool IsRunning { get; set; }

    private TimeSpan _frameDelay;
    private TimeSpan FrameDelay { get => _frameDelay; }

    private TimeSpan _frameMinDelay;
    private TimeSpan FrameMinDelay { get => _frameMinDelay; }

    private int _frameRate;
    public int FrameRate
    {
        get => _frameRate;

        set
        {
            _frameRate = value;
            _frameDelay = TimeSpan.FromMilliseconds(1000d / _frameRate);
            _frameMinDelay = TimeSpan.FromMilliseconds(50d / _frameRate);
        }
    }

    private readonly Mat matFrame;
    private readonly VideoCapture videoCapture;
    private Bitmap? image = null;
    #endregion

    #region WebCam
    public WebCam(int deviceId = 0, int fps = 60)
    {
        FrameRate = fps;
        matFrame = new Mat();
        WebCamDevice.Init();
        CurrentDeviceID = deviceId;
        videoCapture = CurrentDevice.CreateVideoCapture();
    }

    public void Init()
    {
        Task.Run(StartRunning);
    }

    public async void SetDevice(int deviceId, bool async = true)
    {
        void SetTask()
        {
            lock (SyncRoot)
            {
                if (CurrentDeviceID != deviceId)
                {
                    CurrentDeviceID = deviceId;
                    CurrentDevice.UpdateVideoCapture(videoCapture);
                }
            }
        }

        if (!async) SetTask();
        else await Task.Run(SetTask);
    }

    public async void SetDevice(WebCamDevice device, bool async = true)
    {
        void SetTask()
        {
            lock (SyncRoot)
            {
                SetDevice(device.Id, async);
            }
        }

        if (!async) SetTask();
        else await Task.Run(SetTask);
    }

    public void SetFrameSize(Size size)
    {
        videoCapture.Set(VideoCaptureProperties.FrameHeight, size.Height);
        videoCapture.Set(VideoCaptureProperties.FrameWidth, size.Width);
    }

    public void SetFocusState(FocusState focusState)
    {
        videoCapture.Set(VideoCaptureProperties.AutoFocus, (int)focusState);
    }

    public void SetFocusValue(int value)
    {
        videoCapture.Set(VideoCaptureProperties.Focus, value);
    }

    private async void StartRunning()
    {
        IsRunning = true;

        DateTime _startTime;
        TimeSpan _frameTime;

        while (IsRunning)
        {
            if (UpdateFrame is null) throw new NullReferenceException("UpdateFrame");

            _startTime = DateTime.Now;
            lock (SyncRoot)
            {
                if (videoCapture.IsOpened())
                {
                    videoCapture.Read(matFrame);
                    image?.Dispose();
                    image = matFrame.ToBitmap();
                    UpdateFrame.Invoke(this, (Bitmap)image.Clone());
                }
            }

            _frameTime = DateTime.Now - _startTime;
            TimeSpan result = FrameDelay - _frameTime;
            if (result > FrameMinDelay) await Task.Delay(result);
            else await Task.Delay(FrameMinDelay);
        }
    }

    public static Version GetVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version? asmCloneVersion = (Version?)assembly.GetName().Version?.Clone();
        return asmCloneVersion ?? throw new NullReferenceException("version");
    }
    #endregion
}

public enum FocusState
{
    Manual = 0,
    Auto = 1,
}