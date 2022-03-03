using System.Net;
using System.Text.Json.Serialization;

namespace Has.WebMacro
{
    public class ErrorMacroResponse
    {
        public ErrorMacroResponse() { }
        public ErrorMacroResponse(HttpStatusCode httpStatusCode, string message)
        {
            HttpStatusCode = httpStatusCode;
            Error = message;
        }

        [JsonPropertyName("httpStatusCode")]
        public HttpStatusCode HttpStatusCode { get; set; }
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}