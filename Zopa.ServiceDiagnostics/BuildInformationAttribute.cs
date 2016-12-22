using System;
using System.Runtime.Serialization;

namespace Zopa.ServiceDiagnostics
{
    [DataContract]
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class BuildInformationAttribute : Attribute
    {
        public BuildInformationAttribute(string commit, string date, string buildServer, string buildUri)
        {
            Commit = commit;
            Date = date;
            Uri = buildUri;
            Server = buildServer;
        }

        [DataMember(Name = "date")]
        public string Date { get; private set; }

        [DataMember(Name = "server")]
        public string Server { get; private set; }

        [DataMember(Name = "commit")]
        public string Commit { get; private set; }

        [DataMember(Name = "uri")]
        public string Uri { get; private set; }
    }
}
