using System;
using System.Reflection;
using System.Collections.Generic;

namespace PJ03
{
    /// <summary>
    /// A collection of static methods which return a DFA to recognize a given token
    /// </summary>
    public static class TokenDFAs
    {
        #region Helper methods
        /// <summary>
        /// An attribute marker meant to flag a method which returns a DFA to recognize a token, categorized
        /// by the given token name, and with a given level (to sort which token should be recognized if two
        /// tokens of the same length are both recognized)
        /// </summary>
        private class TokenDFAMarker : System.Attribute
        {
            public string Token;
            public int Level;
            public TokenDFAMarker(string token, int level)
            {
                Token = token;
                Level = level;
            }
        }

        public static (DFA, string)[] GetTokenDFAs()
        {
            List<(MethodInfo, TokenDFAMarker)> tokenDFAMethods = new List<(MethodInfo, TokenDFAMarker)>();

            foreach (MethodInfo method in typeof(TokenDFAs).GetRuntimeMethods())
            {
                TokenDFAMarker marker = method.GetCustomAttribute(typeof(TokenDFAMarker)) as TokenDFAMarker;
                if (marker != null)
                {
                    tokenDFAMethods.Add((method, marker));
                }
            }

            // Sort method list by the token level
            tokenDFAMethods.Sort((x, y) => x.Item2.Level - y.Item2.Level);

            // Create new list of DFAs and their tokens, by invoking each method
            List<(DFA, string)> dfaList = tokenDFAMethods.ConvertAll(x =>
            {
                return ((DFA)x.Item1.Invoke(null, null), x.Item2.Token);
            });

            // Return new list as array
            return dfaList.ToArray();
        }
        #endregion

        #region DFA Definitions
        /* Each token type has it's own level, by which the DFAs are sorted, so that the first
         * accepting dfa for a string is used (mostly to avoid keywords being categorized as identifiers)
         */
        #region Comment blocks - level -1
        [TokenDFAMarker("COMMENT_EOL", -1)]
        private static DFA lineComment()
        {
            // recognizes the start of a line comment ("//")
            DFA newDFA = new DFA();

            // /, /
            newDFA.Transitions[0, '/'] = 1;
            newDFA.Transitions[1, '/'] = 2;

            newDFA.AcceptStates.Add(2);

            return newDFA;
        }

        [TokenDFAMarker("COMMENT_BLK_START", -1)]
        private static DFA startBlockComment()
        {
            // recognizes the start of a block comment ("/*")
            DFA newDFA = new DFA();

            // /, *
            newDFA.Transitions[0, '/'] = 1;
            newDFA.Transitions[1, '*'] = 2;

            newDFA.AcceptStates.Add(2);

            return newDFA;
        }

        [TokenDFAMarker("COMMENT_BLK_END", -1)]
        private static DFA endBlockComment()
        {
            // recognizes the end of a block comment ("<any-characters>*/")
            DFA newDFA = new DFA();

            // anything, *, /
            for (int i = 0; i < 127; i++)
            {
                // make all transitions from 0 go to 0
                newDFA.Transitions[0, i] = 0;
            }

            // change transition from 0 on '*' to go to 1
            newDFA.Transitions[0, '*'] = 1;
            newDFA.Transitions[1, '/'] = 2;

            newDFA.AcceptStates.Add(2);

            return newDFA;
        }
        #endregion

        #region Keywords - level 0
        [TokenDFAMarker("KW_CONST", 0)]
        private static DFA KW_CONST()
        {
            // recognizes const keywords (true, false, null, this)
            DFA newDFA = new DFA();

            // t[rue,his]
            newDFA.Transitions[0, 't'] = 1;

            newDFA.Transitions[1, 'r'] = 2;
            newDFA.Transitions[2, 'u'] = 3;
            newDFA.Transitions[3, 'e'] = 4;
            newDFA.AcceptStates.Add(4);

            newDFA.Transitions[1, 'h'] = 14;
            newDFA.Transitions[14, 'i'] = 15;
            newDFA.Transitions[15, 's'] = 16;
            newDFA.AcceptStates.Add(16);

            // false
            newDFA.Transitions[0, 'f'] = 5;
            newDFA.Transitions[5, 'a'] = 6;
            newDFA.Transitions[6, 'l'] = 7;
            newDFA.Transitions[7, 's'] = 8;
            newDFA.Transitions[8, 'e'] = 9;
            newDFA.AcceptStates.Add(9);

            // null
            newDFA.Transitions[0, 'n'] = 10;
            newDFA.Transitions[10, 'u'] = 11;
            newDFA.Transitions[11, 'l'] = 12;
            newDFA.Transitions[12, 'l'] = 13;
            newDFA.AcceptStates.Add(13);

            return newDFA;
        }

        [TokenDFAMarker("KW_TYPE", 0)]
        private static DFA KW_TYPE()
        {
            // recognizes type keywords (int, char, boolean)
            DFA newDFA = new DFA();

            // int
            newDFA.Transitions[0, 'i'] = 1;
            newDFA.Transitions[1, 'n'] = 2;
            newDFA.Transitions[2, 't'] = 3;
            newDFA.AcceptStates.Add(3);

            // char
            newDFA.Transitions[0, 'c'] = 4;
            newDFA.Transitions[4, 'h'] = 5;
            newDFA.Transitions[5, 'a'] = 6;
            newDFA.Transitions[6, 'r'] = 7;
            newDFA.AcceptStates.Add(7);

            // boolean
            newDFA.Transitions[0, 'b'] = 8;
            newDFA.Transitions[8, 'o'] = 9;
            newDFA.Transitions[9, 'o'] = 10;
            newDFA.Transitions[10, 'l'] = 11;
            newDFA.Transitions[11, 'e'] = 12;
            newDFA.Transitions[12, 'a'] = 13;
            newDFA.Transitions[13, 'n'] = 14;
            newDFA.AcceptStates.Add(14);

            return newDFA;
        }

        [TokenDFAMarker("KW_VARDEC", 0)]
        private static DFA KW_VARDEC()
        {
            // recognizes a variable declaration (static, field)
            DFA newDFA = new DFA();

            // static
            newDFA.Transitions[0, 's'] = 1;
            newDFA.Transitions[1, 't'] = 2;
            newDFA.Transitions[2, 'a'] = 3;
            newDFA.Transitions[3, 't'] = 4;
            newDFA.Transitions[4, 'i'] = 5;
            newDFA.Transitions[5, 'c'] = 6;
            newDFA.AcceptStates.Add(6);

            // field
            newDFA.Transitions[0, 'f'] = 7;
            newDFA.Transitions[7, 'i'] = 8;
            newDFA.Transitions[8, 'e'] = 9;
            newDFA.Transitions[9, 'l'] = 10;
            newDFA.Transitions[10, 'd'] = 11;
            newDFA.AcceptStates.Add(11);

            return newDFA;
        }

        [TokenDFAMarker("KW_SUBDEC", 0)]
        private static DFA KW_SUBDEC()
        {
            // recognizes a sub declaration (constructor, function, method)
            DFA newDFA = new DFA();

            // constructor
            newDFA.Transitions[0, 'c'] = 1;
            newDFA.Transitions[1, 'o'] = 2;
            newDFA.Transitions[2, 'n'] = 3;
            newDFA.Transitions[3, 's'] = 4;
            newDFA.Transitions[4, 't'] = 5;
            newDFA.Transitions[5, 'r'] = 6;
            newDFA.Transitions[6, 'u'] = 7;
            newDFA.Transitions[7, 'c'] = 8;
            newDFA.Transitions[8, 't'] = 9;
            newDFA.Transitions[9, 'o'] = 10;
            newDFA.Transitions[10, 'r'] = 11;
            newDFA.AcceptStates.Add(11);

            // function
            newDFA.Transitions[0, 'f'] = 12;
            newDFA.Transitions[12, 'u'] = 13;
            newDFA.Transitions[13, 'n'] = 14;
            newDFA.Transitions[14, 'c'] = 15;
            newDFA.Transitions[15, 't'] = 16;
            newDFA.Transitions[16, 'i'] = 17;
            newDFA.Transitions[17, 'o'] = 18;
            newDFA.Transitions[18, 'n'] = 19;
            newDFA.AcceptStates.Add(19);

            // method
            newDFA.Transitions[0, 'm'] = 20;
            newDFA.Transitions[20, 'e'] = 21;
            newDFA.Transitions[21, 't'] = 22;
            newDFA.Transitions[22, 'h'] = 23;
            newDFA.Transitions[23, 'o'] = 24;
            newDFA.Transitions[24, 'd'] = 25;
            newDFA.AcceptStates.Add(25);

            return newDFA;
        }

        [TokenDFAMarker("KW_VAR", 0)]
        private static DFA KW_VAR()
        {
            // recognizes the var keyword
            DFA newDFA = new DFA();

            // var
            newDFA.Transitions[0, 'v'] = 1;
            newDFA.Transitions[1, 'a'] = 2;
            newDFA.Transitions[2, 'r'] = 3;
            newDFA.AcceptStates.Add(3);

            return newDFA;
        }

        [TokenDFAMarker("KW_VOID", 0)]
        private static DFA KW_VOID()
        {
            // recognizes the void keyword
            DFA newDFA = new DFA();

            // void
            newDFA.Transitions[0, 'v'] = 1;
            newDFA.Transitions[1, 'o'] = 2;
            newDFA.Transitions[2, 'i'] = 3;
            newDFA.Transitions[3, 'd'] = 4;
            newDFA.AcceptStates.Add(4);

            return newDFA;
        }

        [TokenDFAMarker("KW_CLASS", 0)]
        private static DFA KW_CLASS()
        {
            // recognizes the class keyword
            DFA newDFA = new DFA();

            // class
            newDFA.Transitions[0, 'c'] = 1;
            newDFA.Transitions[1, 'l'] = 2;
            newDFA.Transitions[2, 'a'] = 3;
            newDFA.Transitions[3, 's'] = 4;
            newDFA.Transitions[4, 's'] = 5;
            newDFA.AcceptStates.Add(5);

            return newDFA;
        }

        [TokenDFAMarker("KW_LET", 0)]
        private static DFA KW_LET()
        {
            // recognizes the let keyword
            DFA newDFA = new DFA();

            // let
            newDFA.Transitions[0, 'l'] = 1;
            newDFA.Transitions[1, 'e'] = 2;
            newDFA.Transitions[2, 't'] = 3;
            newDFA.AcceptStates.Add(3);

            return newDFA;
        }

        [TokenDFAMarker("KW_IF", 0)]
        private static DFA KW_IF()
        {
            // recognizes the if keyword
            DFA newDFA = new DFA();

            // if
            newDFA.Transitions[0, 'i'] = 1;
            newDFA.Transitions[1, 'f'] = 2;
            newDFA.AcceptStates.Add(2);

            return newDFA;
        }

        [TokenDFAMarker("KW_ELSE", 0)]
        private static DFA KW_ELSE()
        {
            // recognizes the else keyword
            DFA newDFA = new DFA();

            // else
            newDFA.Transitions[0, 'e'] = 1;
            newDFA.Transitions[1, 'l'] = 2;
            newDFA.Transitions[2, 's'] = 3;
            newDFA.Transitions[3, 'e'] = 4;
            newDFA.AcceptStates.Add(4);

            return newDFA;
        }

        [TokenDFAMarker("KW_WHILE", 0)]
        private static DFA KW_WHILE()
        {
            // recognizes the while keyword
            DFA newDFA = new DFA();

            // while
            newDFA.Transitions[0, 'w'] = 1;
            newDFA.Transitions[1, 'h'] = 2;
            newDFA.Transitions[2, 'i'] = 3;
            newDFA.Transitions[3, 'l'] = 4;
            newDFA.Transitions[4, 'e'] = 5;
            newDFA.AcceptStates.Add(5);

            return newDFA;
        }

        [TokenDFAMarker("KW_DO", 0)]
        private static DFA KW_DO()
        {
            // recognizes the do keyword
            DFA newDFA = new DFA();

            // do
            newDFA.Transitions[0, 'd'] = 1;
            newDFA.Transitions[1, 'o'] = 2;
            newDFA.AcceptStates.Add(2);

            return newDFA;
        }

        [TokenDFAMarker("KW_RETURN", 0)]
        private static DFA KW_RETURN()
        {
            // recognizes the return keyword
            DFA newDFA = new DFA();

            // return
            newDFA.Transitions[0, 'r'] = 1;
            newDFA.Transitions[1, 'e'] = 2;
            newDFA.Transitions[2, 't'] = 3;
            newDFA.Transitions[3, 'u'] = 4;
            newDFA.Transitions[4, 'r'] = 5;
            newDFA.Transitions[5, 'n'] = 6;
            newDFA.AcceptStates.Add(6);

            return newDFA;
        }
        #endregion

        #region Symbols - level 1
        [TokenDFAMarker("SY_LPAREN", 1)]
        private static DFA SY_LPAREN()
        {
            // recognizes a left parenthesis
            DFA newDFA = new DFA();

            // (
            newDFA.Transitions[0, '('] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_RPAREN", 1)]
        private static DFA SY_RPAREN()
        {
            // recognizes a right parenthesis
            DFA newDFA = new DFA();

            // )
            newDFA.Transitions[0, ')'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_LBRACKET", 1)]
        private static DFA SY_LBRACKET()
        {
            // recognizes a left bracket
            DFA newDFA = new DFA();

            // [
            newDFA.Transitions[0, '['] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_RBRACKET", 1)]
        private static DFA SY_RBRACKET()
        {
            // recognizes a right bracket
            DFA newDFA = new DFA();

            // ]
            newDFA.Transitions[0, ']'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_LBRACE", 1)]
        private static DFA SY_LBRACE()
        {
            // recognizes a left brace
            DFA newDFA = new DFA();

            // {
            newDFA.Transitions[0, '{'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_RBRACE", 1)]
        private static DFA SY_RBRACE()
        {
            // recognizes a right brace
            DFA newDFA = new DFA();

            // }
            newDFA.Transitions[0, '}'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_SEMI", 1)]
        private static DFA SY_SEMI()
        {
            // recognizes a semi colon
            DFA newDFA = new DFA();

            // ;
            newDFA.Transitions[0, ';'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_PERIOD", 1)]
        private static DFA SY_PERIOD()
        {
            // recognizes a period
            DFA newDFA = new DFA();

            // .
            newDFA.Transitions[0, '.'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_COMMA", 1)]
        private static DFA SY_COMMA()
        {
            // recognizes a comma
            DFA newDFA = new DFA();

            // ,
            newDFA.Transitions[0, ','] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_EQ", 1)]
        private static DFA SY_EQ()
        {
            // recognizes equals sign
            DFA newDFA = new DFA();

            // =
            newDFA.Transitions[0, '='] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_MINUS", 1)]
        private static DFA SY_MINUS()
        {
            // recognizes minus sign
            DFA newDFA = new DFA();

            // -
            newDFA.Transitions[0, '-'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_NOT", 1)]
        private static DFA SY_NOT()
        {
            // recognizes the unary not operator (~)
            DFA newDFA = new DFA();

            // ~
            newDFA.Transitions[0, '~'] = 1;
            newDFA.AcceptStates.Add(1);

            return newDFA;
        }

        [TokenDFAMarker("SY_OP", 1)]
        private static DFA SY_OP()
        {
            // recognizes an operator (+, *, /, &, |, <, >)
            DFA newDFA = new DFA();

            // +
            newDFA.Transitions[0, '+'] = 1;
            newDFA.AcceptStates.Add(1);

            // *
            newDFA.Transitions[0, '*'] = 2;
            newDFA.AcceptStates.Add(2);

            // /
            newDFA.Transitions[0, '/'] = 3;
            newDFA.AcceptStates.Add(3);

            // &
            newDFA.Transitions[0, '&'] = 4;
            newDFA.AcceptStates.Add(4);

            // |
            newDFA.Transitions[0, '|'] = 5;
            newDFA.AcceptStates.Add(5);

            // <
            newDFA.Transitions[0, '<'] = 6;
            newDFA.AcceptStates.Add(6);

            // >
            newDFA.Transitions[0, '>'] = 7;
            newDFA.AcceptStates.Add(7);

            return newDFA;
        }
        #endregion

        #region Identifiers - level 2
        [TokenDFAMarker("IDENT", 2)]
        private static DFA IDENT()
        {
            // recognizes an identifier (variable name, function name, etc)
            // an identifier is any sequence of letters, digits, and underscores
            // that does not start with a digit (and is not a keyword - which is coverd
            // by IDENT being a higher level, thus lower priority, then keywords)
            DFA newDFA = new DFA();

            // (a-z,A-Z,_)(a-z,A-Z,_,0-9)*
            for (char ch = 'a'; ch <= 'z'; ch++)
            {
                newDFA.Transitions[0, ch] = 1;
            }
            for (char ch = 'A'; ch <= 'Z'; ch++)
            {
                newDFA.Transitions[0, ch] = 1;
            }
            newDFA.Transitions[0, '_'] = 1;

            for (char ch = 'a'; ch <= 'z'; ch++)
            {
                newDFA.Transitions[1, ch] = 1;
            }
            for (char ch = 'A'; ch <= 'Z'; ch++)
            {
                newDFA.Transitions[1, ch] = 1;
            }
            for (char ch = '0'; ch <= '9'; ch++)
            {
                newDFA.Transitions[1, ch] = 1;
            }
            newDFA.Transitions[1, '_'] = 1;

            newDFA.AcceptStates.Add(1);

            return newDFA;
        }
        #endregion

        #region Literals - level 3
        [TokenDFAMarker("INTEGER", 3)]
        private static DFA INTEGER()
        {
            // Reconginzes an integer literal (decimal between 0 and 32767 inclusive)
            DFA newDFA = new DFA();

            // state n = accept if number has n - 5 digits or less left
            // 4 digits left - state 9
            for (char ch = '0'; ch <= '9'; ch++)
            {
                newDFA.Transitions[9, ch] = 8;
            }
            newDFA.AcceptStates.Add(9);

            // 3 digits left - state 8
            for (char ch = '0'; ch <= '9'; ch++)
            {
                newDFA.Transitions[8, ch] = 7;
            }
            newDFA.AcceptStates.Add(8);

            // 2 digits left - state 7
            for (char ch = '0'; ch <= '9'; ch++)
            {
                newDFA.Transitions[7, ch] = 6;
            }
            newDFA.AcceptStates.Add(7);

            // 1 digit left - state 6
            for (char ch = '0'; ch <= '9'; ch++)
            {
                newDFA.Transitions[6, ch] = 5;
            }
            newDFA.AcceptStates.Add(6);

            // 0 digits left, state 5
            // no transitions out of state 5 - any new char means it's not a valid number
            newDFA.AcceptStates.Add(5);

            // if digit is less than 3, can accept if <5 digits left
            for (char ch = '0'; ch <= '2'; ch++)
            {
                newDFA.Transitions[0, ch] = 9;
            }
            // if digit is 3, might be max value, move to next digit and check for 2
            newDFA.Transitions[0, '3'] = 1;
            // if digit is > 3, can accept if <4 digits left
            for (char ch = '4'; ch <= '9'; ch++)
            {
                newDFA.Transitions[0, ch] = 8;
            }

            // if next digit is less than 2, can accept if <4 digits left
            for (char ch = '0'; ch <= '1'; ch++)
            {
                newDFA.Transitions[1, ch] = 8;
            }
            // if digit is 2, might be max value, move to next digit and check for 7
            newDFA.Transitions[1, '2'] = 2;
            // if digit is > 2, can accept if <3 digits left
            for (char ch = '3'; ch <= '9'; ch++)
            {
                newDFA.Transitions[1, ch] = 7;
            }

            // if next digit is less than 7, can accept if <3 digits left
            for (char ch = '0'; ch <= '6'; ch++)
            {
                newDFA.Transitions[2, ch] = 7;
            }
            // if digit is 7, might be max value, move to next digit and check for 6
            newDFA.Transitions[2, '7'] = 3;
            // if digit is > 7, can accept if <2 digits left
            for (char ch = '8'; ch <= '9'; ch++)
            {
                newDFA.Transitions[2, ch] = 6;
            }

            // if next digit is less than 6, can accept if <2 digits left
            for (char ch = '0'; ch <= '5'; ch++)
            {
                newDFA.Transitions[3, ch] = 6;
            }
            // if digit is 6, might be max value, move to next digit and check for 7
            newDFA.Transitions[3, '6'] = 4;
            // if digit is > 6, can accept if <1 digits left
            for (char ch = '7'; ch <= '9'; ch++)
            {
                newDFA.Transitions[3, ch] = 5;
            }

            // if next digit is less than 7, can accept if <1 digits left
            for (char ch = '0'; ch <= '6'; ch++)
            {
                newDFA.Transitions[4, ch] = 5;
            }
            // if digit is 7, might be max value, only accept if no digits left
            newDFA.Transitions[4, '7'] = 5;
            // if digit is > 7, we have exceded max value, no transitions needed

            // accept states 1-4
            newDFA.AcceptStates.Add(1);
            newDFA.AcceptStates.Add(2);
            newDFA.AcceptStates.Add(3);
            newDFA.AcceptStates.Add(4);

            return newDFA;
        }

        [TokenDFAMarker("STRING", 3)]
        private static DFA STRING()
        {
            // recognizes a string literal (sequence of ascii characters, not including " or \n, enclosed in ")
            DFA newDFA = new DFA();

            // "(ascii chars - {'\"', '\n'})*"
            newDFA.Transitions[0, '\"'] = 1;
            for (char ch = (char)0; ch < 127; ch++)
            {
                if (!char.IsControl(ch) && ch != '\"' && ch != '\n')
                {
                    newDFA.Transitions[1, ch] = 1;
                }
            }
            newDFA.Transitions[1, '\"'] = 2;
            newDFA.AcceptStates.Add(2);

            return newDFA;
        }
        #endregion

        #endregion
    }
}