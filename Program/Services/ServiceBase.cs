using Microsoft.Extensions.Options;

namespace nhl_service_dotnet.Services
{
    public abstract class ServiceBase
    {
        protected readonly AppSettings settings;

        protected ServiceBase(IOptions<AppSettings>? settings)
        {
            this.settings = settings?.Value ?? new AppSettings();
        }
    }
}
