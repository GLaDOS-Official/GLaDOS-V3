using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLaDOSV3.Models
{
    public class AnsiColor
    {
        private const    string ESC = $"\u001B";
        private const    string CSI = $"\u001B[";
        private const    string BEL = $"\u0007";
        private readonly string value;

        AnsiColor(string value) => this.value = value;

        public override string ToString() => this.value;
        public class Cursor
        {
            /// <summary>
            /// 	Cursor up by <paramref name="n"/> columns
            /// </summary>
            /// <returns></returns>
            public static AnsiColor Up(ushort n) => new AnsiColor($"{CSI}{n}A");
            /// <summary>
            /// 	Cursor down by <paramref name="n"/> columns
            /// </summary>
            /// <returns></returns>
            public static AnsiColor Down(ushort n) => new AnsiColor($"{CSI}{n}B");
            /// <summary>
            /// 	Cursor right by <paramref name="n"/> columns
            /// </summary>
            /// <returns></returns>
            public static AnsiColor Right(ushort n) => new AnsiColor($"{CSI}{n}C");
            /// <summary>
            /// 	Cursor left by <paramref name="n"/> columns
            /// </summary>
            /// <returns></returns>
            public static AnsiColor Left(ushort n) => new AnsiColor($"{CSI}{n}D");
            /// <summary>
            /// Go to row <paramref name="row"/>
            /// </summary>
            /// <returns></returns>
            public static AnsiColor GoTo(ushort row) => new AnsiColor($"{CSI}{row}G");
            /// <summary>
            /// 	Go to <paramref name="x"/> <paramref name="y"/> position
            /// </summary>
            /// <returns></returns>
            public static AnsiColor GoTo(ushort x, int y) => new AnsiColor($"{CSI}{y};{x}H");
            /// <summary>
            /// Go to home (set cursor to 0 / 0)
            /// </summary>
            public static AnsiColor Home => new AnsiColor($"{CSI}H");
            /// <summary>
            /// Go to end
            /// </summary>
            public static AnsiColor End => new AnsiColor($"{CSI}F");
            /// <summary>
            ///  Saves the current cursor position
            /// </summary>
            public static AnsiColor SavePosition => new AnsiColor($"{CSI}s");
            /// <summary>
            /// Restores the cursor to the last saved position
            /// </summary>
            public static AnsiColor RestorePosition => new AnsiColor($"{CSI}u");
            /// <summary>
            /// 	Text Cursor Enable Blinking
            /// </summary>
            public static AnsiColor EnableBlinking => new AnsiColor($"{CSI}?12h");
            /// <summary>
            /// 	Text Cursor Disable Blinking
            /// </summary>
            public static AnsiColor DisableBlinking => new AnsiColor($"{CSI}?12l");
            /// <summary>
            /// 	Text Cursor Enable Mode Show
            /// </summary>
            public static AnsiColor ShowCursor => new AnsiColor($"{CSI}?25h");
            /// <summary>
            /// 	Text Cursor Enable Mode Hide
            /// </summary>
            public static AnsiColor HideCursor => new AnsiColor($"{CSI}?25l");
        }
        public class Decoration
        {
            /// <summary>
            /// Sets bold, underline and inverse mode off
            /// </summary>
            public static AnsiColor Reset => new AnsiColor($"{CSI}0m");
            /// <summary>
            /// Sets bold mode on
            /// </summary>
            public static AnsiColor Bold => new AnsiColor($"{CSI}1m");
            /// <summary>
            /// Sets underline mode on
            /// </summary>
            public static AnsiColor Underline => new AnsiColor($"{CSI}4m");
            /// <summary>
            /// 	Swaps foreground and background colors
            /// </summary>
            public static AnsiColor Negative => new AnsiColor($"{CSI}7m");
            /// <summary>
            /// 	Returns foreground/background to normal
            /// </summary>
            public static AnsiColor Positive => new AnsiColor($"{CSI}27m");
        }
        public class Screen
        {
            /// <summary>
            /// Clears screen from cursor to end of line
            /// </summary>
            public static AnsiColor ClearTillEnd => new AnsiColor($"{CSI}0J");
            /// <summary>
            /// Clears screen from cursor to start of line
            /// </summary>
            public static AnsiColor ClearToBeginning => new AnsiColor($"{CSI}1J");
            /// <summary>
            /// Clears the entire screen
            /// </summary>
            public static AnsiColor ClearScreen => new AnsiColor($"{CSI}2J");
            /// <summary>
            /// Scroll text up by <paramref name="n"/>. Also known as pan down, new lines fill in from the bottom of the screen
            /// </summary>
            public static AnsiColor ScrollUp(ushort n) => new AnsiColor($"{CSI}{n}S");
            /// <summary>
            /// Scroll text down by <paramref name="n"/>. Also known as pan down, new lines fill in from the bottom of the screen
            /// </summary>
            public static AnsiColor ScrollDown(ushort n) => new AnsiColor($"{CSI}{n}T");
            /// <summary>
            /// Sets the console window’s title to <string>.
            /// </summary>
            public static AnsiColor SetConsoleTitle(string title) => new AnsiColor($"{ESC}]2;{title}{BEL}");

        }
        public class Line
        {
            /// <summary>
            /// Clears line from cursor to end of line
            /// </summary>
            public static AnsiColor ClearToEOL => new AnsiColor($"{CSI}0K");
            /// <summary>
            /// Clears line from cursor to start of line
            /// </summary>
            public static AnsiColor ClearToStart => new AnsiColor($"{CSI}1K");
            /// <summary>
            /// Clears the entire line
            /// </summary>
            public static AnsiColor ClearLine => new AnsiColor($"{CSI}2K");
            /// <summary>
            ///  Moves cursor to beginning of line by <paramref name="n"/> lines down
            /// </summary>            /// <returns></returns>
            public static AnsiColor NextLine(ushort n) => new AnsiColor($"{CSI}{n}E");
            /// <summary>
            ///  Moves cursor to beginning of line by <paramref name="n"/> lines up
            /// </summary>
            /// <returns></returns>
            public static AnsiColor PrevLine(ushort n) => new AnsiColor($"{CSI}{n}F");
        }
        public class Foreground
        {
            public static AnsiColor Black => new AnsiColor($"{CSI}38;5;0m");
            public static AnsiColor Maroon => new AnsiColor($"{CSI}38;5;1m");
            public static AnsiColor Green => new AnsiColor($"{CSI}38;5;2m");
            public static AnsiColor Olive => new AnsiColor($"{CSI}38;5;3m");
            public static AnsiColor Navy => new AnsiColor($"{CSI}38;5;4m");
            public static AnsiColor Purple => new AnsiColor($"{CSI}38;5;5m");
            public static AnsiColor Teal => new AnsiColor($"{CSI}38;5;6m");
            public static AnsiColor Silver => new AnsiColor($"{CSI}38;5;7m");
            public static AnsiColor Grey => new AnsiColor($"{CSI}38;5;8m");
            public static AnsiColor Red => new AnsiColor($"{CSI}38;5;9m");
            public static AnsiColor Lime => new AnsiColor($"{CSI}38;5;10m");
            public static AnsiColor Yellow => new AnsiColor($"{CSI}38;5;11m");
            public static AnsiColor Blue => new AnsiColor($"{CSI}38;5;12m");
            public static AnsiColor Fuchsia => new AnsiColor($"{CSI}38;5;13m");
            public static AnsiColor Aqua => new AnsiColor($"{CSI}38;5;14m");
            public static AnsiColor White => new AnsiColor($"{CSI}38;5;15m");
            public static AnsiColor Grey0 => new AnsiColor($"{CSI}38;5;16m");
            public static AnsiColor NavyBlue => new AnsiColor($"{CSI}38;5;17m");
            public static AnsiColor DarkBlue => new AnsiColor($"{CSI}38;5;18m");
            public static AnsiColor Blue3 => new AnsiColor($"{CSI}38;5;19m");
            public static AnsiColor Blue1 => new AnsiColor($"{CSI}38;5;21m");
            public static AnsiColor DarkGreen => new AnsiColor($"{CSI}38;5;22m");
            public static AnsiColor DeepSkyBlue4 => new AnsiColor($"{CSI}38;5;23m");
            public static AnsiColor DodgerBlue3 => new AnsiColor($"{CSI}38;5;26m");
            public static AnsiColor DodgerBlue2 => new AnsiColor($"{CSI}38;5;27m");
            public static AnsiColor Green4 => new AnsiColor($"{CSI}38;5;28m");
            public static AnsiColor SpringGreen4 => new AnsiColor($"{CSI}38;5;29m");
            public static AnsiColor Turquoise4 => new AnsiColor($"{CSI}38;5;30m");
            public static AnsiColor DeepSkyBlue3 => new AnsiColor($"{CSI}38;5;31m");
            public static AnsiColor DodgerBlue1 => new AnsiColor($"{CSI}38;5;33m");
            public static AnsiColor Green3 => new AnsiColor($"{CSI}38;5;34m");
            public static AnsiColor SpringGreen3 => new AnsiColor($"{CSI}38;5;35m");
            public static AnsiColor DarkCyan => new AnsiColor($"{CSI}38;5;36m");
            public static AnsiColor LightSeaGreen => new AnsiColor($"{CSI}38;5;37m");
            public static AnsiColor DeepSkyBlue2 => new AnsiColor($"{CSI}38;5;38m");
            public static AnsiColor DeepSkyBlue1 => new AnsiColor($"{CSI}38;5;39m");
            public static AnsiColor SpringGreen2 => new AnsiColor($"{CSI}38;5;42m");
            public static AnsiColor Cyan3 => new AnsiColor($"{CSI}38;5;43m");
            public static AnsiColor DarkTurquoise => new AnsiColor($"{CSI}38;5;44m");
            public static AnsiColor Turquoise2 => new AnsiColor($"{CSI}38;5;45m");
            public static AnsiColor Green1 => new AnsiColor($"{CSI}38;5;46m");
            public static AnsiColor SpringGreen1 => new AnsiColor($"{CSI}38;5;48m");
            public static AnsiColor MediumSpringGreen => new AnsiColor($"{CSI}38;5;49m");
            public static AnsiColor Cyan2 => new AnsiColor($"{CSI}38;5;50m");
            public static AnsiColor Cyan1 => new AnsiColor($"{CSI}38;5;51m");
            public static AnsiColor DarkRed => new AnsiColor($"{CSI}38;5;52m");
            public static AnsiColor DeepPink4 => new AnsiColor($"{CSI}38;5;53m");
            public static AnsiColor Purple4 => new AnsiColor($"{CSI}38;5;54m");
            public static AnsiColor Purple3 => new AnsiColor($"{CSI}38;5;56m");
            public static AnsiColor BlueViolet => new AnsiColor($"{CSI}38;5;57m");
            public static AnsiColor Orange4 => new AnsiColor($"{CSI}38;5;58m");
            public static AnsiColor Grey37 => new AnsiColor($"{CSI}38;5;59m");
            public static AnsiColor MediumPurple4 => new AnsiColor($"{CSI}38;5;60m");
            public static AnsiColor SlateBlue3 => new AnsiColor($"{CSI}38;5;61m");
            public static AnsiColor RoyalBlue1 => new AnsiColor($"{CSI}38;5;63m");
            public static AnsiColor Chartreuse4 => new AnsiColor($"{CSI}38;5;64m");
            public static AnsiColor DarkSeaGreen4 => new AnsiColor($"{CSI}38;5;65m");
            public static AnsiColor PaleTurquoise4 => new AnsiColor($"{CSI}38;5;66m");
            public static AnsiColor SteelBlue => new AnsiColor($"{CSI}38;5;67m");
            public static AnsiColor SteelBlue3 => new AnsiColor($"{CSI}38;5;68m");
            public static AnsiColor CornflowerBlue => new AnsiColor($"{CSI}38;5;69m");
            public static AnsiColor Chartreuse3 => new AnsiColor($"{CSI}38;5;70m");
            public static AnsiColor CadetBlue => new AnsiColor($"{CSI}38;5;72m");
            public static AnsiColor SkyBlue3 => new AnsiColor($"{CSI}38;5;74m");
            public static AnsiColor SteelBlue1 => new AnsiColor($"{CSI}38;5;75m"); public static AnsiColor PaleGreen3 => new AnsiColor($"{CSI}38;5;77m");
            public static AnsiColor SeaGreen3 => new AnsiColor($"{CSI}38;5;78m");
            public static AnsiColor Aquamarine3 => new AnsiColor($"{CSI}38;5;79m");
            public static AnsiColor MediumTurquoise => new AnsiColor($"{CSI}38;5;80m");
            public static AnsiColor Chartreuse2 => new AnsiColor($"{CSI}38;5;82m");
            public static AnsiColor SeaGreen2 => new AnsiColor($"{CSI}38;5;83m");
            public static AnsiColor SeaGreen1 => new AnsiColor($"{CSI}38;5;84m");
            public static AnsiColor Aquamarine1 => new AnsiColor($"{CSI}38;5;86m");
            public static AnsiColor DarkSlateGray2 => new AnsiColor($"{CSI}38;5;87m");
            public static AnsiColor DarkMagenta => new AnsiColor($"{CSI}38;5;90m");
            public static AnsiColor DarkViolet => new AnsiColor($"{CSI}38;5;92m");
            public static AnsiColor LightPink4 => new AnsiColor($"{CSI}38;5;95m");
            public static AnsiColor Plum4 => new AnsiColor($"{CSI}38;5;96m");
            public static AnsiColor MediumPurple3 => new AnsiColor($"{CSI}38;5;97m");
            public static AnsiColor SlateBlue1 => new AnsiColor($"{CSI}38;5;99m");
            public static AnsiColor Yellow4 => new AnsiColor($"{CSI}38;5;100m");
            public static AnsiColor Wheat4 => new AnsiColor($"{CSI}38;5;101m");
            public static AnsiColor Grey53 => new AnsiColor($"{CSI}38;5;102m");
            public static AnsiColor LightSlateGrey => new AnsiColor($"{CSI}38;5;103m");
            public static AnsiColor MediumPurple => new AnsiColor($"{CSI}38;5;104m");
            public static AnsiColor LightSlateBlue => new AnsiColor($"{CSI}38;5;105m");
            public static AnsiColor DarkOliveGreen3 => new AnsiColor($"{CSI}38;5;107m");
            public static AnsiColor DarkSeaGreen => new AnsiColor($"{CSI}38;5;108m");
            public static AnsiColor LightSkyBlue3 => new AnsiColor($"{CSI}38;5;109m");
            public static AnsiColor SkyBlue2 => new AnsiColor($"{CSI}38;5;111m");
            public static AnsiColor DarkSeaGreen3 => new AnsiColor($"{CSI}38;5;115m");
            public static AnsiColor DarkSlateGray3 => new AnsiColor($"{CSI}38;5;116m");
            public static AnsiColor SkyBlue1 => new AnsiColor($"{CSI}38;5;117m");
            public static AnsiColor Chartreuse1 => new AnsiColor($"{CSI}38;5;118m");
            public static AnsiColor LightGreen => new AnsiColor($"{CSI}38;5;119m");
            public static AnsiColor PaleGreen1 => new AnsiColor($"{CSI}38;5;121m");
            public static AnsiColor DarkSlateGray1 => new AnsiColor($"{CSI}38;5;123m");
            public static AnsiColor Red3 => new AnsiColor($"{CSI}38;5;124m");
            public static AnsiColor MediumVioletRed => new AnsiColor($"{CSI}38;5;126m");
            public static AnsiColor Magenta3 => new AnsiColor($"{CSI}38;5;127m");
            public static AnsiColor DarkOrange3 => new AnsiColor($"{CSI}38;5;130m");
            public static AnsiColor IndianRed => new AnsiColor($"{CSI}38;5;131m");
            public static AnsiColor HotPink3 => new AnsiColor($"{CSI}38;5;132m");
            public static AnsiColor MediumOrchid3 => new AnsiColor($"{CSI}38;5;133m");
            public static AnsiColor MediumOrchid => new AnsiColor($"{CSI}38;5;134m");
            public static AnsiColor MediumPurple2 => new AnsiColor($"{CSI}38;5;135m");
            public static AnsiColor DarkGoldenrod => new AnsiColor($"{CSI}38;5;136m");
            public static AnsiColor LightSalmon3 => new AnsiColor($"{CSI}38;5;137m");
            public static AnsiColor RosyBrown => new AnsiColor($"{CSI}38;5;138m");
            public static AnsiColor Grey63 => new AnsiColor($"{CSI}38;5;139m");
            public static AnsiColor MediumPurple1 => new AnsiColor($"{CSI}38;5;141m");
            public static AnsiColor Gold3 => new AnsiColor($"{CSI}38;5;142m");
            public static AnsiColor DarkKhaki => new AnsiColor($"{CSI}38;5;143m");
            public static AnsiColor NavajoWhite3 => new AnsiColor($"{CSI}38;5;144m");
            public static AnsiColor Grey69 => new AnsiColor($"{CSI}38;5;145m");
            public static AnsiColor LightSteelBlue3 => new AnsiColor($"{CSI}38;5;146m");
            public static AnsiColor LightSteelBlue => new AnsiColor($"{CSI}38;5;147m");
            public static AnsiColor Yellow3 => new AnsiColor($"{CSI}38;5;148m");
            public static AnsiColor DarkSeaGreen2 => new AnsiColor($"{CSI}38;5;151m");
            public static AnsiColor LightCyan3 => new AnsiColor($"{CSI}38;5;152m");
            public static AnsiColor LightSkyBlue1 => new AnsiColor($"{CSI}38;5;153m");
            public static AnsiColor GreenYellow => new AnsiColor($"{CSI}38;5;154m");
            public static AnsiColor DarkOliveGreen2 => new AnsiColor($"{CSI}38;5;155m");
            public static AnsiColor DarkSeaGreen1 => new AnsiColor($"{CSI}38;5;158m");
            public static AnsiColor PaleTurquoise1 => new AnsiColor($"{CSI}38;5;159m");
            public static AnsiColor DeepPink3 => new AnsiColor($"{CSI}38;5;161m");
            public static AnsiColor Magenta2 => new AnsiColor($"{CSI}38;5;165m");
            public static AnsiColor HotPink2 => new AnsiColor($"{CSI}38;5;169m");
            public static AnsiColor Orchid => new AnsiColor($"{CSI}38;5;170m");
            public static AnsiColor MediumOrchid1 => new AnsiColor($"{CSI}38;5;171m");
            public static AnsiColor Orange3 => new AnsiColor($"{CSI}38;5;172m");
            public static AnsiColor LightPink3 => new AnsiColor($"{CSI}38;5;174m");
            public static AnsiColor Pink3 => new AnsiColor($"{CSI}38;5;175m");
            public static AnsiColor Plum3 => new AnsiColor($"{CSI}38;5;176m");
            public static AnsiColor Violet => new AnsiColor($"{CSI}38;5;177m");
            public static AnsiColor LightGoldenrod3 => new AnsiColor($"{CSI}38;5;179m");
            public static AnsiColor Tan => new AnsiColor($"{CSI}38;5;180m");
            public static AnsiColor MistyRose3 => new AnsiColor($"{CSI}38;5;181m");
            public static AnsiColor Thistle3 => new AnsiColor($"{CSI}38;5;182m");
            public static AnsiColor Plum2 => new AnsiColor($"{CSI}38;5;183m");
            public static AnsiColor Khaki3 => new AnsiColor($"{CSI}38;5;185m");
            public static AnsiColor LightGoldenrod2 => new AnsiColor($"{CSI}38;5;186m");
            public static AnsiColor LightYellow3 => new AnsiColor($"{CSI}38;5;187m");
            public static AnsiColor Grey84 => new AnsiColor($"{CSI}38;5;188m");
            public static AnsiColor LightSteelBlue1 => new AnsiColor($"{CSI}38;5;189m");
            public static AnsiColor Yellow2 => new AnsiColor($"{CSI}38;5;190m");
            public static AnsiColor DarkOliveGreen1 => new AnsiColor($"{CSI}38;5;191m");
            public static AnsiColor Honeydew2 => new AnsiColor($"{CSI}38;5;194m");
            public static AnsiColor LightCyan1 => new AnsiColor($"{CSI}38;5;195m");
            public static AnsiColor Red1 => new AnsiColor($"{CSI}38;5;196m");
            public static AnsiColor DeepPink2 => new AnsiColor($"{CSI}38;5;197m");
            public static AnsiColor DeepPink1 => new AnsiColor($"{CSI}38;5;198m");
            public static AnsiColor Magenta1 => new AnsiColor($"{CSI}38;5;201m");
            public static AnsiColor OrangeRed1 => new AnsiColor($"{CSI}38;5;202m");
            public static AnsiColor IndianRed1 => new AnsiColor($"{CSI}38;5;203m");
            public static AnsiColor HotPink => new AnsiColor($"{CSI}38;5;205m");
            public static AnsiColor DarkOrange => new AnsiColor($"{CSI}38;5;208m");
            public static AnsiColor Salmon1 => new AnsiColor($"{CSI}38;5;209m");
            public static AnsiColor LightCoral => new AnsiColor($"{CSI}38;5;210m");
            public static AnsiColor PaleVioletRed1 => new AnsiColor($"{CSI}38;5;211m");
            public static AnsiColor Orchid2 => new AnsiColor($"{CSI}38;5;212m");
            public static AnsiColor Orchid1 => new AnsiColor($"{CSI}38;5;213m");
            public static AnsiColor Orange1 => new AnsiColor($"{CSI}38;5;214m");
            public static AnsiColor SandyBrown => new AnsiColor($"{CSI}38;5;215m");
            public static AnsiColor LightSalmon1 => new AnsiColor($"{CSI}38;5;216m");
            public static AnsiColor LightPink1 => new AnsiColor($"{CSI}38;5;217m");
            public static AnsiColor Pink1 => new AnsiColor($"{CSI}38;5;218m");
            public static AnsiColor Plum1 => new AnsiColor($"{CSI}38;5;219m");
            public static AnsiColor Gold1 => new AnsiColor($"{CSI}38;5;220m");
            public static AnsiColor NavajoWhite1 => new AnsiColor($"{CSI}38;5;223m");
            public static AnsiColor MistyRose1 => new AnsiColor($"{CSI}38;5;224m");
            public static AnsiColor Thistle1 => new AnsiColor($"{CSI}38;5;225m");
            public static AnsiColor Yellow1 => new AnsiColor($"{CSI}38;5;226m");
            public static AnsiColor LightGoldenrod1 => new AnsiColor($"{CSI}38;5;227m");
            public static AnsiColor Khaki1 => new AnsiColor($"{CSI}38;5;228m");
            public static AnsiColor Wheat1 => new AnsiColor($"{CSI}38;5;229m");
            public static AnsiColor Cornsilk => new AnsiColor($"{CSI}38;5;230m");
            public static AnsiColor Grey100 => new AnsiColor($"{CSI}38;5;231m");
            public static AnsiColor Grey3 => new AnsiColor($"{CSI}38;5;232m");
            public static AnsiColor Grey7 => new AnsiColor($"{CSI}38;5;233m");
            public static AnsiColor Grey11 => new AnsiColor($"{CSI}38;5;234m");
            public static AnsiColor Grey15 => new AnsiColor($"{CSI}38;5;235m");
            public static AnsiColor Grey19 => new AnsiColor($"{CSI}38;5;236m");
            public static AnsiColor Grey23 => new AnsiColor($"{CSI}38;5;237m");
            public static AnsiColor Grey27 => new AnsiColor($"{CSI}38;5;238m");
            public static AnsiColor Grey30 => new AnsiColor($"{CSI}38;5;239m");
            public static AnsiColor Grey35 => new AnsiColor($"{CSI}38;5;240m");
            public static AnsiColor Grey39 => new AnsiColor($"{CSI}38;5;241m");
            public static AnsiColor Grey42 => new AnsiColor($"{CSI}38;5;242m");
            public static AnsiColor Grey46 => new AnsiColor($"{CSI}38;5;243m");
            public static AnsiColor Grey50 => new AnsiColor($"{CSI}38;5;244m");
            public static AnsiColor Grey54 => new AnsiColor($"{CSI}38;5;245m");
            public static AnsiColor Grey58 => new AnsiColor($"{CSI}38;5;246m");
            public static AnsiColor Grey62 => new AnsiColor($"{CSI}38;5;247m");
            public static AnsiColor Grey66 => new AnsiColor($"{CSI}38;5;248m");
            public static AnsiColor Grey70 => new AnsiColor($"{CSI}38;5;249m");
            public static AnsiColor Grey74 => new AnsiColor($"{CSI}38;5;250m");
            public static AnsiColor Grey78 => new AnsiColor($"{CSI}38;5;251m");
            public static AnsiColor Grey82 => new AnsiColor($"{CSI}38;5;252m");
            public static AnsiColor Grey85 => new AnsiColor($"{CSI}38;5;253m");
            public static AnsiColor Grey89 => new AnsiColor($"{CSI}38;5;254m");
            public static AnsiColor Grey93 => new AnsiColor($"{CSI}38;5;255m");
        }
        public class Background
        {
            public static AnsiColor Black => new AnsiColor($"{CSI}48;5;0m");
            public static AnsiColor Maroon => new AnsiColor($"{CSI}48;5;1m");
            public static AnsiColor Green => new AnsiColor($"{CSI}48;5;2m");
            public static AnsiColor Olive => new AnsiColor($"{CSI}48;5;3m");
            public static AnsiColor Navy => new AnsiColor($"{CSI}48;5;4m");
            public static AnsiColor Purple => new AnsiColor($"{CSI}48;5;5m");
            public static AnsiColor Teal => new AnsiColor($"{CSI}48;5;6m");
            public static AnsiColor Silver => new AnsiColor($"{CSI}48;5;7m");
            public static AnsiColor Grey => new AnsiColor($"{CSI}48;5;8m");
            public static AnsiColor Red => new AnsiColor($"{CSI}48;5;9m");
            public static AnsiColor Lime => new AnsiColor($"{CSI}48;5;10m");
            public static AnsiColor Yellow => new AnsiColor($"{CSI}48;5;11m");
            public static AnsiColor Blue => new AnsiColor($"{CSI}48;5;12m");
            public static AnsiColor Fuchsia => new AnsiColor($"{CSI}48;5;13m");
            public static AnsiColor Aqua => new AnsiColor($"{CSI}48;5;14m");
            public static AnsiColor White => new AnsiColor($"{CSI}48;5;15m");
            public static AnsiColor Grey0 => new AnsiColor($"{CSI}48;5;16m");
            public static AnsiColor NavyBlue => new AnsiColor($"{CSI}48;5;17m");
            public static AnsiColor DarkBlue => new AnsiColor($"{CSI}48;5;18m");
            public static AnsiColor Blue3 => new AnsiColor($"{CSI}48;5;19m");
            public static AnsiColor Blue1 => new AnsiColor($"{CSI}48;5;21m");
            public static AnsiColor DarkGreen => new AnsiColor($"{CSI}48;5;22m");
            public static AnsiColor DeepSkyBlue4 => new AnsiColor($"{CSI}48;5;23m");
            public static AnsiColor DodgerBlue3 => new AnsiColor($"{CSI}48;5;26m");
            public static AnsiColor DodgerBlue2 => new AnsiColor($"{CSI}48;5;27m");
            public static AnsiColor Green4 => new AnsiColor($"{CSI}48;5;28m");
            public static AnsiColor SpringGreen4 => new AnsiColor($"{CSI}48;5;29m");
            public static AnsiColor Turquoise4 => new AnsiColor($"{CSI}48;5;30m");
            public static AnsiColor DeepSkyBlue3 => new AnsiColor($"{CSI}48;5;31m");
            public static AnsiColor DodgerBlue1 => new AnsiColor($"{CSI}48;5;33m");
            public static AnsiColor Green3 => new AnsiColor($"{CSI}48;5;34m");
            public static AnsiColor SpringGreen3 => new AnsiColor($"{CSI}48;5;35m");
            public static AnsiColor DarkCyan => new AnsiColor($"{CSI}48;5;36m");
            public static AnsiColor LightSeaGreen => new AnsiColor($"{CSI}48;5;37m");
            public static AnsiColor DeepSkyBlue2 => new AnsiColor($"{CSI}48;5;38m");
            public static AnsiColor DeepSkyBlue1 => new AnsiColor($"{CSI}48;5;39m");
            public static AnsiColor SpringGreen2 => new AnsiColor($"{CSI}48;5;42m");
            public static AnsiColor Cyan3 => new AnsiColor($"{CSI}48;5;43m");
            public static AnsiColor DarkTurquoise => new AnsiColor($"{CSI}48;5;44m");
            public static AnsiColor Turquoise2 => new AnsiColor($"{CSI}48;5;45m");
            public static AnsiColor Green1 => new AnsiColor($"{CSI}48;5;46m");
            public static AnsiColor SpringGreen1 => new AnsiColor($"{CSI}48;5;48m");
            public static AnsiColor MediumSpringGreen => new AnsiColor($"{CSI}48;5;49m");
            public static AnsiColor Cyan2 => new AnsiColor($"{CSI}48;5;50m");
            public static AnsiColor Cyan1 => new AnsiColor($"{CSI}48;5;51m");
            public static AnsiColor DarkRed => new AnsiColor($"{CSI}48;5;52m");
            public static AnsiColor DeepPink4 => new AnsiColor($"{CSI}48;5;53m");
            public static AnsiColor Purple4 => new AnsiColor($"{CSI}48;5;54m");
            public static AnsiColor Purple3 => new AnsiColor($"{CSI}48;5;56m");
            public static AnsiColor BlueViolet => new AnsiColor($"{CSI}48;5;57m");
            public static AnsiColor Orange4 => new AnsiColor($"{CSI}48;5;58m");
            public static AnsiColor Grey37 => new AnsiColor($"{CSI}48;5;59m");
            public static AnsiColor MediumPurple4 => new AnsiColor($"{CSI}48;5;60m");
            public static AnsiColor SlateBlue3 => new AnsiColor($"{CSI}48;5;61m");
            public static AnsiColor RoyalBlue1 => new AnsiColor($"{CSI}48;5;63m");
            public static AnsiColor Chartreuse4 => new AnsiColor($"{CSI}48;5;64m");
            public static AnsiColor DarkSeaGreen4 => new AnsiColor($"{CSI}48;5;65m");
            public static AnsiColor PaleTurquoise4 => new AnsiColor($"{CSI}48;5;66m");
            public static AnsiColor SteelBlue => new AnsiColor($"{CSI}48;5;67m");
            public static AnsiColor SteelBlue3 => new AnsiColor($"{CSI}48;5;68m");
            public static AnsiColor CornflowerBlue => new AnsiColor($"{CSI}48;5;69m");
            public static AnsiColor Chartreuse3 => new AnsiColor($"{CSI}48;5;70m");
            public static AnsiColor CadetBlue => new AnsiColor($"{CSI}48;5;72m");
            public static AnsiColor SkyBlue3 => new AnsiColor($"{CSI}48;5;74m");
            public static AnsiColor SteelBlue1 => new AnsiColor($"{CSI}48;5;75m"); public static AnsiColor PaleGreen3 => new AnsiColor($"{CSI}48;5;77m");
            public static AnsiColor SeaGreen3 => new AnsiColor($"{CSI}48;5;78m");
            public static AnsiColor Aquamarine3 => new AnsiColor($"{CSI}48;5;79m");
            public static AnsiColor MediumTurquoise => new AnsiColor($"{CSI}48;5;80m");
            public static AnsiColor Chartreuse2 => new AnsiColor($"{CSI}48;5;82m");
            public static AnsiColor SeaGreen2 => new AnsiColor($"{CSI}48;5;83m");
            public static AnsiColor SeaGreen1 => new AnsiColor($"{CSI}48;5;84m");
            public static AnsiColor Aquamarine1 => new AnsiColor($"{CSI}48;5;86m");
            public static AnsiColor DarkSlateGray2 => new AnsiColor($"{CSI}48;5;87m");
            public static AnsiColor DarkMagenta => new AnsiColor($"{CSI}48;5;90m");
            public static AnsiColor DarkViolet => new AnsiColor($"{CSI}48;5;92m");
            public static AnsiColor LightPink4 => new AnsiColor($"{CSI}48;5;95m");
            public static AnsiColor Plum4 => new AnsiColor($"{CSI}48;5;96m");
            public static AnsiColor MediumPurple3 => new AnsiColor($"{CSI}48;5;97m");
            public static AnsiColor SlateBlue1 => new AnsiColor($"{CSI}48;5;99m");
            public static AnsiColor Yellow4 => new AnsiColor($"{CSI}48;5;100m");
            public static AnsiColor Wheat4 => new AnsiColor($"{CSI}48;5;101m");
            public static AnsiColor Grey53 => new AnsiColor($"{CSI}48;5;102m");
            public static AnsiColor LightSlateGrey => new AnsiColor($"{CSI}48;5;103m");
            public static AnsiColor MediumPurple => new AnsiColor($"{CSI}48;5;104m");
            public static AnsiColor LightSlateBlue => new AnsiColor($"{CSI}48;5;105m");
            public static AnsiColor DarkOliveGreen3 => new AnsiColor($"{CSI}48;5;107m");
            public static AnsiColor DarkSeaGreen => new AnsiColor($"{CSI}48;5;108m");
            public static AnsiColor LightSkyBlue3 => new AnsiColor($"{CSI}48;5;109m");
            public static AnsiColor SkyBlue2 => new AnsiColor($"{CSI}48;5;111m");
            public static AnsiColor DarkSeaGreen3 => new AnsiColor($"{CSI}48;5;115m");
            public static AnsiColor DarkSlateGray3 => new AnsiColor($"{CSI}48;5;116m");
            public static AnsiColor SkyBlue1 => new AnsiColor($"{CSI}48;5;117m");
            public static AnsiColor Chartreuse1 => new AnsiColor($"{CSI}48;5;118m");
            public static AnsiColor LightGreen => new AnsiColor($"{CSI}48;5;119m");
            public static AnsiColor PaleGreen1 => new AnsiColor($"{CSI}48;5;121m");
            public static AnsiColor DarkSlateGray1 => new AnsiColor($"{CSI}48;5;123m");
            public static AnsiColor Red3 => new AnsiColor($"{CSI}48;5;124m");
            public static AnsiColor MediumVioletRed => new AnsiColor($"{CSI}48;5;126m");
            public static AnsiColor Magenta3 => new AnsiColor($"{CSI}48;5;127m");
            public static AnsiColor DarkOrange3 => new AnsiColor($"{CSI}48;5;130m");
            public static AnsiColor IndianRed => new AnsiColor($"{CSI}48;5;131m");
            public static AnsiColor HotPink3 => new AnsiColor($"{CSI}48;5;132m");
            public static AnsiColor MediumOrchid3 => new AnsiColor($"{CSI}48;5;133m");
            public static AnsiColor MediumOrchid => new AnsiColor($"{CSI}48;5;134m");
            public static AnsiColor MediumPurple2 => new AnsiColor($"{CSI}48;5;135m");
            public static AnsiColor DarkGoldenrod => new AnsiColor($"{CSI}48;5;136m");
            public static AnsiColor LightSalmon3 => new AnsiColor($"{CSI}48;5;137m");
            public static AnsiColor RosyBrown => new AnsiColor($"{CSI}48;5;138m");
            public static AnsiColor Grey63 => new AnsiColor($"{CSI}48;5;139m");
            public static AnsiColor MediumPurple1 => new AnsiColor($"{CSI}48;5;141m");
            public static AnsiColor Gold3 => new AnsiColor($"{CSI}48;5;142m");
            public static AnsiColor DarkKhaki => new AnsiColor($"{CSI}48;5;143m");
            public static AnsiColor NavajoWhite3 => new AnsiColor($"{CSI}48;5;144m");
            public static AnsiColor Grey69 => new AnsiColor($"{CSI}48;5;145m");
            public static AnsiColor LightSteelBlue3 => new AnsiColor($"{CSI}48;5;146m");
            public static AnsiColor LightSteelBlue => new AnsiColor($"{CSI}48;5;147m");
            public static AnsiColor Yellow3 => new AnsiColor($"{CSI}48;5;148m");
            public static AnsiColor DarkSeaGreen2 => new AnsiColor($"{CSI}48;5;151m");
            public static AnsiColor LightCyan3 => new AnsiColor($"{CSI}48;5;152m");
            public static AnsiColor LightSkyBlue1 => new AnsiColor($"{CSI}48;5;153m");
            public static AnsiColor GreenYellow => new AnsiColor($"{CSI}48;5;154m");
            public static AnsiColor DarkOliveGreen2 => new AnsiColor($"{CSI}48;5;155m");
            public static AnsiColor DarkSeaGreen1 => new AnsiColor($"{CSI}48;5;158m");
            public static AnsiColor PaleTurquoise1 => new AnsiColor($"{CSI}48;5;159m");
            public static AnsiColor DeepPink3 => new AnsiColor($"{CSI}48;5;161m");
            public static AnsiColor Magenta2 => new AnsiColor($"{CSI}48;5;165m");
            public static AnsiColor HotPink2 => new AnsiColor($"{CSI}48;5;169m");
            public static AnsiColor Orchid => new AnsiColor($"{CSI}48;5;170m");
            public static AnsiColor MediumOrchid1 => new AnsiColor($"{CSI}48;5;171m");
            public static AnsiColor Orange3 => new AnsiColor($"{CSI}48;5;172m");
            public static AnsiColor LightPink3 => new AnsiColor($"{CSI}48;5;174m");
            public static AnsiColor Pink3 => new AnsiColor($"{CSI}48;5;175m");
            public static AnsiColor Plum3 => new AnsiColor($"{CSI}48;5;176m");
            public static AnsiColor Violet => new AnsiColor($"{CSI}48;5;177m");
            public static AnsiColor LightGoldenrod3 => new AnsiColor($"{CSI}48;5;179m");
            public static AnsiColor Tan => new AnsiColor($"{CSI}48;5;180m");
            public static AnsiColor MistyRose3 => new AnsiColor($"{CSI}48;5;181m");
            public static AnsiColor Thistle3 => new AnsiColor($"{CSI}48;5;182m");
            public static AnsiColor Plum2 => new AnsiColor($"{CSI}48;5;183m");
            public static AnsiColor Khaki3 => new AnsiColor($"{CSI}48;5;185m");
            public static AnsiColor LightGoldenrod2 => new AnsiColor($"{CSI}48;5;186m");
            public static AnsiColor LightYellow3 => new AnsiColor($"{CSI}48;5;187m");
            public static AnsiColor Grey84 => new AnsiColor($"{CSI}48;5;188m");
            public static AnsiColor LightSteelBlue1 => new AnsiColor($"{CSI}48;5;189m");
            public static AnsiColor Yellow2 => new AnsiColor($"{CSI}48;5;190m");
            public static AnsiColor DarkOliveGreen1 => new AnsiColor($"{CSI}48;5;191m");
            public static AnsiColor Honeydew2 => new AnsiColor($"{CSI}48;5;194m");
            public static AnsiColor LightCyan1 => new AnsiColor($"{CSI}48;5;195m");
            public static AnsiColor Red1 => new AnsiColor($"{CSI}48;5;196m");
            public static AnsiColor DeepPink2 => new AnsiColor($"{CSI}48;5;197m");
            public static AnsiColor DeepPink1 => new AnsiColor($"{CSI}48;5;198m");
            public static AnsiColor Magenta1 => new AnsiColor($"{CSI}48;5;201m");
            public static AnsiColor OrangeRed1 => new AnsiColor($"{CSI}48;5;202m");
            public static AnsiColor IndianRed1 => new AnsiColor($"{CSI}48;5;203m");
            public static AnsiColor HotPink => new AnsiColor($"{CSI}48;5;205m");
            public static AnsiColor DarkOrange => new AnsiColor($"{CSI}48;5;208m");
            public static AnsiColor Salmon1 => new AnsiColor($"{CSI}48;5;209m");
            public static AnsiColor LightCoral => new AnsiColor($"{CSI}48;5;210m");
            public static AnsiColor PaleVioletRed1 => new AnsiColor($"{CSI}48;5;211m");
            public static AnsiColor Orchid2 => new AnsiColor($"{CSI}48;5;212m");
            public static AnsiColor Orchid1 => new AnsiColor($"{CSI}48;5;213m");
            public static AnsiColor Orange1 => new AnsiColor($"{CSI}48;5;214m");
            public static AnsiColor SandyBrown => new AnsiColor($"{CSI}48;5;215m");
            public static AnsiColor LightSalmon1 => new AnsiColor($"{CSI}48;5;216m");
            public static AnsiColor LightPink1 => new AnsiColor($"{CSI}48;5;217m");
            public static AnsiColor Pink1 => new AnsiColor($"{CSI}48;5;218m");
            public static AnsiColor Plum1 => new AnsiColor($"{CSI}48;5;219m");
            public static AnsiColor Gold1 => new AnsiColor($"{CSI}48;5;220m");
            public static AnsiColor NavajoWhite1 => new AnsiColor($"{CSI}48;5;223m");
            public static AnsiColor MistyRose1 => new AnsiColor($"{CSI}48;5;224m");
            public static AnsiColor Thistle1 => new AnsiColor($"{CSI}48;5;225m");
            public static AnsiColor Yellow1 => new AnsiColor($"{CSI}48;5;226m");
            public static AnsiColor LightGoldenrod1 => new AnsiColor($"{CSI}48;5;227m");
            public static AnsiColor Khaki1 => new AnsiColor($"{CSI}48;5;228m");
            public static AnsiColor Wheat1 => new AnsiColor($"{CSI}48;5;229m");
            public static AnsiColor Cornsilk1 => new AnsiColor($"{CSI}48;5;230m");
            public static AnsiColor Grey100 => new AnsiColor($"{CSI}48;5;231m");
            public static AnsiColor Grey3 => new AnsiColor($"{CSI}48;5;232m");
            public static AnsiColor Grey7 => new AnsiColor($"{CSI}48;5;233m");
            public static AnsiColor Grey11 => new AnsiColor($"{CSI}48;5;234m");
            public static AnsiColor Grey15 => new AnsiColor($"{CSI}48;5;235m");
            public static AnsiColor Grey19 => new AnsiColor($"{CSI}48;5;236m");
            public static AnsiColor Grey23 => new AnsiColor($"{CSI}48;5;237m");
            public static AnsiColor Grey27 => new AnsiColor($"{CSI}48;5;238m");
            public static AnsiColor Grey30 => new AnsiColor($"{CSI}48;5;239m");
            public static AnsiColor Grey35 => new AnsiColor($"{CSI}48;5;240m");
            public static AnsiColor Grey39 => new AnsiColor($"{CSI}48;5;241m");
            public static AnsiColor Grey42 => new AnsiColor($"{CSI}48;5;242m");
            public static AnsiColor Grey46 => new AnsiColor($"{CSI}48;5;243m");
            public static AnsiColor Grey50 => new AnsiColor($"{CSI}48;5;244m");
            public static AnsiColor Grey54 => new AnsiColor($"{CSI}48;5;245m");
            public static AnsiColor Grey58 => new AnsiColor($"{CSI}48;5;246m");
            public static AnsiColor Grey62 => new AnsiColor($"{CSI}48;5;247m");
            public static AnsiColor Grey66 => new AnsiColor($"{CSI}48;5;248m");
            public static AnsiColor Grey70 => new AnsiColor($"{CSI}48;5;249m");
            public static AnsiColor Grey74 => new AnsiColor($"{CSI}48;5;250m");
            public static AnsiColor Grey78 => new AnsiColor($"{CSI}48;5;251m");
            public static AnsiColor Grey82 => new AnsiColor($"{CSI}48;5;252m");
            public static AnsiColor Grey85 => new AnsiColor($"{CSI}48;5;253m");
            public static AnsiColor Grey89 => new AnsiColor($"{CSI}48;5;254m");
            public static AnsiColor Grey93 => new AnsiColor($"{CSI}48;5;255m");
        }
        /// <summary>
        /// Reset certain terminal settings to their defaults.
        /// </summary>
        public static AnsiColor SoftReset = new AnsiColor($"{CSI}!p");
    }
}
