using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Fizzler.Systems.HtmlAgilityPack;
using GLaDOSV3.Helpers;
using HtmlAgilityPack;
using Octokit;

namespace GLaDOSV3.Module.WinDocs
{
    public class WinapiModule : ModuleBase<SocketCommandContext>
    {
        private Random rnd = new Random();
        [Command("win32", RunMode = RunMode.Async)]
        [Remarks("win32 <winapi>")]
        [Summary("Winapi search!")]
        public async Task WinapiSearch([Remainder] string winapi)
        {
            var typing = Context.Channel.TriggerTypingAsync();
            try
            {
                var github = new GitHubClient(new ProductHeaderValue("GLaDOS_V3"))
                {
                    Credentials = new Credentials("dddbe430ea9eb99c131ebbb60b6c8e1507731496")
                };
                var searchResult =
                    (await
                         github.Search.SearchCode(new
                                                      SearchCodeRequest($"{winapi} repo:MicrosoftDocs/win32 repo:MicrosoftDocs/windows-driver-docs-ddi repo:MicrosoftDocs/windows-driver-docs",
                                                                        "MicrosoftDocs", "win32"))).Items.ToArray();
                if (searchResult.Length == 0)
                {
                    await this.ReplyAsync("Windows function not found!");
                    return;
                }

                string resultString = "";
                foreach (var gitResult in searchResult)
                {
                    var result =
                        await github.Repository.Content.GetAllContents(gitResult.Repository.Id, gitResult.Path);
                    string content = result[0].Content.ToLowerInvariant();
                    var index = content.IndexOf($"[**{winapi.ToLowerInvariant()}**]", StringComparison.Ordinal)
                                     + 3;
                    if (index <= 10)
                        index = content.IndexOf($">{winapi.ToLowerInvariant()}</a>", StringComparison.Ordinal) + 1;

                    if (index <= 10) continue;
                    Regex r = new Regex($"<a[^>]+href=[\"'](.*?)[\"']>{winapi.ToLowerInvariant()}<\\/a>",
                                        RegexOptions.Compiled | RegexOptions.CultureInvariant);
                    MatchCollection m = r.Matches(content);
                    if (m.Count != 0)
                    {
                        resultString = m[0].Groups[1].Value;
                        break;
                    }

                    resultString = result[0].Content[index..];
                    resultString = resultString[(resultString.IndexOf("**](", StringComparison.Ordinal) + 4)..];
                    if (resultString.StartsWith("https://"))
                    {
                        index = resultString.IndexOf(".aspx") + 5;
                        if (index >= 10) resultString = resultString.Substring(0, index);
                        index = resultString.IndexOf("),", StringComparison.Ordinal);
                        if (index >= 10) resultString = resultString.Substring(0, index);
                        index = resultString.IndexOf(") ", StringComparison.Ordinal);
                        if (index >= 10) resultString = resultString.Substring(0, index);
                        index = resultString.IndexOf(',', StringComparison.Ordinal);
                        if (index >= 10) resultString = resultString.Substring(0, index);
                        index = resultString.IndexOf('\n', StringComparison.Ordinal);
                        if (index >= 10) resultString = resultString.Substring(0, index);

                    }
                    else
                    {
                        resultString = resultString.Substring(0, resultString.IndexOf(')', StringComparison.Ordinal));
                        resultString =
                            resultString[(resultString.IndexOf("/api/", StringComparison.Ordinal) + 5)..];
                        resultString = $"https://docs.microsoft.com/en-us/windows/win32/api/{resultString}";
                    }

                    break;
                }

                if (string.IsNullOrWhiteSpace(resultString))
                {
                    await this.ReplyAsync("Windows function not found!");
                    return;
                }

                var web    = await new HtmlWeb().LoadFromWebAsync(resultString);
                
                var syntax = web.DocumentNode.QuerySelector("#main > pre > code")?.InnerText;;
                EmbedBuilder builder = new EmbedBuilder
                {
                    Color = new Color(this.rnd.Next(256), this.rnd.Next(256), this.rnd.Next(256)), Footer =
                        new EmbedFooterBuilder
                        {
                            Text = $"Requested by {Context.User.Username}#{Context.User.Discriminator}",
                            IconUrl = Context.User.GetAvatarUrl()
                        },
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = "https://www.freepngimg.com/download/microsoft_windows/4-2-microsoft-windows-png-pic.png",
                        Name = $"Microsoft Docs",
                        Url = resultString
                    },
                    Description = await GetRequirements(web.DocumentNode)
                };
                builder.AddField("Syntax", $"```cpp\n{syntax ?? "fuck me error"}\n```");
                var embeds = Tools.SplitMessage((await GetParameters(web.DocumentNode)), 1024);
                for (var i = 0; i < embeds.Length; i++)
                {
                    builder.AddField((i == 0 ? "Parameters" : "\u200B"), embeds[i]);
                }
                {
                    string[] words = (syntax ?? "fuck me error").ReduceWhitespace().Replace("\n", "").Replace("\r", "").Split(' ');
                    words = words.Where(f => !f.StartsWith("__")).ToArray();
                    words = words.Where(f => !f.EndsWith("API")).ToArray();
                    string type                                 = words[0];
                    string name                                 = words[1][..^1];
                    string args                                 = "";
                    for (int i = 2; i < words.Length; i++) args += $"{words[i]} ";
                    args = args[..^5];
                    builder.AddField("Typedef **(BETA)**", $"```cpp\ntypedef {type}(*{name}_t)({args});\n```");
                }
                await this.ReplyAsync(embed: builder.Build());
            }
            catch (ForbiddenException e) { await this.ReplyAsync(e.ToString()); }
            finally
            {
                typing.Dispose();
            }
        }
        private async Task<string> GetParameters(HtmlNode document)
        {
            var parameters = document.QuerySelector("#parameters");
            var response = "";
            int i = 0;
            while (parameters.NextSibling != null && parameters.NextSibling.Id != "return-value")
            {
                parameters = parameters.NextSibling;
                if (parameters.InnerHtml == "\n") continue;
                if (i++ % 2 == 0 && parameters.Name != "table") { response += $"**{parameters.InnerText}**: "; continue; }
                if (parameters.Name == "table")
                {
                    string fix = "";
                    foreach (HtmlNode row in parameters.SelectNodes("tr"))
                    {
                        ///This is the row.
                        foreach (HtmlNode cell in row.SelectNodes("th|td"))
                        {
                            if (cell.InnerHtml.ToLowerInvariant() == "meaning" || cell.InnerHtml.ToLowerInvariant() == "value") break;
                            if (cell.Attributes.Count == 1 && cell.Attributes.First().Value == "40%") fix += $"\n{await cleanupString(cell.InnerHtml)}\n";
                            else fix += $"{await cleanupString(cell.InnerHtml)}";
                            ///This the cell.
                        }
                    }
                    response += $"\n*{fix}*\n";
                } 
                else response += $"{await cleanupString(parameters.InnerHtml)}\n";
            }
            return response;    
        }
        private async Task<string> cleanupString(String fix)
        {
            fix = fix.Replace("*", "\\*").Replace("\n", " ").Trim();
            Regex r = new Regex("<\\s*a href=\"(.*?)\"[^>]*>(.*?)<\\s*\\/\\s*a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var matches = r.Matches(fix);
            foreach (Match t in matches) fix = fix.Replace(t.Groups[0].Value, $"[{t.Groups[2]}](https://docs.microsoft.com/{t.Groups[1]})");
            fix = Regex.Replace(fix, "<a[^>]+id=\"(.*?)\"[^>]*>(.*?)</a>", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            r = new Regex("<p>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            matches = r.Matches(fix);
            foreach (Match t in matches) fix = fix.Replace(t.Groups[0].Value, $"\n{t.Groups[1]}");
            if (fix.StartsWith("<dl> <dt>"))
            {
                fix = fix[9..];
                fix = fix.Replace("</dt>", "");
                fix = fix.Replace("<dt>", "(").Replace(" </dl>", ")");
            }
            fix = fix.Replace("<code>", "``", StringComparison.OrdinalIgnoreCase).Replace("</code>", "``", StringComparison.OrdinalIgnoreCase);
            fix = fix.Replace("<i>", "*", StringComparison.OrdinalIgnoreCase).Replace("</i>", "*", StringComparison.OrdinalIgnoreCase);
            r = new Regex("<div(.*?)>(.*?)</div>", RegexOptions.Singleline | RegexOptions.Compiled);
            matches = r.Matches(fix);
            foreach (Match t in matches) fix = fix.Replace(t.Groups[0].Value, $"{t.Groups[2]}");
            r = new Regex("<b>(.*?)</b>", RegexOptions.Singleline | RegexOptions.Compiled);
            matches = r.Matches(fix);
            foreach (Match t in matches) fix = fix.Replace(t.Groups[0].Value, $"**{t.Groups[1]}**");
            return fix;
        }
        private async Task<string> GetRequirements(HtmlNode document)
        {
            var requirements = document.QuerySelector("#requirements");
            requirements = requirements.NextSibling.NextSibling;
            var stringReq = requirements.InnerText.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            var reqString = "";
            for (var i = 0; i < stringReq.Length; i++)
            {
                var t = stringReq[i];
                int start = t.LastIndexOf("[") + "[".Length;
                int end = t.IndexOf("]", start);
                string result = t.Remove((start <= 0 ? 0 : start), (end - start <= 0 ? 0 : end - start)).Replace(" []", "");
                if (i % 2 == 0) reqString += $"**{result}**: ";
                else reqString += $"{result}\n";
            }
            return reqString;
        }
    }
}
