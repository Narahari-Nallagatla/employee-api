using Microsoft.ApplicationInsights.Channel;

namespace EmployeeApi.Interfaces
{
    public interface ITelemetryInitializer
    {
        void Initialize(ITelemetry telemetry);
    }
}
