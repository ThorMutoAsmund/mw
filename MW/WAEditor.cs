using MW.Helpers;
using MW.Parsing;
using System.Globalization;
using System.Text;

namespace MW
{
    public static class WAEditor
    {
        public static event Action<List<string>>? Recalc;

        public static bool ChangesMade = false;
        public static List<string> Lines { get; } = new() { "" };
        
        // Colors
        static ConsoleColor? origForegroundColor = null;
        static ConsoleColor? origBackgroundColor = null;
        static int margin = 5;

        // Cursor in buffer (column, row) — NOT screen coordinates
        static int cx = 0;
        static int cy = 0;

        // Viewport top-left in buffer coordinates
        static int top = 0;
        static int left = 0;

        // Debug
        static string debugMessage = string.Empty;

        public static void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.TreatControlCAsInput = true;
            Console.CursorVisible = true;

            SetColor();
            Console.Clear();

            // Repeat until quit / Ctrl+Q
            var startInCommandMode = !Env.IsProjectLoaded;

            for (; ; )
            {
                // Enter command mode from the start?
                if (startInCommandMode)
                {
                    Render(commandMode: true);

                    if (RunCommandMode())
                    {
                        break;
                    }

                    SetColor();
                    //continue;
                    startInCommandMode = false;
                }

                Render();
                var key = Console.ReadKey(intercept: true);

                if ((key.Modifiers & ConsoleModifiers.Control) != 0 && (key.Modifiers & ConsoleModifiers.Alt) == 0)
                {
                    bool stop = false;
                    switch (key.Key)
                    {
                        case ConsoleKey.Q:
                            {
                                if (Show.OkToDiscardChanges())
                                {
                                    Console.SetCursorPosition(0, Math.Min(Console.WindowHeight - 1, Console.WindowHeight - 1));
                                    stop = true;
                                    break;
                                }

                                SetColor();
                                Render(commandMode: true);
                                continue;
                            }
                        case ConsoleKey.Spacebar:
                            {
                                var isPlaying = Playback.TogglePlaySong();
                                ShowInfo(isPlaying ? "Playing..." : "Stopped");

                                continue;
                            }
                        case ConsoleKey.D1:
                        case ConsoleKey.D2:
                        case ConsoleKey.D3:
                        case ConsoleKey.D4:
                        case ConsoleKey.D5:
                        case ConsoleKey.D6:
                        case ConsoleKey.D7:
                        case ConsoleKey.D8:
                        case ConsoleKey.D9:
                        case ConsoleKey.D0:
                            {
                                double? jumpedTo = null;
                                switch (key.Key)
                                {
                                    case ConsoleKey.D1: jumpedTo = Playback.JumpTo(0); break;
                                    case ConsoleKey.D2: jumpedTo = Playback.JumpTo(1); break;
                                    case ConsoleKey.D3: jumpedTo = Playback.JumpTo(2); break;
                                    case ConsoleKey.D4: jumpedTo = Playback.JumpTo(3); break;
                                    case ConsoleKey.D5: jumpedTo = Playback.JumpTo(4); break;
                                    case ConsoleKey.D6: jumpedTo = Playback.JumpTo(5); break;
                                    case ConsoleKey.D7: jumpedTo = Playback.JumpTo(6); break;
                                    case ConsoleKey.D8: jumpedTo = Playback.JumpTo(7); break;
                                    case ConsoleKey.D9: jumpedTo = Playback.JumpTo(8); break;
                                    case ConsoleKey.D0: jumpedTo = Playback.JumpTo(9); break;
                                    default:
                                        continue;
                                }

                                if (jumpedTo.HasValue)
                                {
                                    ShowInfo($"Jumping to {jumpedTo.Value.FromSeconds()}s");
                                }

                                continue;
                            }
                        case ConsoleKey.RightArrow:
                            {
                                var useSmall = (key.Modifiers & ConsoleModifiers.Shift) != 0;
                                var jumpedTo = useSmall ? Playback.SeekFine(1D) : Playback.Seek(1D);
                                if (jumpedTo.HasValue)
                                {
                                    ShowInfo($"Jumping to {jumpedTo.Value.FromSeconds()}s");
                                }
                                continue;
                            }
                        case ConsoleKey.LeftArrow:
                            {
                                var useSmall = (key.Modifiers & ConsoleModifiers.Shift) != 0;
                                var jumpedTo = useSmall ? Playback.SeekFine(-1D) : Playback.Seek(-1D);
                                if (jumpedTo.HasValue)
                                {
                                    ShowInfo($"Jumping to {jumpedTo.Value.FromSeconds()}s");
                                }
                                continue;
                            }
                    }
                    if (stop)
                    {
                        break;
                    }
                }


                if (key.Key == ConsoleKey.Escape)
                {
                    Render(commandMode: true);

                    if (RunCommandMode())
                    {
                        break;
                    }

                    SetColor();
                    continue;
                }

                HandleKey(key);
                ClampCursor();
                EnsureCursorVisible();            
            }

            // Leave the screen in a tidy state
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.WriteLine();
        }

        private static void RecalcIfChangesMade(List<string> lines)
        {
            if (ChangesMade)
            {
                Recalc?.Invoke(lines);
                ChangesMade = false;
            }
        }

        public static void ShowInfo(object? message = null)
        {
            debugMessage = message?.ToString() ?? string.Empty;
        }

        public static void LoadText(string[] text)
        {
            Lines.Clear();
            Lines.AddRange(text);
        }

        private static void HandleKey(ConsoleKeyInfo k)
        {
            switch (k.Key)
            {
                case ConsoleKey.LeftArrow: MoveLeft(); break;
                case ConsoleKey.RightArrow: MoveRight(); break;
                case ConsoleKey.UpArrow: MoveUp(); break;
                case ConsoleKey.DownArrow: MoveDown(); break;
                case ConsoleKey.Home: MoveHome(k); break;
                case ConsoleKey.End: MoveEnd(k); break;
                case ConsoleKey.PageUp: PageUp(); break;
                case ConsoleKey.PageDown: PageDown(); break;
                case ConsoleKey.Backspace: Backspace(); break;
                case ConsoleKey.Delete: Delete(); break;
                case ConsoleKey.Enter: InsertNewLine(); break;
                case ConsoleKey.Tab: InsertTab(); break;

                default:
                    // Printable characters
                    char ch = k.KeyChar;
                    if (!char.IsControl(ch))
                    {
                        InsertChar(ch);
                    }
                    break;
            }
        }

        private static void MoveLeft()
        {
            if (cx > 0)
            {
                cx--;
            }
            else if (cy > 0) 
            { 
                cy--; 
                cx = Lines[cy].Length;
                RecalcIfChangesMade(Lines);
            }
        }

        private static void MoveRight()
        {
            if (cx < Lines[cy].Length)
            {
                cx++;
            }
            else if (cy < Lines.Count - 1) 
            { 
                cy++; 
                cx = 0;
                RecalcIfChangesMade(Lines);
            }
        }

        private static void MoveUp()
        {
            if (cy > 0)
            { 
                cy--; 
                cx = Math.Min(cx, Lines[cy].Length);
                RecalcIfChangesMade(Lines);
            }
        }

        private static void MoveDown()
        {
            if (cy < Lines.Count - 1) 
            { 
                cy++; 
                cx = Math.Min(cx, Lines[cy].Length);
                RecalcIfChangesMade(Lines);
            }
        }

        private static void MoveHome(ConsoleKeyInfo k)
        {
            if ((k.Modifiers & ConsoleModifiers.Control) != 0)
            {
                if (cy != 0)
                {
                    cy = 0;
                    RecalcIfChangesMade(Lines);
                }
            }

            cx = 0;
        }

        private static void MoveEnd(ConsoleKeyInfo k)
        {
            if ((k.Modifiers & ConsoleModifiers.Control) != 0) 
            { 
                if (cy != Lines.Count - 1)
                {
                    cy = Lines.Count - 1;
                    RecalcIfChangesMade(Lines);
                }
            }
            
            cx = Lines[cy].Length;
        }

        private static void PageUp()
        {
            var newCy = Math.Max(0, cy - Math.Max(1, Console.WindowHeight - 2));
            if (cy != newCy)
            {
                cy = newCy;
                RecalcIfChangesMade(Lines);
            }

            cx = Math.Min(cx, Lines[cy].Length);
        }

        private static void PageDown()
        {
            var newCy = Math.Min(Lines.Count - 1, cy + Math.Max(1, Console.WindowHeight - 2));
            if (cy != newCy)
            {
                cy = newCy;
                RecalcIfChangesMade(Lines);
            }

            cx = Math.Min(cx, Lines[cy].Length);
        }

        private static void Backspace()
        {
            if (cx > 0)
            {
                Lines[cy] = Lines[cy].Remove(cx - 1, 1);
                cx--;
                ChangesMade = true;
            }
            else if (cy > 0)
            {
                int prevLen = Lines[cy - 1].Length;
                Lines[cy - 1] = Lines[cy - 1] + Lines[cy];
                Lines.RemoveAt(cy);
                cy--; 
                cx = prevLen;
                ChangesMade = true;
                RecalcIfChangesMade(Lines);
            }
        }

        private static void Delete()
        {
            if (cx < Lines[cy].Length)
            {
                Lines[cy] = Lines[cy].Remove(cx, 1);
                ChangesMade = true;
            }
            else if (cy < Lines.Count - 1)
            {
                Lines[cy] = Lines[cy] + Lines[cy + 1];
                Lines.RemoveAt(cy + 1);
                ChangesMade = true;
            }
        }

        private static void InsertNewLine()
        {
            string line = Lines[cy];
            string left = line.Substring(0, cx);
            string right = line.Substring(cx);
            Lines[cy] = left;
            Lines.Insert(cy + 1, right);
            cy++; 
            cx = 0;
            ChangesMade = true;
            RecalcIfChangesMade(Lines);
        }

        private static void InsertTab()
        {
            InsertText("    "); // 4-space tabs
            ChangesMade = true;
        }

        private static void InsertChar(char c) => InsertText(c.ToString());

        private static void InsertText(string s)
        {
            Lines[cy] = Lines[cy].Insert(cx, s);
            cx += s.Length;
            ChangesMade = true;
        }

        private static void ClampCursor()
        {
            cy = Math.Max(0, Math.Min(cy, Lines.Count - 1));
            cx = Math.Max(0, Math.Min(cx, Lines[cy].Length));
        }

        private static void EnsureCursorVisible()
        {
            int scrW = Math.Max(1, Console.WindowWidth);
            int scrH = Math.Max(2, Console.WindowHeight); // keep 1 line for status

            // vertical
            if (cy < top) top = cy;
            if (cy >= top + scrH - 1) top = cy - (scrH - 2);
            if (top < 0) top = 0;

            // horizontal
            if (cx < left) left = cx;
            if (cx >= left + scrW) left = cx - (scrW - 1);
            if (left < 0) left = 0;
        }

        private static void Render(bool commandMode = false)
        {
            int width = Math.Max(1, Console.WindowWidth);
            int height = Math.Max(2, Console.WindowHeight); // last line = status
            int commandAreaHeight = 10;
            Console.CursorVisible = false;

            // Draw text area
            for (int row = 0; row < height - 1 - (commandMode ? commandAreaHeight : 0); row++)
            {
                int bufRow = top + row;
                Console.SetCursorPosition(0, row);

                if (bufRow >= 0 && bufRow < Lines.Count)
                {
                    var line = Lines[bufRow];
                    string slice = Slice(line, left, width);
                    WritePadded(slice, width, lineNo: bufRow, lineNodPadding: margin - 1);
                }
                else
                {
                    WritePadded("", width);
                }
            }

            // Draw status line
            Console.SetCursorPosition(0, height - 1 - (commandMode ? commandAreaHeight : 0));
            var status = $"{Env.ApplicationNameAndVersion} | Ln {cy + 1}, Col {cx + 1} | {debugMessage}";
            if (status.Length > width) status = status.Substring(0, width);
            WritePadded(status, width, invert: true);

            // Place cursor
            int cursorX = cx - left;
            int cursorY = cy - top;
            cursorX = Math.Max(0, Math.Min(cursorX, width - 1));
            cursorY = Math.Max(0, Math.Min(cursorY, height - 2));

            Console.SetCursorPosition(margin+cursorX, cursorY);
            
            if (commandMode)
            {
                for (int row = height -  commandAreaHeight; row < height; row++)
                {
                    ResetColor();

                    Console.SetCursorPosition(0, row);
                    for (int col = 0; col < width; col++)
                    {
                        Console.Write(" ");
                    }
                }

                Console.SetCursorPosition(0, height - commandAreaHeight);
            }

            Console.CursorVisible = true;
        }

        static string Slice(string s, int start, int width)
            => start >= s.Length ? "" :
               s.Length - start <= width ? s.Substring(start) :
               s.Substring(start, width);

        static void SetColor()
        {
            // Save colors
            if (origForegroundColor == null)
            {
                origForegroundColor = Console.ForegroundColor;
            }
            if (origBackgroundColor == null)
            {
                origBackgroundColor = Console.BackgroundColor;
            }

            // Background
            var bk_r = "20";
            var bk_g = "20";
            var bk_b = "20";

            // Foreground
            var fg_r = "46";
            var fg_g = "149";
            var fg_b = "211";

            Console.Write($"\u001b[38;2;{fg_r};{fg_g};{fg_b}m");
            Console.Write($"\u001b[48;2;{bk_r};{bk_g};{bk_b}m");
        }

        static void ResetColor()
        {
            Console.BackgroundColor = origBackgroundColor ?? ConsoleColor.Black;
            Console.ForegroundColor = origForegroundColor ?? ConsoleColor.White;
        }

        static void WritePadded(string content, int width, bool invert = false, int? lineNo = null, int lineNodPadding = 0)
        {
            if (content.Length < width)
            {
                content = content + new string(' ', width - content.Length);
            }

            if (invert)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(content);
                SetColor();
            }
            else
            {
                if (lineNo.HasValue)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(lineNo.Value.ToString().PadLeft(lineNodPadding) + " ");
                    SetColor();
                }
                Console.Write(content);
            }
        }

        static bool RunCommandMode()
        {
            List<string> memory = new();
            int memoryPos = -1;
            StringBuilder sb = new ();

            var prompt = "> ";

            int width = Math.Max(1, Console.WindowWidth);
            int bottom = Math.Max(0, Console.GetCursorPosition().Top);

            Console.Write(prompt);

            for (; ;)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape && Env.IsProjectLoaded)
                {
                    // Always stop before returning to editor
                    Playback.Stop();

                    return false;
                }
                else if (key.Key == ConsoleKey.Q && (key.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    return Show.OkToDiscardChanges();
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    var commandLine = sb.ToString();
                    Console.WriteLine();
                    if (!string.IsNullOrEmpty(commandLine))
                    {
                        if (memory.Count == 0 || memory[memory.Count - 1] != commandLine)
                        {
                            memory.Add(commandLine);
                        }

                        if (CommandParser.Parse(commandLine))
                        {
                            return true;
                        }
                    }
                    Console.Write(prompt);
                    bottom = Math.Max(0, Console.GetCursorPosition().Top);
                    sb.Clear();
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        int x = prompt.Length + sb.Length;
                        Console.SetCursorPosition(x, bottom);
                        Console.Write(' ');
                        Console.SetCursorPosition(x, bottom);
                    }
                    continue;
                }
                else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow)
                {
                    if (memoryPos == -1)
                    {
                        if (key.Key == ConsoleKey.UpArrow && memory.Count > 0)
                        {
                            memoryPos = memory.Count - 1;
                        }
                    }                    
                    else
                    {
                        if (key.Key == ConsoleKey.UpArrow && memoryPos > 0)
                        {
                            memoryPos -= 1;
                            if (memoryPos >= memory.Count)
                            {
                                memoryPos = memory.Count - 1;
                            }
                        }
                        else if (key.Key == ConsoleKey.DownArrow && memoryPos < memory.Count - 1)
                        {
                            memoryPos += 1;
                        }
                    }

                    if (memoryPos > -1 && memoryPos < memory.Count)
                    {
                        Console.CursorVisible = false;
                        Console.SetCursorPosition(0, bottom);
                        Console.Write(prompt);
                        for (int col = prompt.Length; col < width; col++)
                        {
                            Console.Write(" ");
                        }

                        var recalledText = memory[memoryPos];
                        sb.Clear();
                        sb.Append(recalledText);

                        Console.SetCursorPosition(prompt.Length, bottom);
                        Console.Write(recalledText);
                        Console.CursorVisible = true;
                    }
                }

                // accept printable chars
                if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }
    }
}
