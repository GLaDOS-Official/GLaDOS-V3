using Discord.Commands;
using GladosV3.Attributes;
using org.mariuszgromada.math.mxparser;
using System;
using System.Threading.Tasks;

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
                math = math?.Replace("PI", "pi", StringComparison.OrdinalIgnoreCase).Replace(",","", StringComparison.OrdinalIgnoreCase); 
                var done = new Expression(math).calculate();
                if (double.IsNaN(done))
                    throw new FormatException("idk");
                await this.ReplyAsync(
                    $"Math is solved! The output is: {done}").ConfigureAwait(false);
            }
            catch (FormatException)
            {
                await this.ReplyAsync($@"**Error:** Impossible to solve!").ConfigureAwait(false);
            }
        }
    }
}