# Blaze Text Editor
> A simple command-line text editor that I wrote for fun in C# for both Windows and Linux

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

# Modes

## VIEW
- Escape Key
  > Exit the program
- Arrow Keys
  > Navigate around the file
- Plus Key
  > Increase amount of spaces per tab
- Minus Key
  > Decrease amount of spaces per tab
- I Key
  > Enter insert mode
- R Key
  > Remove the line that the cursor is currently on
- O Key
  > Open a file
- C Key
  > Close the currently opened file
- S Key
  > Save the currently open file or create and save a new file

## INSERT
- Backspace Key
  > Removes a character at the cursor position
- Tab Key
  > Adds a set amount of spaces starting at the cursor position
- Enter Key
  > Creates a new line
- Left Arrow Key
  > Moves the cursor to the start of the line
- Right Arrow Key
  > Moves the cursor to the end of the line
- Down Arrow Key
  > Moves the cursor to the middle of the line
- Home Key
  > Exits insert mode

## READONLY
- Escape Key
  > Exits the program
- C Key
  > Closes the file
