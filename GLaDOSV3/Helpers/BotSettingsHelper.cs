using System.Data;

namespace GladosV3.Helpers
{
    public class BotSettingsHelper<T>
    {
        private T GetValue(string key)
        {
            DataTable dt = SqLite.Connection.GetValuesAsync("BotSettings", $"WHERE name IS '{key}'").GetAwaiter().GetResult();
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
