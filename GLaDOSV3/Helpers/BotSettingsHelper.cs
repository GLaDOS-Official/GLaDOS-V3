﻿using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace GladosV3.Helpers
{
    public class BotSettingsHelper<T> 
    {
        private static Assembly currentAssembly = Assembly.GetEntryAssembly();
        private static StackTrace st = new StackTrace();
        private T GetValue(string key)
        {
            var sf = st.GetFrame(2);
            var prevMethod   = sf.GetMethod();
            if (key.Contains("token", System.StringComparison.OrdinalIgnoreCase) && prevMethod.DeclaringType.Assembly != currentAssembly) return default;
            using DataTable dt = SqLite.Connection.GetValuesAsync("BotSettings", $"WHERE name IS '{key}'").GetAwaiter().GetResult();
            return (T)dt.Rows[0]["value"];
        }
        private void SetKey(string key, T value) => SqLite.Connection.SetValueAsync("BotSettings", "value", value, $"WHERE name IS '{key}'").GetAwaiter();

        public T this[string key]
        {
            get => this.GetValue(key);
            set => this.SetKey(key, value);
        }
    }
}
