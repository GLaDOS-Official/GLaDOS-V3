using System;
using System.Data;
using System.Globalization;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;
namespace GladosV3.Modules
{
    [Name("Math")]
    [Remarks("Do some math I guess")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        [Command("solve")]
        [Summary("solve <math>")]
        [Remarks("Solve's the math problem!")]
        public async Task Solve([Remainder] string number)
        {
            try
            {
                Expression ex = new Expression(number);
                var done = ex.calculate();
                if (double.IsNaN(done))
                    throw new FormatException("idk");
                await ReplyAsync(
                    $"Math is solved! The output is: {Double.Parse(done.ToString(String.Empty), NumberStyles.Float).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}");
            }
            catch (FormatException)
            {
                await ReplyAsync($@"**Error:** Impossible to solve!");
            }
        }
    }
}
