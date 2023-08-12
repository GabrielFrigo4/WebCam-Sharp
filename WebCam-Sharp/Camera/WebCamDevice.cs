using OpenCvSharp;
using Size = System.Drawing.Size;

namespace WebCam_Sharp.Camera;
public struct WebCamDevice
{
    public int Id { get; private set; }
    public Size Size { get; private set; }
    public VideoCapture VideoCapture { get; private set; }
    public string Name { get; private set; }
    public string Caption { get; private set; }
    public string Description { get; private set; }
    public string Manufacturer { get; private set; }


    public WebCamDevice(int id, Size size, VideoCapture videoCapture,
        string name, string caption, string description, string manufacturer)
    {
        Id = id;
        Size = size;
        VideoCapture = videoCapture;
        Name = name;
        Caption = caption;
        Description = description;
        Manufacturer = manufacturer;
    }

    public void UpdateVideoCapture(out VideoCapture videoCapture)
    {
        videoCapture = this.VideoCapture;
        videoCapture.Open(this.Id);
        videoCapture.Set(VideoCaptureProperties.FrameHeight, this.Size.Height);
        videoCapture.Set(VideoCaptureProperties.FrameWidth, this.Size.Width);
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