using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace GLaDOSV3.Helpers
{
    public class BotSettingsHelper<T> 
    {
        private static readonly Assembly currentAssembly = Assembly.GetEntryAssembly();
        private static readonly StackTrace st = new StackTrace(false);
        private static T GetValue(string key)
        {
            //TODO: make better
            var sf = st.GetFrame(3);
            var prevMethod = sf.GetMethod();
            if (key.Contains("token", System.StringComparison.OrdinalIgnoreCase) && prevMethod.DeclaringType.Assembly != currentAssembly) return default;
            using DataTable dt = SqLite.Connection.GetValuesAsync("BotSettings", $"WHERE name IS '{key}'").GetAwaiter().GetResult();
            return (T)dt.Rows[0]["value"];
        }
        private static void SetKey(string key, T value) => SqLite.Connection.SetValueAsync("BotSettings", "value", value, $"WHERE name IS '{key}'").GetAwaiter();

        public T this[string key]
        {
            get => GetValue(key);
            set => SetKey(key, value);
        }
    }
}
