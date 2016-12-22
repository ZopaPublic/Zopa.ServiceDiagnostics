using System;
using System.Runtime.Serialization;

namespace Zopa.ServiceDiagnostics
{
    [DataContract]
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ProjectInformationAttribute : Attribute
    {
        public ProjectInformationAttribute(string repositoryUri, string email)
        {
            RepositoryUri = repositoryUri;
            Email = email;
        }

        [DataMember(Name = "repositoryUr")]
        public string RepositoryUri { get; private set; }

        [DataMember(Name = "email")]
        public string Email { get; private set; }
    }
}