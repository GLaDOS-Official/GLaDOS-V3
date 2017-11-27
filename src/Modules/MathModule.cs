using Discord.Commands;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GladosV3.Modules
{
    [Name("Math")]
    [Remarks("Do some math I guess")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        [Command("solve")]
        [Summary("solve <math>")]
        [Remarks("Solve's the math problem!")]
        public async Task Solve([Remainder]string number)
        {
            Regex r = new Regex(@"^[0-9-+*\/\. ()]+$", RegexOptions.IgnoreCase);
            if (r.IsMatch(number))
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                var v = dt.Compute(number, "");
                await ReplyAsync(
                    $"Math is solved! The output is: {decimal.Parse(v.ToString()).ToString("N3", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))}");
            }
            else
            {
                await ReplyAsync($"Due to safety our bot, only number operations are allowed, no letters.");
            }
        }
    }
}
