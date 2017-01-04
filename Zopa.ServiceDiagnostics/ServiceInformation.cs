using System.Reflection;
using System.Runtime.Serialization;
using Zopa.ServiceDiagnostics.Health;

namespace Zopa.ServiceDiagnostics
{
    [DataContract]
    public class ServiceInformation
    {
        public ServiceInformation(Assembly entryAssembly, HealthCheckResults health)
        {
            Version = entryAssembly.GetName().Version.ToString();
            Name = entryAssembly.GetName().Name;
            Health = health;

            BuildInfo = entryAssembly.GetCustomAttribute<BuildInformationAttribute>();
            ProjectInfo = entryAssembly.GetCustomAttribute<ProjectInformationAttribute>();
        }

        [DataMember]
        public string Version { get; }

        [DataMember]
        public string Name { get; }

        [DataMember]
        public BuildInformationAttribute BuildInfo { get; }

        [DataMember]
        public ProjectInformationAttribute ProjectInfo { get; }

        [DataMember]
        public HealthCheckResults Health { get; set; }
    }
}