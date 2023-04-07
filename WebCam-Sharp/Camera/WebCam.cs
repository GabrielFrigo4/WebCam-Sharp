using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using System.Management;
using System.Reflection;
using Size = System.Drawing.Size;

namespace WebCam_Sharp.Camera;
public class WebCam
{
    #region Vars
    public EventHandler<Bitmap>? UpdateFrame;
    public object SyncRoot = new();

    public WebCamDevice[] Devices { get; private set; }
    public WebCamDevice CurrentDevice { get; private set; }

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
    private VideoCapture videoCapture;
    private Bitmap? image = null;
    #endregion

    #region WebCam
    public WebCam(int deviceId = 0, int fps = 60)
    {
        FrameRate = fps;
        matFrame = new Mat();
        Devices = GetDevices();
        CurrentDevice = Devices[deviceId];
        videoCapture = CurrentDevice.VideoCapture;
        videoCapture.Open(CurrentDevice.Id);
        videoCapture.Set(VideoCaptureProperties.FrameHeight, CurrentDevice.Size.Height);
        videoCapture.Set(VideoCaptureProperties.FrameWidth, CurrentDevice.Size.Width);
    }

    public void Init()
    {
        Task.Run(StartRunning);
    }

    public async void SetDevice(WebCamDevice device, bool async = true)
    {
        void SetTask()
        {
            lock (SyncRoot)
            {
                if (CurrentDevice != device)
                {
                    CurrentDevice = device;
                    videoCapture = CurrentDevice.VideoCapture;
                    videoCapture.Open(CurrentDevice.Id);
                    videoCapture.Set(VideoCaptureProperties.FrameHeight, CurrentDevice.Size.Height);
                    videoCapture.Set(VideoCaptureProperties.FrameWidth, CurrentDevice.Size.Width);
                }
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

    private static WebCamDevice[] GetDevices()
    {
        List<Tuple<string, string, string, string>> cameraInfos = new();
        string queryCode = "SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')";
        using (ManagementObjectSearcher searcher = new(queryCode))
        {
            foreach (var device in searcher.Get())
            {
                string name = string.Empty;
                string cap = string.Empty;
                string desc = string.Empty;
                string man = string.Empty;

                if (device["Name"] is string _name)
                    name = _name;

                if (device["Caption"] is string _cap)
                    cap = _cap;

                if (device["Description"] is string _desc)
                    desc = _desc;

                if (device["Manufacturer"] is string _man)
                    man = _man;

                cameraInfos.Add(new(name, cap, desc, man));
            }
        }

        WebCamDevice[] devices = new WebCamDevice[cameraInfos.Count];
        Parallel.For(0, cameraInfos.Count, (id) =>
        {
            VideoCapture _videoCapture = new(id);
            var _tup = cameraInfos[id];
            Size _size = new();

            _videoCapture.Set(VideoCaptureProperties.FrameHeight, int.MaxValue);
            _videoCapture.Set(VideoCaptureProperties.FrameWidth, int.MaxValue);
            _size.Width = (int)_videoCapture.Get(VideoCaptureProperties.FrameWidth);
            _size.Height = (int)_videoCapture.Get(VideoCaptureProperties.FrameHeight);

            devices[id] = new(id, _size, _videoCapture,
                _tup.Item1, _tup.Item2, _tup.Item3, _tup.Item4);
        });

        return devices;
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