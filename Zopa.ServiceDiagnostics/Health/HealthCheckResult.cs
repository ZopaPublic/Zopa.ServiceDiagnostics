using System;
using System.Runtime.Serialization;

namespace Zopa.ServiceDiagnostics.Health
{
    [DataContract]
    public class HealthCheckResult
    {
        private HealthCheckResult() { }

        [DataMember(Name = "exceptionMessage")]
        public string ExceptionMessage { get; private set; }

        [DataMember(Name = "executionTime")]
        public TimeSpan ExecutionTime { get; private set; }

        [DataMember(Name = "additionalMessage")]
        public string AdditionalMessage { get; private set; }

        [DataMember(Name = "name")]
        public string Name { get; private set; }

        [DataMember(Name = "passed")]
        public bool Passed => ExceptionMessage == null;

        public static HealthCheckResult Pass(string name, TimeSpan executionTime, string additionalMessage = null)
        {
            return new HealthCheckResult
            {
                Name = name,
                ExecutionTime = executionTime,
                AdditionalMessage = additionalMessage
            };
        }

        public static HealthCheckResult Fail(string name, Exception ex, TimeSpan executionTime, string additionalMessage = null)
        {
            return new HealthCheckResult
            {
                Name = name,
                ExceptionMessage = ex.GetFullStackTrace(),
                ExecutionTime = executionTime,
                AdditionalMessage = additionalMessage
            };
        }
    }
}
