namespace WebAPI.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEY_HEADER_NAME = "AzureAPIKey";  // Header name
        private const string APIKEY_VALUE = "Pl@n3t10geth3r"; // API key value 

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the header contains the API key
            if (!context.Request.Headers.TryGetValue(APIKEY_HEADER_NAME, out var providedApiKey) || providedApiKey != APIKEY_VALUE)
            {
                // If the key is missing or invalid, set the response status to 401 Unauthorized
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Unauthorized access. API key is missing or invalid.");
                return;
            }

            // If the API key is valid, pass control to the next middleware
            await _next(context);
        }
    }
}
