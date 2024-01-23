using DirectShowLib;
using OpenCvSharp;
using System.Management;

namespace WebCam_Sharp.Camera;
public struct WebCamDevice
{
    public static WebCamDevice[] Devices { get; private set; } = [];

    public int Id { get; private set; } = -1;
    public int PNP_Id { get => Id; private set => Id = value; }
    public int DirectShow_Id { get; private set; } = -1;
    public Size Size { get; private set; } = new(-1, -1);

    public string Caption { get; private set; } = string.Empty;
    public string ClassGuid { get; private set; } = string.Empty;
    public string[] CompatibleID { get; private set; } = Array.Empty<string>();
    public string Description { get; private set; } = string.Empty;
    public string DeviceID { get; private set; } = string.Empty;
    public string[] HardwareID { get; private set; } = Array.Empty<string>();
    public string Manufacturer { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string PNPClass { get; private set; } = string.Empty;
    public string PNPDeviceID { get; private set; } = string.Empty;
    public string Service { get; private set; } = string.Empty;

    public WebCamDevice(int id, string caption, string classGuid,
        string[] compatibleID, string description, string deviceID,
        string[] hardwareID, string manufacturer, string name,
        string pnpClass, string pnpDeviceID, string service)
    {
        this.Id = id;
        this.Caption = caption;
        this.ClassGuid = classGuid;
        this.CompatibleID = compatibleID;
        this.Description = description;
        this.DeviceID = deviceID;
        this.HardwareID = hardwareID;
        this.Manufacturer = manufacturer;
        this.Name = name;
        this.PNPClass = pnpClass;
        this.PNPDeviceID = pnpDeviceID;
        this.Service = service;
    }

    public WebCamDevice(int id, Size size, string caption, string classGuid,
        string[] compatibleID, string description, string deviceID,
        string[] hardwareID, string manufacturer, string name,
        string pnpClass, string pnpDeviceID, string service)
    {
        this.Id = id;
        this.Size = size;
        this.Caption = caption;
        this.ClassGuid = classGuid;
        this.CompatibleID = compatibleID;
        this.Description = description;
        this.DeviceID = deviceID;
        this.HardwareID = hardwareID;
        this.Manufacturer = manufacturer;
        this.Name = name;
        this.PNPClass = pnpClass;
        this.PNPDeviceID = pnpDeviceID;
        this.Service = service;
    }

    public VideoCapture CreateVideoCapture(VideoCaptureAPIs videoCaptureAPIs = VideoCaptureAPIs.ANY)
    {
        VideoCapture videoCapture = new(GetVideoCaptureID(videoCaptureAPIs), videoCaptureAPIs);
        videoCapture.Set(VideoCaptureProperties.FrameHeight, int.MaxValue);
        videoCapture.Set(VideoCaptureProperties.FrameWidth, int.MaxValue);
        int width = (int)videoCapture.Get(VideoCaptureProperties.FrameWidth);
        int height = (int)videoCapture.Get(VideoCaptureProperties.FrameHeight);
        this.Size = new(width, height);
        this.Save();
        return videoCapture;
    }

    public void UpdateVideoCapture(VideoCapture videoCapture, VideoCaptureAPIs videoCaptureAPIs = VideoCaptureAPIs.ANY)
    {
        videoCapture.Open(GetVideoCaptureID(videoCaptureAPIs), videoCaptureAPIs);
        if (this.Size == new Size(-1, -1))
        {
            videoCapture.Set(VideoCaptureProperties.FrameHeight, int.MaxValue);
            videoCapture.Set(VideoCaptureProperties.FrameWidth, int.MaxValue);
            int width = (int)videoCapture.Get(VideoCaptureProperties.FrameWidth);
            int height = (int)videoCapture.Get(VideoCaptureProperties.FrameHeight);
            this.Size = new(width, height);
            this.Save();
        }
        else
        {
            videoCapture.Set(VideoCaptureProperties.FrameHeight, this.Size.Height);
            videoCapture.Set(VideoCaptureProperties.FrameWidth, this.Size.Width);
        }
    }

    public int GetVideoCaptureID(VideoCaptureAPIs videoCaptureAPIs)
    {
        return videoCaptureAPIs switch
        {
            VideoCaptureAPIs.DSHOW => this.DirectShow_Id,
            VideoCaptureAPIs.MSMF => this.PNP_Id,
            _ => this.Id,
        };
    }

    public readonly void Save()
    {
        Devices[this.Id] = this;
    }

    public static void Init()
    {
        List<(string, string, string[], string,
            string, string[], string, string,
            string, string, string)> cameraInfos = [];

        string queryCode = "SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')";
        using (ManagementObjectSearcher searcher = new(queryCode))
        {
            foreach (var device in searcher.Get())
            {
                string _Caption = string.Empty;
                string _ClassGuid = string.Empty;
                string[] _CompatibleID = Array.Empty<string>();
                string _Description = string.Empty;
                string _DeviceID = string.Empty;
                string[] _HardwareID = Array.Empty<string>();
                string _Manufacturer = string.Empty;
                string _Name = string.Empty;
                string _PNPClass = string.Empty;
                string _PNPDeviceID = string.Empty;
                string _Service = string.Empty;

                if (device[nameof(Caption)] is string __Caption__)
                    _Caption = __Caption__;

                if (device[nameof(ClassGuid)] is string __ClassGuid__)
                    _ClassGuid = __ClassGuid__;

                if (device[nameof(CompatibleID)] is string[] __CompatibleID__)
                    _CompatibleID = __CompatibleID__;

                if (device[nameof(Description)] is string __Description__)
                    _Description = __Description__;

                if (device[nameof(DeviceID)] is string __DeviceID__)
                    _DeviceID = __DeviceID__;

                if (device[nameof(HardwareID)] is string[] __HardwareID__)
                    _HardwareID = __HardwareID__;

                if (device[nameof(Manufacturer)] is string __Manufacturer__)
                    _Manufacturer = __Manufacturer__;

                if (device[nameof(Name)] is string __Name__)
                    _Name = __Name__;

                if (device[nameof(PNPClass)] is string __PNPClass__)
                    _PNPClass = __PNPClass__;

                if (device[nameof(PNPDeviceID)] is string __PNPDeviceID__)
                    _PNPDeviceID = __PNPDeviceID__;

                if (device[nameof(Service)] is string __Service__)
                    _Service = __Service__;

                var t = (_Caption, _ClassGuid, _CompatibleID,
                    _Description, _DeviceID, _HardwareID, _Manufacturer,
                    _Name, _PNPClass, _PNPDeviceID, _Service);

                cameraInfos.Add(t);
            }
        }

        string[] names = GetDirectShow_DeviceNames();
        Devices = new WebCamDevice[cameraInfos.Count];
        Parallel.For(0, cameraInfos.Count, (id) =>
        {
            var _tup = cameraInfos[id];

            Devices[id] = new(id, _tup.Item1, _tup.Item2, _tup.Item3,
                _tup.Item4, _tup.Item5, _tup.Item6, _tup.Item7,
                _tup.Item8, _tup.Item9, _tup.Item10, _tup.Item11);

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == Devices[id].Name)
                {
                    Devices[id].DirectShow_Id = i;
                    break;
                }
            }
        });
    }

    public static string[] GetDirectShow_DeviceNames()
    {
        List<string> cameraNames = [];
        DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

        foreach (var device in devices)
        {
            cameraNames.Add(device.Name);
            device.Dispose();
        }

        return [.. cameraNames];
    }

    public static string[] GetPNP_DeviceNames()
    {
        List<string> cameraNames = [];

        foreach (var device in Devices)
            cameraNames.Add(device.Name);

        return [.. cameraNames];
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