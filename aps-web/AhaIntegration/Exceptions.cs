using System;

namespace AhaIntegration
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }

        public override string Message
        {
            get { return $"Status {StatusCode}: {Content}"; }
        }
    }
}
