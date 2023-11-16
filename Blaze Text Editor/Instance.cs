namespace Blaze_Text_Editor
{
    internal class Instance
    {
        private string? FilePath = null;
        private string? Mode;
        private int CursorX = 0;
        private int CursorY = 0;
        private int ScrollIndex = 0;
        private int TabSpaces = 4;

        private readonly int InfobarPosition = Console.WindowHeight - 1;
        private readonly string BlankLine = new(' ', Console.WindowWidth - 1);
        private readonly List<string> Lines = new();
        private readonly Dictionary<ConsoleKey, Action> ViewModeInput;
        private readonly Dictionary<ConsoleKey, Action> InsertModeInput;

        internal Instance(string? filePath)
        {
            ViewModeInput = new Dictionary<ConsoleKey, Action>()
            {
                { ConsoleKey.UpArrow, MoveCursorUp },
                { ConsoleKey.LeftArrow, MoveCursorLeft },
                { ConsoleKey.DownArrow, MoveCursorDown },
                { ConsoleKey.RightArrow, MoveCursorRight },
                { ConsoleKey.OemPlus, ()=> TabSpaces++ },
                { ConsoleKey.OemMinus, ()=> TabSpaces -= TabSpaces > 1 ? 1 : 0 },
                { ConsoleKey.I, InsertAtCurrentLine },
                { ConsoleKey.R, RemoveCurrentLine },
                { ConsoleKey.O, OpenFile },
                { ConsoleKey.C, CloseFile },
                { ConsoleKey.S, SaveFile },
                { ConsoleKey.Escape, ()=>
                    {
                        Console.Clear();
                        Environment.Exit(0);
                    }
                }
            };

            InsertModeInput = new Dictionary<ConsoleKey, Action>()
            {
                { ConsoleKey.Backspace, InsertModeBackspace },
                { ConsoleKey.Tab, InsertModeTab },
                { ConsoleKey.Enter, CreateNewLine },
                { ConsoleKey.LeftArrow, ()=> CursorX = 0 },
                { ConsoleKey.RightArrow, ()=> CursorX = Lines[CursorY + ScrollIndex].Length },
                { ConsoleKey.DownArrow, ()=> CursorX = Lines[CursorY + ScrollIndex].Length / 2 }
            };

            if (!File.Exists(filePath))
            {
                Lines.Add("");
                Mode = "VIEW";
                return;
            }

            Lines.AddRange(File.ReadAllLines(filePath));
            FilePath = filePath;

            CheckIfReadOnly();
        }

        internal void Start()
        {
            RefreshScreen();
            DrawInfoBar();

            while (true)
            {
                if (Lines.Count <= 0)
                {
                    Lines.Add("");
                }
                DrawInfoBar();
                Console.SetCursorPosition(CursorX, CursorY);

                ConsoleKey key = Console.ReadKey(true).Key;
                if (ViewModeInput.ContainsKey(key) && Mode != "READONLY")
                {
                    ViewModeInput[key].Invoke();
                    continue;
                }
                if (key == ConsoleKey.C || key == ConsoleKey.Escape)
                {
                    ViewModeInput[key].Invoke();
                }
            }
        }

        private void ResetPositionAndLines(params string[] lines)
        {
            CursorX = 0;
            CursorY = 0;
            ScrollIndex = 0;
            TabSpaces = 4;
            Lines.Clear();
            Lines.AddRange(lines);
        }

        private void CheckIfReadOnly()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return;
            }

            FileInfo info = new(FilePath);
            if (info.IsReadOnly)
            {
                Mode = "READONLY";
                return;
            }
            Mode = "VIEW";
        }

        #region Screen Drawing

        private void RefreshScreen()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            for (int i = 0; i < InfobarPosition; i++)
            {
                Utilities.WriteAtPosition(0, i, BlankLine);

                if (i < Lines.Count)
                {
                    Utilities.WriteAtPosition(0, i, $"{Lines[i + ScrollIndex]}");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Utilities.WriteAtPosition(0, i, "~");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void DrawInfoBar()
        {
            string? file = !string.IsNullOrEmpty(FilePath) ? FilePath.Split('\\')[^1] : null;
            string info = $"[Position: {CursorX}, {CursorY + ScrollIndex}]   [Tab: {TabSpaces}]   [Mode: {Mode}]   [File: {file}]";

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Utilities.WriteAtPosition(0, InfobarPosition, BlankLine);
            Utilities.WriteAtPosition(0, InfobarPosition, info);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        #endregion

        #region Cursor Movement

        private void MoveCursorLeft()
        {
            if (CursorX > 0)
            {
                CursorX--;
            }
        }

        private void MoveCursorRight()
        {
            if (CursorX < Lines[CursorY + ScrollIndex].Length)
            {
                CursorX++;
            }
        }

        private void MoveCursorUp()
        {
            if (CursorY > 0)
            {
                CursorY--;
            }
            else if (CursorY == 0 && ScrollIndex > 0)
            {
                ScrollScreen(-1);
            }
            ResetCursor();
        }

        private void MoveCursorDown()
        {
            if (CursorY < Lines.Count - 1 && CursorY + 1 < InfobarPosition)
            {
                CursorY++;
            }
            else if (CursorY + 1 == InfobarPosition && CursorY + ScrollIndex < Lines.Count - 1)
            {
                ScrollScreen(1);
            }
            ResetCursor();
        }

        private void ResetCursor()
        {
            if (string.IsNullOrEmpty(Lines[CursorY + ScrollIndex]))
            {
                CursorX = 0;
                return;
            }
            if (CursorX >= Lines[CursorY + ScrollIndex].Length)
            {
                CursorX = Lines[CursorY + ScrollIndex].Length;
            }
        }

        #endregion

        #region Line Editing

        private void ScrollScreen(int amount)
        {
            ScrollIndex += amount;
            RefreshScreen();
            DrawInfoBar();
        }

        private void CreateNewLine()
        {
            string oldLine = Lines[CursorY + ScrollIndex].Remove(CursorX);
            string newLine = Lines[CursorY + ScrollIndex][CursorX..];

            Lines.Insert(CursorY + ScrollIndex, oldLine);

            if (CursorY + 1 == InfobarPosition)
            {
                ScrollScreen(1);
            }
            else
            {
                CursorY++;
            }

            Lines[CursorY + ScrollIndex] = newLine;
            CursorX = 0;
            RefreshScreen();
        }

        private void InsertModeBackspace()
        {
            if (Lines[CursorY + ScrollIndex] != "")
            {
                Lines[CursorY + ScrollIndex] = Lines[CursorY + ScrollIndex].Remove(CursorX - 1, 1);
                MoveCursorLeft();
            }
        }

        private void InsertModeTab()
        {
            if (CursorX + TabSpaces < Console.WindowWidth - 2)
            {
                Lines[CursorY + ScrollIndex] = Lines[CursorY + ScrollIndex].Insert(CursorX, new string(' ', TabSpaces));
                CursorX += TabSpaces;
            }
        }

        private void UpdateCurrentLine()
        {
            if (CursorY + ScrollIndex < Lines.Count)
            {
                Utilities.WriteAtPosition(0, CursorY, BlankLine);
                Utilities.WriteAtPosition(0, CursorY, Lines[CursorY + ScrollIndex]);
                return;
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Utilities.WriteAtPosition(0, CursorY + ScrollIndex, "~");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void InsertAtCurrentLine()
        {
            Mode = "INSERT";
            ConsoleKeyInfo input;
            do
            {
                UpdateCurrentLine();
                DrawInfoBar();
                Console.SetCursorPosition(CursorX, CursorY);
                input = Console.ReadKey();

                if (InsertModeInput.ContainsKey(input.Key))
                {
                    InsertModeInput[input.Key]();
                    continue;
                }
                if (char.IsAscii(input.KeyChar) && !char.IsControl(input.KeyChar) && CursorX < Console.WindowWidth - 2)
                {
                    if (string.IsNullOrEmpty(Lines[CursorY + ScrollIndex]))
                    {
                        Lines[CursorY + ScrollIndex] = input.KeyChar.ToString();
                        CursorX++;
                        continue;
                    }
                    Lines[CursorY + ScrollIndex] = Lines[CursorY + ScrollIndex].Insert(CursorX, $"{input.KeyChar}");
                    CursorX++;
                }
            }
            while (input.Key != ConsoleKey.Home);
            ResetCursor();
            Mode = "VIEW";
        }

        private void RemoveCurrentLine()
        {
            if (CursorY > 0)
            {
                Lines.RemoveAt(CursorY + ScrollIndex);
                CursorY--;
            }
            RefreshScreen();
        }

        #endregion

        #region File Control

        private void OpenFile()
        {
            string? path;

            Console.BackgroundColor = ConsoleColor.DarkRed;
            do
            {
                Utilities.WriteAtPosition(0, 0, BlankLine);

                Console.SetCursorPosition(0, 0);
                path = Utilities.PromptUser("Enter a file path: ");
            }
            while (!File.Exists(path));

            FilePath = path;
            ResetPositionAndLines(File.ReadAllLines(path));
            if (Lines.Count <= 0)
            {
                Lines.Add("");
            }

            CheckIfReadOnly();
            RefreshScreen();
        }

        private void SaveFile()
        {
            string? path;
            string? name;

            if (File.Exists(FilePath))
            {
                File.WriteAllLines(FilePath, Lines);
                RefreshScreen();
                return;
            }

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Utilities.WriteAtPosition(0, 0, $"{BlankLine}\n{BlankLine}");

            Console.SetCursorPosition(0, 0);
            path = Utilities.PromptUser("Enter a path: ");
            name = Utilities.PromptUser("Enter a name: ");

            if (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(name))
            {
                FilePath = @$"{path}\{name}";
                _ = Directory.CreateDirectory(path);
                File.WriteAllLines(FilePath, Lines);
            }

            RefreshScreen();
        }

        private void CloseFile()
        {
            Mode = "VIEW";
            FilePath = null;
            ResetPositionAndLines("");
            DrawInfoBar();
            RefreshScreen();
        }

        #endregion
    }
}