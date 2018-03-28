using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using GladosV3.Attributes;
using org.mariuszgromada.math.mxparser;

namespace GladosV3.Module.Default
{
    [Name("Math")]
    [Remarks("Do some math I guess")]
    public class MathModule : ModuleBase<SocketCommandContext>
    {
        [Command("solve")]
        [Remarks("solve <math>")]
        [Summary("Solves the math problem!")]
        [Timeout(10, 5, Measure.Minutes)]
        public async Task Solve([Remainder] string math = "")
        {
            try
            {
                var done = new Expression(math).calculate();
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