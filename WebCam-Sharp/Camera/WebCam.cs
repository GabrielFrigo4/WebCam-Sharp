using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.Reflection;

using Size = System.Drawing.Size;

namespace WebCam_Sharp.Camera;
public class WebCam: IDisposable
{
    #region Vars
    public EventHandler<Bitmap>? UpdateFrame;
    public object SyncRoot = new();

    public int CurrentDeviceID { get; private set; } = 0;
    public WebCamDevice CurrentDevice { get => WebCamDevice.Devices[CurrentDeviceID]; }

    private bool IsRunning { get; set; }

    private TimeSpan _frameDelay;
    public TimeSpan FrameDelay { get => _frameDelay; }

    private TimeSpan _frameMinDelay;
    public TimeSpan FrameMinDelay { get => _frameMinDelay; }

    private int _frameRate;
    public int FrameRate
    {
        get => _frameRate;

        set
        {
            _frameRate = value;
            _frameDelay = TimeSpan.FromMilliseconds(1000d / _frameRate);
            _frameMinDelay = TimeSpan.FromMilliseconds(50d / _frameRate);
            videoCapture.Set(VideoCaptureProperties.Fps, value);
        }
    }

    private readonly Mat matFrame;
    private readonly VideoCapture videoCapture;
    private Bitmap? image = null;
    #endregion

    #region WebCam
    public WebCam(int deviceId = 0, int fps = 60, VideoCaptureAPIs videoCaptureAPI = VideoCaptureAPIs.ANY)
    {
        matFrame = new Mat();
        WebCamDevice.Init();
        CurrentDeviceID = deviceId;
        videoCapture = CurrentDevice.CreateVideoCapture(videoCaptureAPI);
        FrameRate = fps;
    }

    public void Begin()
    {
        Task.Run(StartRunning);
    }

    public async void SetDevice(int deviceId, VideoCaptureAPIs videoCaptureAPI = VideoCaptureAPIs.ANY, bool async = true)
    {
        void SetTask()
        {
            lock (SyncRoot)
            {
                if (CurrentDeviceID != deviceId)
                {
                    CurrentDeviceID = deviceId;
                    CurrentDevice.UpdateVideoCapture(videoCapture, videoCaptureAPI);
                }
            }
        }

        if (!async) SetTask();
        else await Task.Run(SetTask);
    }

    public async void SetDevice(WebCamDevice device, VideoCaptureAPIs videoCaptureAPI = VideoCaptureAPIs.ANY, bool async = true)
    {
        void SetTask()
        {
            lock (SyncRoot)
            {
                SetDevice(device.Id, videoCaptureAPI, async);
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

    public void End()
    {
        IsRunning = false;
    }

    public void Dispose()
    {
        matFrame.Dispose();
        videoCapture.Dispose();
        image?.Dispose();
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