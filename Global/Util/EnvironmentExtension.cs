using System;

namespace Global.Util
{
    public static class EnvironmentExtension
    {

        public static string ThrowingGetEnvironmentVariable(string key, string friendlyName)
        {
            return Environment.GetEnvironmentVariable(
                key,
                EnvironmentVariableTarget.User
            ) ?? throw new Exception($"no {friendlyName} set with the {key} environment variable");
        }

    }
}
