using System.Collections.Generic;
using UnityEngine;

public class KeyCodeDictionary
{
    public static Dictionary<KeyCode, string> keys  = new Dictionary<KeyCode, string> ()
    {
        //Alphabet
        {KeyCode.A, "A"},
        {KeyCode.B, "B"},
        {KeyCode.C, "C"},
        {KeyCode.D, "D"},
        {KeyCode.E, "E"},
        {KeyCode.F, "F"},
        {KeyCode.G, "G"},
        {KeyCode.H, "H"},
        {KeyCode.I, "I"},
        {KeyCode.J, "J"},
        {KeyCode.K, "K"},
        {KeyCode.L, "L"},
        {KeyCode.M, "M"},
        {KeyCode.N, "N"},
        {KeyCode.O, "O"},
        {KeyCode.P, "P"},
        {KeyCode.Q, "Q"},
        {KeyCode.R, "R"},
        {KeyCode.S, "S"},
        {KeyCode.T, "T"},
        {KeyCode.U, "U"},
        {KeyCode.V, "V"},
        {KeyCode.W, "W"},
        {KeyCode.X, "X"},
        {KeyCode.Y, "Y"},
        {KeyCode.Z, "Z"},

        //Numbers
        {KeyCode.Alpha1, "1"},
        {KeyCode.Alpha2, "2"},
        {KeyCode.Alpha3, "3"},
        {KeyCode.Alpha4, "4"},
        {KeyCode.Alpha5, "5"},
        {KeyCode.Alpha6, "6"},
        {KeyCode.Alpha7, "7"},
        {KeyCode.Alpha8, "8"},
        {KeyCode.Alpha9, "9"},
        {KeyCode.Alpha0, "0"},

        //Function keys
        {KeyCode.F1, "F1"},
        {KeyCode.F2, "F2"},
        {KeyCode.F3, "F3"},
        {KeyCode.F4, "F4"},
        {KeyCode.F5, "F5"},
        {KeyCode.F6, "F6"},
        {KeyCode.F7, "F7"},
        {KeyCode.F8, "F8"},
        {KeyCode.F9, "F9"},
        {KeyCode.F10, "F10"},
        {KeyCode.F11, "F11"},
        {KeyCode.F12, "F12"},
        {KeyCode.F13, "F13"},
        {KeyCode.F14, "F14"},
        {KeyCode.F15, "F15"},

        //KeyPad Numbers
        {KeyCode.Keypad1, "N1"},
        {KeyCode.Keypad2, "N2"},
        {KeyCode.Keypad3, "N3"},
        {KeyCode.Keypad4, "N4"},
        {KeyCode.Keypad5, "N5"},
        {KeyCode.Keypad6, "N6"},
        {KeyCode.Keypad7, "N7"},
        {KeyCode.Keypad8, "N8"},
        {KeyCode.Keypad9, "N9"},
        {KeyCode.Keypad0, "N0"},
        {KeyCode.KeypadPeriod, "N."},
        {KeyCode.KeypadDivide, "N/"},
        {KeyCode.KeypadMultiply, "N*"},
        {KeyCode.KeypadMinus, "N-"},
        {KeyCode.KeypadPlus, "N+"},
        {KeyCode.KeypadEquals, "N="},

        //Shift, ctrl, tab, etc
        {KeyCode.LeftShift, "Shift"},
        {KeyCode.RightShift, "R-shift"},
        {KeyCode.LeftControl, "Ctrl"},
        {KeyCode.RightControl, "R-ctrl"},
        {KeyCode.LeftAlt, "Alt"},
        {KeyCode.RightAlt, "R-alt"},
        {KeyCode.LeftWindows, "Win"},
        {KeyCode.RightWindows, "R-win"},
        {KeyCode.Tab, "Tab"},
        {KeyCode.CapsLock, "Caps"},
        {KeyCode.Space, "Space"},

        //Symbols
        {KeyCode.Exclaim, "!"}, //1
        {KeyCode.DoubleQuote, "\""},
        {KeyCode.Hash, "#"}, //3
        {KeyCode.Dollar, "$"}, //4
        {KeyCode.Ampersand, "&"}, //7
        {KeyCode.Quote, "\'"}, //remember the special forward slash rule... this isnt wrong
        {KeyCode.LeftParen, "("}, //9
        {KeyCode.RightParen, ")"}, //0
        {KeyCode.Asterisk, "*"}, //8
        {KeyCode.Plus, "+"},
        {KeyCode.Comma, ","},
        {KeyCode.Minus, "-"},
        {KeyCode.Period, "."},
        {KeyCode.Slash, "/"},
        {KeyCode.Colon, ":"},
        {KeyCode.Semicolon, ";"},
        {KeyCode.Less, "<"},
        {KeyCode.Equals, "="},
        {KeyCode.Greater, ">"},
        {KeyCode.Question, "?"},
        {KeyCode.At, "@"}, //2
        {KeyCode.LeftBracket, "["},
        {KeyCode.Backslash, "\\"}, //remember the special forward slash rule... this isnt wrong
        {KeyCode.RightBracket, "]"},
        {KeyCode.Caret, "^"}, //6
        {KeyCode.Underscore, "_"},
        {KeyCode.BackQuote, "`"},
        {KeyCode.Tilde, "~"},
        {KeyCode.LeftCurlyBracket, "{"}, 
        {KeyCode.RightCurlyBracket, "}"}, 
        {KeyCode.Pipe, "|"},   
        {KeyCode.Percent, "%"},

        //Mouse Buttons
        {KeyCode.Mouse0, "LMB"},
        {KeyCode.Mouse1, "RMB"},
        {KeyCode.Mouse2, "MMB"},

        //KeyCode none
        {KeyCode.None, ""},
    };

}
