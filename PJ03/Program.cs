using System;
using System.IO;
using System.Collections.Generic;

namespace PJ03
{
    class Program
    {
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
            File.WriteAllText(tokenFile, "");

            // read jack file into string
            string file = File.ReadAllText(jackFile);

            // append newline to file string, to ensure the last token is recognized
            file += '\n';

            // Get tokens from input file
            (string, string)[] tokens = TokenizeString(file);

            // Write tokens to output file
            foreach ((string tokenType, string token) in tokens)
            {
                File.AppendAllText(
                    tokenFile,
                    String.Format("{0}, {1}\n", tokenType, token)
                );
            }
        }

        public static (string tokenType, string token)[] TokenizeString(string file)
        {
            // Reset DFAs
            ResetDFAs();

            // List of matched tokens
            List<(string, string)> matchedTokens = new List<(string, string)>();

            // The last token recognized
            string lastTokenType = "";
            string lastTokenText = "";

            // loop through every character in file
            foreach (char ch in file)
            {
                // The token type which best matches the string read thusfar
                string bestMatch = "";

                // The string that matched the token
                string match = "";

                // Flag for if any machines are still running (not in state 255/non accepting trap state)
                bool machinesRunning = false;

                /* Simulate all token DFAs - the array is sorted by which token types
                 * should take precidence, so the last accepting DFA that is run should
                 * be used
                 */
                (bestMatch, match, machinesRunning) = RunDFAs(ch);

                if (bestMatch == "WHITESPACE")
                {
                    /* Check if any machines are still running, (some machines like comments or strings recognize whitespace)
                     * and if none are, that means this whitespace should be used to delimit a token, so add last recognized
                     * token to the list of matched tokens
                     */
                    if (!machinesRunning && (lastTokenType != ""))
                    {
                        matchedTokens.Add((lastTokenType, lastTokenText));

                        // reset last token
                        lastTokenType = "";
                        lastTokenText = "";

                        // reset DFAs
                        ResetDFAs();
                    }

                    if (!machinesRunning)
                    {
                        ResetDFAs();
                    }
                }
                else if (bestMatch == "COMMENT_EOL" || bestMatch == "COMMENT_BLK")
                {
                    // Input matched a comment, don't tokenize coments

                    // reset last token
                    lastTokenType = "";
                    lastTokenText = "";

                    // reset DFAs
                    ResetDFAs();
                }
                else if (bestMatch == "" && !machinesRunning)
                {
                    // If the input didn't match any tokens, and no machines are still running
                    if (lastTokenType != "")
                    {
                        // if we did find a token earlier in the string, add it to list
                        matchedTokens.Add((lastTokenType, lastTokenText));

                        // reset DFAs, and run them on ch again
                        ResetDFAs();
                        (bestMatch, match, machinesRunning) = RunDFAs(ch);

                        // If a match was found for this char, set lastToken
                        if (bestMatch != "")
                        {
                            lastTokenType = bestMatch;
                            lastTokenText = match;

                            // If no machines are running anymore, write token
                            if (!machinesRunning)
                            {
                                matchedTokens.Add((lastTokenType, lastTokenText));

                                // reset last token
                                lastTokenType = "";
                                lastTokenText = "";

                                // reset DFAs
                                ResetDFAs();
                            }
                        }
                    }
                    else
                    {
                        // If the input didn't match any tokens, and no machines are still running, assume current token is bad
                        lastTokenType = "BADTOKEN";
                        lastTokenText += ch;
                    }
                }
                else if (bestMatch != "")
                {
                    // Otherwise, remember string thusfar matched bestMatch
                    lastTokenType = bestMatch;
                    lastTokenText = match;
                }
            }

            return matchedTokens.ToArray();
        }

        /// <summary>
        /// Reset all DFAs to their start state
        /// </summary>
        public static void ResetDFAs()
        {
            foreach ((DFA machine, string recognizedToken) in TOKEN_DFAS)
            {
                machine.Reset();
            }
        }

        /// <summary>
        /// Simulate all token DFAs with the given character
        /// </summary>
        /// <returns>A tuple of (the best match for the token type, are any non-accepting machines still running (might accept later))</returns>
        public static (string bestMatch, string match, bool machinesRunning) RunDFAs(char ch)
        {
            string bestMatch = "";
            string match = "";
            bool machinesRunning = false;

            foreach ((DFA machine, string recognizedToken) in TOKEN_DFAS)
            {
                machine.Step(ch);

                // If the machine accepts save the token it recognizes
                if (machine.DoesAccept())
                {
                    bestMatch = recognizedToken;
                    match = machine.GetReadString();
                }

                // If the machine is still running (not in a trap state), and is not the whitespace machine, set machinesRunning flag
                if (machine.IsRunning() && recognizedToken != "WHITESPACE")
                {
                    machinesRunning = true;
                }
            }

            return (bestMatch, match, machinesRunning);
        }
    }
}
