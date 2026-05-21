using EmployeeApi.Interfaces;
using Microsoft.ApplicationInsights.Channel;

namespace EmployeeApi.Telemetry
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = "EmployeeApi";
            telemetry.Context.Cloud.RoleInstance =
                Environment.MachineName;
        }
    }
}
