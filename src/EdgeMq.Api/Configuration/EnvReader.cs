using System;

namespace EdgeMq.Api.Configuration;

public static class EnvReader
{
    public static T GetEnvironmentValue<T>(string environmentVariable, T defaultValue) where T : notnull
    {
        var env = Environment.GetEnvironmentVariable(environmentVariable);
        var result = new object();

        switch (defaultValue)
        {
            case uint uintDefaultValue:
                result = uint.TryParse(env, out var uintVal) ? uintVal : uintDefaultValue;
                break;
            case ulong ulongDefaultValue:
                result = ulong.TryParse(env, out var ulongVal) ? ulongVal : ulongDefaultValue;
                break;
            case string stringDefaultValue:
                result = env ?? stringDefaultValue;
                break;
        }

        return (T) result;
    }

}