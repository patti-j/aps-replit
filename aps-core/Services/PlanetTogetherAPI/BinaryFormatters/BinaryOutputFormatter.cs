using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

using PT.Common.File;

namespace PT.PlanetTogetherAPI.BinaryFormatters;

public class BinaryOutputFormatter : OutputFormatter
{
    public BinaryOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/octet-stream"));
    }

    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        // This formatter should be used when given the appropriate header (currently sent by the PtSerializingHttpClient)
        // and the api returns content that can be transferred in this way (byte[] or IPTSerializable).
        return context.HttpContext.Request.Headers.Any(h => h.Key.Equals("PTSerializationEnabled") &&
                                                            (typeof(byte[]).IsAssignableFrom(context.ObjectType) || typeof(IPTSerializable).IsAssignableFrom(context.ObjectType)));
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        object responseObject = context.Object;

        HttpResponse response = context.HttpContext.Response;
        response.ContentType = "application/octet-stream";

        try
        {
            if (responseObject == null)
            {
                return;
            }

            if (responseObject is IPTSerializable ptSerializable)
            {
                // Requires serialization before writing to response
                using (MemoryStream serializationStream = new ())
                {
                    Serialization.SerializeToStream(ptSerializable, serializationStream);
                    await WriteToResponseBody(serializationStream, response);
                }
            }
            else if (responseObject is MemoryStream responseStream)
            {
                // Write directly to response
                await WriteToResponseBody(responseStream, response);
            }
            else if (responseObject is byte[] responseBytes)
            {
                await response.Body.WriteAsync(responseBytes);
            }
            else
            {
                throw new ArgumentException("BinaryOutputFormatter attempted to format an incompatible response.");
            }
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException("BinaryOutputFormatter", e);
            throw e;
        }
    }

    public override void WriteResponseHeaders(OutputFormatterWriteContext context)
    {
        base.WriteResponseHeaders(context);

        // Returns the same header in the response, so the client knows to parse it appropriately.
        context.HttpContext.Response.Headers.Add("PTSerializationEnabled", "true");
    }

    private static async Task WriteToResponseBody(MemoryStream stream, HttpResponse response)
    {
        stream.Position = 0;
        await stream.CopyToAsync(response.Body);
    }
}