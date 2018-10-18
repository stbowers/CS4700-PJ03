using System;

namespace PJ03
{
    class Program
    {
        // Whitespace characters should be used to delimit tokens
        public static readonly char[] TOKEN_DELIMITERS = new[] { ' ', '\t', '\n', '\r' };

        // List of DFAs which recognize a certain token, and what that token is (string to print to token file)
        // Array is ordered by the precedence of each token, so if a string matches two tokens the one with a
        // lower index will be chosen
        public static readonly (DFA, string)[] TOKEN_DFAS = TokenDFAs.GetTokenDFAs();

        static void Main(string[] args)
        {
            (string, string)[] sources = {
                // (sourceFile, outputTokenFile)
                ("./JackFiles/Ball.jack", "./Output/Ball.tok"),
                ("./JackFiles/Bat.jack", "./Output/Bat.tok"),
                ("./JackFiles/Main.jack", "./Output/Main.tok"),
                ("./JackFiles/PongGame.jack", "./Output/PongGame.tok"),
                ("./JackFiles/Square.jack", "./Output/Square.tok"),
                ("./JackFiles/SquareGame.jack", "./Output/SquareGame.tok"),
                ("./JackFiles/Test.jack", "./Output/Test.tok"),
            };

            foreach ((string source, string output) in sources)
            {
                Console.WriteLine("Lexing file: {0} -> {1}", source, output);
                ParseJackFile(source, output);
            }
        }

        public static void ParseJackFile(string jackFile, string tokenFile)
        {
            // Create/clear output token file
            System.IO.File.WriteAllText(tokenFile, "");

            // read jack file into string
            string file = System.IO.File.ReadAllText(jackFile);

            // the last recognized token
            string token = "";

            // the index at which token was recognized
            int lastValidIndex = 0;

            // the index of the last fully processed char
            int lastProcessedIndex = -1;

            // The number of characters seen since the last delimiter
            int charsSinceLastDelimiter = 0;

            // bools to keep track of if we're in a comment block
            bool inEOLComment = false;
            bool inBlockComment = false;

            // loop through every character in file
            for (int i = 0; i < file.Length; i++)
            {
                char ch = file[i];

                // If ch is in TOKEN_DELIMETERS, stop reading the current token and write it out
                if (Array.Find(TOKEN_DELIMITERS, x => x == ch) == ch)
                {
                    // If the last processed character is the one before this, increment it
                    if (lastProcessedIndex == i - 1)
                    {
                        lastProcessedIndex++;
                    }

                    // check if we're currently in a comment section
                    if (inEOLComment || inBlockComment)
                    {
                        // check if we should end the comment section
                        if (inEOLComment && ch == '\n')
                        {
                            inEOLComment = false;

                            // reset token
                            token = "";
                            lastProcessedIndex = i;
                            lastValidIndex = i;
                        }
                        else if (inBlockComment && token == "COMMENT_BLK_END")
                        {
                            inBlockComment = false;

                            // reset token
                            token = "";
                            lastProcessedIndex = i;
                            lastValidIndex = i;
                        }
                    }
                    // Check for beginning comment section
                    else if (token == "COMMENT_EOL")
                    {
                        inEOLComment = true;
                    }
                    else if (token == "COMMENT_BLK_START")
                    {
                        inBlockComment = true;
                    }
                    // If not a comment, check if we actually recognzied any token
                    else if (token != "")
                    {
                        // write token
                        string tokenLine = String.Format("{0}, {1}\n", token, file.Substring(lastProcessedIndex + 1, (lastValidIndex + 1) - (lastProcessedIndex + 1)));
                        System.IO.File.AppendAllText(tokenFile, tokenLine);

                        // reset token
                        token = "";
                        lastProcessedIndex = lastValidIndex;

                        // move processing back to right after the token that was read, so we can continue reading the line if there is more
                        i = lastValidIndex;
                    }
                    // If we didn't recongize a token, check if we've read any characters since the last recongized token
                    else
                    {
                        if (charsSinceLastDelimiter > 0)
                        {
                            // if we have read any characters since the last valid token, print out a BADTOKEN
                            string tokenLine = String.Format("BADTOKEN, {0}\n", file.Substring(lastProcessedIndex + 1, i - (lastProcessedIndex + 1)));
                            System.IO.File.AppendAllText(tokenFile, tokenLine);

                            // reset token
                            lastProcessedIndex = i;
                            lastValidIndex = i;
                        }

                        // If we didn't read any characters since the last valid token, we've just finish reading all tokens util the current
                        // delimeter, and we can continue
                    }

                    // reset DFAs
                    foreach ((DFA dfa, string str) in TOKEN_DFAS)
                    {
                        dfa.Reset();
                    }

                    // reset charsSinceLastDelimiter
                    charsSinceLastDelimiter = 0;
                }
                // Otherwise, process char as part of a token
                else
                {
                    // if char isn't delimiter, add to charsSinceLastDelimiter
                    charsSinceLastDelimiter++;

                    // we should only accept the first token we find for this length,
                    // but we still need to feed the char to all DFAs, so keep track
                    // of if we've found a token, and don't accept new tokens
                    // if we have
                    bool tokenFound = false;

                    // Simulate all token DFAs on this character
                    foreach ((DFA dfa, string recognizedToken) in TOKEN_DFAS)
                    {
                        dfa.Step(ch);

                        // if the dfa accepts the string thusfar, and no higher precident tokens were found for this length, rewrite token
                        if (dfa.DoesAccept() && !tokenFound)
                        {
                            token = recognizedToken;
                            lastValidIndex = i;

                            tokenFound = true;
                        }
                    }
                }
            }
        }

        public static void _ParseJackFile(string jackFile, string tokenFile)
        {
            // Create/clear output token file
            System.IO.File.WriteAllText(tokenFile, "");

            // read jack file into string
            string file = System.IO.File.ReadAllText(jackFile);

            // the last recognized token
            string token = "";

            // the index at which token was recognized
            int lastValidIndex = 0;

            // the index of the last fully processed char
            int lastProcessedIndex = -1;

            // The number of characters seen since the last delimiter
            int charsSinceLastDelimiter = 0;

            // bools to keep track of if we're in a comment block
            bool inEOLComment = false;
            bool inBlockComment = false;

            // loop through every character in file
            for (int i = 0; i < file.Length; i++)
            {
                char ch = file[i];

                // If ch is in TOKEN_DELIMETERS, stop reading the current token and write it out
                if (Array.Find(TOKEN_DELIMITERS, x => x == ch) == ch)
                {
                    // If the last processed character is the one before this, increment it
                    if (lastProcessedIndex == i - 1)
                    {
                        lastProcessedIndex++;
                    }

                    // check if we're currently in a comment section
                    if (inEOLComment || inBlockComment)
                    {
                        // check if we should end the comment section
                        if (inEOLComment && ch == '\n')
                        {
                            inEOLComment = false;

                            // reset token
                            token = "";
                            lastProcessedIndex = i;
                            lastValidIndex = i;
                        }
                        else if (inBlockComment && token == "COMMENT_BLK_END")
                        {
                            inBlockComment = false;

                            // reset token
                            token = "";
                            lastProcessedIndex = i;
                            lastValidIndex = i;
                        }
                    }
                    // Check for beginning comment section
                    else if (token == "COMMENT_EOL")
                    {
                        inEOLComment = true;
                    }
                    else if (token == "COMMENT_BLK_START")
                    {
                        inBlockComment = true;
                    }
                    // If not a comment, check if we actually recognzied any token
                    else if (token != "")
                    {
                        // write token
                        string tokenLine = String.Format("{0}, {1}\n", token, file.Substring(lastProcessedIndex + 1, (lastValidIndex + 1) - (lastProcessedIndex + 1)));
                        System.IO.File.AppendAllText(tokenFile, tokenLine);

                        // reset token
                        token = "";
                        lastProcessedIndex = lastValidIndex;

                        // move processing back to right after the token that was read, so we can continue reading the line if there is more
                        i = lastValidIndex;
                    }
                    // If we didn't recongize a token, check if we've read any characters since the last recongized token
                    else
                    {
                        if (charsSinceLastDelimiter > 0)
                        {
                            // if we have read any characters since the last valid token, print out a BADTOKEN
                            string tokenLine = String.Format("BADTOKEN, {0}\n", file.Substring(lastProcessedIndex + 1, i - (lastProcessedIndex + 1)));
                            System.IO.File.AppendAllText(tokenFile, tokenLine);

                            // reset token
                            lastProcessedIndex = i;
                            lastValidIndex = i;
                        }

                        // If we didn't read any characters since the last valid token, we've just finish reading all tokens util the current
                        // delimeter, and we can continue
                    }

                    // reset DFAs
                    foreach ((DFA dfa, string str) in TOKEN_DFAS)
                    {
                        dfa.Reset();
                    }

                    // reset charsSinceLastDelimiter
                    charsSinceLastDelimiter = 0;
                }
                // Otherwise, process char as part of a token
                else
                {
                    // if char isn't delimiter, add to charsSinceLastDelimiter
                    charsSinceLastDelimiter++;

                    // we should only accept the first token we find for this length,
                    // but we still need to feed the char to all DFAs, so keep track
                    // of if we've found a token, and don't accept new tokens
                    // if we have
                    bool tokenFound = false;

                    // Simulate all token DFAs on this character
                    foreach ((DFA dfa, string recognizedToken) in TOKEN_DFAS)
                    {
                        dfa.Step(ch);

                        // if the dfa accepts the string thusfar, and no higher precident tokens were found for this length, rewrite token
                        if (dfa.DoesAccept() && !tokenFound)
                        {
                            token = recognizedToken;
                            lastValidIndex = i;

                            tokenFound = true;
                        }
                    }
                }
            }
        }
    }
}
