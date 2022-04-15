using System;

namespace Global.Util
{
    public static class EnvironmentExtension
    {

        public static String ThrowingGetEnvironmentVariable(String key, String friendlyName)
        {
            return Environment.GetEnvironmentVariable(
                key,
                EnvironmentVariableTarget.User
            ) ?? throw new Exception($"no {friendlyName} set with the {key} environment variable");
        }

    }
}
