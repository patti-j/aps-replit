using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PT.UIDefinitions;

public class PrintControllerFormat
{
    private string _Name = string.Empty;
    private ImageFormat _Format;
    private ImageCodecInfo _Codec;

    public string Name => _Name;

    public ImageFormat Format => _Format;

    public ImageCodecInfo Codec => _Codec;

    private PrintControllerFormat() { }

    public static PrintControllerFormat[] Formats
    {
        get
        {
            ArrayList a = new ();

            Type type = typeof(ImageFormat);
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == type)
                {
                    PrintControllerFormat format = new ();
                    format._Name = property.Name;
                    format._Format = (ImageFormat)property.GetValue(null, null);
                    format._Codec = GetImageCodecInfo(format._Format);
                    a.Add(format);
                }
            }

            PrintControllerFormat[] formats = new PrintControllerFormat[a.Count];
            a.CopyTo(formats);

            return formats;
        }
    }

    public static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }

        return null;
    }
}

public class PrintControllerFile : PreviewPrintController
{
    private readonly ImageFormat _Format;
    private readonly float _Scale = 1f;
    private readonly long _Quality = 75L;
    private readonly string _Output = string.Empty;

    private readonly ImageCodecInfo _Codec;
    private int _Page;
    private Metafile _Metafile;

    public PrintControllerFile(ImageFormat format, float scale, long quality, string output)
    {
        if (quality < 0 || quality > 100)
        {
            throw new ArgumentOutOfRangeException("quality", quality, "Quality must be between 0 and 100");
        }

        _Format = format;
        _Scale = scale;
        _Quality = quality;
        _Output = output;

        _Codec = PrintControllerFormat.GetImageCodecInfo(_Format);

        string dir = Path.GetDirectoryName(_Output);
        if (dir.Length > 0)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

    public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
    {
        _Page++;

        return base.OnStartPage(document, e);
    }

    public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
    {
        base.OnEndPage(document, e);

        // Get the current Metafile
        PreviewPageInfo[] ppia = GetPreviewPageInfo();
        PreviewPageInfo ppi = ppia[ppia.Length - 1];
        Image image = ppi.Image;
        _Metafile = (Metafile)image;

//			using ( _Metafile )
        {
            if (_Format == ImageFormat.Emf)
            {
                _Metafile.Save(PagePath, _Format);
                return;
            }

            if (_Format == ImageFormat.Wmf)
            {
                _Metafile.Save(PagePath, _Format);
                return;
            }

//				_Metafile.Save( PagePath, _Format );
            SaveViaBitmap(document, e);
        }
    }

    protected string PagePath => _Output + "." + _Page.ToString("000") + Extension;

    protected string Extension
    {
        get
        {
            if (_Format == ImageFormat.Emf)
            {
                return ".emf";
            }

            if (_Format == ImageFormat.Wmf)
            {
                return ".wmf";
            }

            if (_Codec == null)
            {
                return ".unknown";
            }

            string[] extensions = _Codec.FilenameExtension.Split(';');
            if (extensions.Length < 1)
            {
                Debug.Assert(false);
                return ".unknown";
            }

            if (extensions[0].Length < 1)
            {
                Debug.Assert(false);
                return ".unknown";
            }

            string extension = extensions[0].Substring(1);

            return extension.ToLower();
        }
    }

    protected void SaveViaBitmap(PrintDocument document, PrintPageEventArgs e)
    {
        int width = e.PageBounds.Width;
        int height = e.PageBounds.Height;

        using (Bitmap bitmap = new ((int)(width * _Scale), (int)(height * _Scale)))
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.White);

            if (_Scale != 1)
            {
                graphics.ScaleTransform(_Scale, _Scale);
            }

            Point point = new (0, 0);
            Graphics.EnumerateMetafileProc callback = new (PlayRecord);

            graphics.EnumerateMetafile(_Metafile, point, callback);

            if (_Scale == 1 || true)
            {
                Save(bitmap);
            }
            else
            {
                using (Bitmap bitmap2 = new (width, height))
                using (Graphics graphics2 = Graphics.FromImage(bitmap2))
                {
                    graphics2.DrawImage(bitmap, 0, 0, width, height);

                    Save(bitmap2);
                }
            }
        }
    }

    protected bool PlayRecord(
        EmfPlusRecordType recordType,
        int flags,
        int dataSize,
        IntPtr data,
        PlayRecordCallback callbackData)
    {
        byte[] dataArray = null;
        if (data != IntPtr.Zero)
        {
            // Copy the unmanaged record to a managed byte buffer 
            // that can be used by PlayRecord.
            dataArray = new byte[dataSize];
            Marshal.Copy(data, dataArray, 0, dataSize);
        }

        _Metafile.PlayRecord(recordType, flags, dataSize, dataArray);

        return true;
    }

    protected void Save(Bitmap bitmap)
    {
        if (_Format == ImageFormat.Jpeg)
        {
            EncoderParameters parameters = new (1);
            EncoderParameter parameter = new (Encoder.Quality, _Quality);
            parameters.Param[0] = parameter;

            bitmap.Save(PagePath, _Codec, parameters);
            return;
        }

        bitmap.Save(PagePath, _Format);
    }
}