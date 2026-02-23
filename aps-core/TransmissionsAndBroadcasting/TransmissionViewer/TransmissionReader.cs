using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Scheduler;
using PT.Transmissions;

namespace TransmissionViewer;

/// <summary>
/// Reads a transmission file and converts it to text.
/// </summary>
public class TransmissionReader
{
    private readonly Transmission transmission;

    public TransmissionReader(string transmissionFileName)
    {
        transmission = ReadTransmission(transmissionFileName);
    }

    public string GetText()
    {
        //return this.GetReflectedText(this.transmission);
        return Dumper.DumpObject(transmission, 5);
    }

    public Transmission GetTransmission()
    {
        return transmission;
    }

    private Transmission ReadTransmission(string filename)
    {
        try
        {
            if (!File.Exists(filename))
            {
                throw new PTHandleableException("2351", new object[] { filename });
            }

            FileStream stream = File.OpenRead(filename);
            TransmissionRecording recording;
            using (BinaryFileReader fileReader = new (filename))
            {
                TransmissionClassFactory factory = new ();
                recording = new TransmissionRecording(fileReader, factory);
            }

            return recording.Transmission;
        }
        catch (Exception e)
        {
            string msg = e.Message + "\n\n" + e.StackTrace;
            throw new PTHandleableException(msg);
        }
    }

    private string GetReflectedText(object o)
    {
        if (o is string || o is TimeSpan || o is DateTime || o is bool || o is decimal || o is double || o is int || o is long || o is BaseId
           )
        {
            return o.ToString();
        }

        //Not base type so do reflection to get properties 
        Type type = o.GetType();
        int propCount = TypeDescriptor.GetProperties(type, true).Count;

        System.Text.StringBuilder stringBuilder = new ();

        PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(type, true); //.Sort(GetSortStringArray(type));
        for (int i = 0; i < propCount; i++)
        {
            PropertyDescriptor pd;
            pd = pdc[i]; //sort by default sort specified in the object
            //pd = TypeDescriptor.GetProperties(type).Sort()[i]; ////sort alphabetically

            if (i > 0)
            {
                stringBuilder.Append(", ");
            }

            object o2 = pd.GetValue(o);
            stringBuilder.Append(string.Format("{0}={1}", pd.Name, GetReflectedText(o2)));
        }

        return stringBuilder.ToString();
    }
}