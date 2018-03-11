using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using GladosV3.Helpers;
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
        public async Task Solve([Remainder] string math = "")
        {
            if (math == "")
            {
                foreach(var message in Tools.splitMessage(mXparser.getHelp()))
                   await ReplyAsync(message);
                return;
            }
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