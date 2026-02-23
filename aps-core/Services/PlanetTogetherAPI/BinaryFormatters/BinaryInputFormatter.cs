using Microsoft.AspNetCore.Mvc.Formatters;

using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

namespace PT.PlanetTogetherAPI.BinaryFormatters;

public class BinaryInputFormatter : InputFormatter
{
    public BinaryInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/octet-stream"));
    }

    public override bool CanRead(InputFormatterContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return context.HttpContext.Request.ContentType == "application/octet-stream";
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        try
        {
            using (MemoryStream ms = new ())
            {
                Stream requestBody = context.HttpContext.Request.Body;
                await requestBody.CopyToAsync(ms);

                return await InputFormatterResult.SuccessAsync(ms);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}