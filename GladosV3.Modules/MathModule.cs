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
        [Remarks("solve <math>")]
        [Summary("Solves the math problem!")]
        public async Task Solve([Remainder] string math)
        {
            try
            {
                Expression ex = new Expression(math);
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
