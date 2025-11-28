using System.Net;

namespace nhl_service_dotnet.Exceptions
{
    public class NhlException : Exception
    {
        private HttpStatusCode status;
        public HttpStatusCode StatusCode
        {
            get { return status; }
            set { status = value; }
        }

        public NhlException(string message)
            : base(message) { }

        public NhlException(string message, HttpStatusCode status)
            : base(message)
        {
            this.StatusCode = status;
        }
    }
}
