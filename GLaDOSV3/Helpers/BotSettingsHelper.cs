using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GLaDOSV3.Helpers
{
    public class BotSettingsHelper<T> 
    {
        private static readonly Assembly MainAssembly = Assembly.GetEntryAssembly();
        private static T GetValue(string key, Assembly callingAssembly)
        {
            if (key.Contains("token", StringComparison.OrdinalIgnoreCase) && callingAssembly != MainAssembly) return default;
            using DataTable dt = SqLite.Connection.GetValuesAsync("BotSettings", $"WHERE name IS '{key}'").GetAwaiter().GetResult();
            return (T)dt.Rows[0]["value"];
        }
        private static void SetKey(string key, T value) => SqLite.Connection.SetValueAsync("BotSettings", "value", value, $"WHERE name IS '{key}'").GetAwaiter();

        public T this[string key]
        {
            get => GetValue(key, Assembly.GetCallingAssembly());
            set => SetKey(key, value);
        }
    }
}
