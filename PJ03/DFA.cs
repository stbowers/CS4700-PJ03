using System;
using System.Collections.Generic;

namespace PJ03
{
    public class DFA
    {
        /// <summary>
        /// Transition table where Transitions[x, y] is the state to transition to
        /// given a start state x, and reading the character y
        /// </summary>
        public System.Byte[,] Transitions = new System.Byte[256, 127];

        /// <summary>
        /// The list of accept states for this DFA
        /// </summary>
        public List<System.Byte> AcceptStates = new List<System.Byte>();

        // The DFA's current state, start at 0
        System.Byte currentState = 0;

        // Constructor
        public DFA()
        {
            // Set all ints in transition table to 255; all transitions go to trap state 255
            for (int i = 0; i < Transitions.GetLength(0); i++)
            {
                for (int j = 0; j < Transitions.GetLength(1); j++)
                {
                    Transitions[i, j] = 255;
                }
            }
        }

        /// <summary>
        /// Move the DFA to the next state given an input character
        /// </summary>
        public void Step(char transition)
        {
            currentState = Transitions[currentState, transition];
        }

        /// <summary>
        /// Returns true if the DFA is currently in an accept state
        /// </summary>
        public bool DoesAccept()
        {
            return AcceptStates.Contains(currentState);
        }

        /// <summary>
        /// Move DFA back to the start state
        /// </summary>
        public void Reset()
        {
            currentState = 0;
        }
    }
}