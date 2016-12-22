using System;
using System.Text;

namespace Zopa.ServiceDiagnostics
{
    public static class ExceptionExtensions
    {
        public static string GetFullStackTrace(this Exception exception)
        {
            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
                if (exception != null)
                {
                    stringBuilder.AppendLine("--------inner--------");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
