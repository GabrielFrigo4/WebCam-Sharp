using OpenCvSharp;
using System.Management;
using Size = System.Drawing.Size;

namespace WebCam_Sharp.Camera;
public struct WebCamDevice
{
    public static WebCamDevice[] Devices { get; private set; } = Array.Empty<WebCamDevice>();

    public int Id { get; private set; } = -1;
    public Size Size { get; private set; } = new(-1, -1);
    public string Name { get; private set; } = string.Empty;
    public string Caption { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Manufacturer { get; private set; } = string.Empty;

    public WebCamDevice(int id, string name, string caption,
        string description, string manufacturer)
    {
        Id = id;
        Name = name;
        Caption = caption;
        Description = description;
        Manufacturer = manufacturer;
    }

    public WebCamDevice(int id, Size size, string name, 
        string caption, string description, string manufacturer)
    {
        this.Id = id;
        this.Size = size;
        this.Name = name;
        this.Caption = caption;
        this.Description = description;
        this.Manufacturer = manufacturer;
    }

    public VideoCapture CreateVideoCapture()
    {
        VideoCapture videoCapture = new(this.Id);
        videoCapture.Open(this.Id);
        videoCapture.Set(VideoCaptureProperties.FrameHeight, int.MaxValue);
        videoCapture.Set(VideoCaptureProperties.FrameWidth, int.MaxValue);
        int width = (int)videoCapture.Get(VideoCaptureProperties.FrameWidth);
        int height = (int)videoCapture.Get(VideoCaptureProperties.FrameHeight);
        this.Size = new(width, height);
        this.Save();
        return videoCapture;
    }

    public readonly void UpdateVideoCapture(VideoCapture videoCapture)
    {
        videoCapture.Open(this.Id);
        videoCapture.Set(VideoCaptureProperties.FrameHeight, this.Size.Height);
        videoCapture.Set(VideoCaptureProperties.FrameWidth, this.Size.Width);
    }

    public readonly void Save()
    {
        Devices[this.Id] = this;
    }

    public static void Init()
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

                if (device[nameof(Name)] is string _name)
                    name = _name;

                if (device[nameof(Caption)] is string _cap)
                    cap = _cap;

                if (device[nameof(Description)] is string _desc)
                    desc = _desc;

                if (device[nameof(Manufacturer)] is string _man)
                    man = _man;

                cameraInfos.Add(new(name, cap, desc, man));
            }
        }

        Devices = new WebCamDevice[cameraInfos.Count];
        Parallel.For(0, cameraInfos.Count, (id) =>
        {
            var _tup = cameraInfos[id];

            Devices[id] = new(id, _tup.Item1, _tup.Item2, _tup.Item3, _tup.Item4);
        });
    }

    public static bool operator ==(WebCamDevice wcd1, WebCamDevice wcd2)
    {
        return wcd1.Equals(wcd2);
    }

    public static bool operator !=(WebCamDevice wcd1, WebCamDevice wcd2)
    {
        return !wcd1.Equals(wcd2);
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}