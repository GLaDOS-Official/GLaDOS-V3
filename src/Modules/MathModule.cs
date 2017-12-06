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
        public async Task Solve([Remainder]string number)
        {
            Regex r = new Regex(@"^[0-9-+*\/\., ()]+$", RegexOptions.IgnoreCase);
           /* if (!r.IsMatch(number))
                await ReplyAsync($"Due to safety to the bot, only number operations are allowed, no letters.");
            else*/
                try
                {
                    /* System.Data.DataTable dt = new System.Data.DataTable
                     {
                         Locale = System.Globalization.CultureInfo.CreateSpecificCulture("en-US")
                     };
                     dt.Columns.Add("",typeof(System.Int64), $"Convert(({number}),'System.Int64')");
                     dt.Rows.Add(dt.NewRow());*/
                    //Double result = Convert.ToDouble(new DataTable().Compute("2147483647 * 2147483647", null));
                    Expression ex = new Expression(number);
                    var done = ex.calculate();
                    if(double.IsNaN(done))
                       throw new FormatException("NaN returned");
                    //  t64 v = Int64.Parse(dt.Rows[0][""].ToString());
                    await ReplyAsync(
                        $"Math is solved! The output is: {Double.Parse(done.ToString(String.Empty), NumberStyles.Float).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}");
                }
                catch (FormatException e)
                {
                    await ReplyAsync($@"**Error:** Impossible to solve ({e.Message})");
                }
        }
    }
}
