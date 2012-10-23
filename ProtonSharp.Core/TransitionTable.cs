#region License and Copyright Notice
// Copyright (c) 2010 Ananth Balasubramaniam
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace ProtonSharp.Core
{
    public sealed class StateSet: SortedSet<int> { }
    public sealed class StateSetsVector : List<StateSet> { }

    public sealed class StateSetIdMap : Dictionary<string, int> { }
    public sealed class StateTransitionMap : Dictionary<int, List<TouchSymbol>>{ } // Is this List<TouchSymbol[]> ?
    public sealed class StateMapMap : Dictionary<int, StateTransitionMap> { }

    public sealed class TransitionTable
    {
        private readonly StateMapMap _transitionMap;
        private int _startState = 1; // Typically start state is numbered 1

        public void AddTransition(TouchSymbol touchSymbol, int stateOne, int stateTwo)
        {
            StateTransitionMap mapOne;
            if (!_transitionMap.TryGetValue(stateOne, out mapOne))
            {
                mapOne = new StateTransitionMap();
                _transitionMap.Add(stateOne, mapOne);
            }

            StateTransitionMap mapTwo;
            if (!_transitionMap.TryGetValue(stateTwo, out mapTwo))
            {
                mapTwo = new StateTransitionMap();
                _transitionMap.Add(stateTwo, mapTwo);
            }

            List<TouchSymbol> transitions;
            if (!mapOne.TryGetValue(stateTwo, out transitions))
            {
                transitions = new List<TouchSymbol>();
                mapOne.Add(stateTwo, transitions);
            }

            if (!transitions.Contains(touchSymbol))
                transitions.Add(touchSymbol);
        }

        private string StateSetToString(StateSet stateSet)
        {
            if (stateSet.Count == 0) return string.Empty;
            return string.Join("_", stateSet);
        }

        public int ConvertToDFA(TransitionTable dfa)
        {
            StateSetIdMap dfaSetToStateMap = new StateSetIdMap();
            int dfaStateCount = 1;

            StateSetsVector stateQueue = new StateSetsVector();
            StateSetsVector stateChecked = new StateSetsVector();
            StateSet startStateSet = new StateSet();

            stateQueue.Add(startStateSet);

            dfaSetToStateMap[StateSetToString(startStateSet)] = dfaStateCount;
            dfaStateCount++;

            while (stateQueue.Count > 0)
            {
                StateSet stateSet = stateQueue[0];
                int dfaStateInt = dfaSetToStateMap[StateSetToString(stateSet)];

                Dictionary<string, StateSet> transitionMap = new Dictionary<string, StateSet>();
                Dictionary<string, TouchSymbol> transitionMapForTouchSymbol = new Dictionary<string, TouchSymbol>();

                foreach (var it in stateSet)
                {
                    int stateInt = it;
                    var stateMap = _transitionMap[stateInt];
                    foreach (var jt in stateMap)
                    {
                        int nextStateInt = jt.Key;
                        var nextStateTransitions = jt.Value;

                        for (int i = 0; i < nextStateTransitions.Count; i++)
                        {
                            var transitionTouchSymbol = nextStateTransitions[i];
                            var touchSymbol = transitionMapForTouchSymbol[transitionTouchSymbol.SymbolString];

                            if (!transitionMap.ContainsKey(transitionTouchSymbol.SymbolString))
                            {
                                transitionMap[transitionTouchSymbol.SymbolString] = new SortedSet<int> { nextStateInt };
                            }
                            else
                            {
                                var setset = transitionMap[touchSymbol.SymbolString];
                                setset.Add(nextStateInt);
                                if (transitionTouchSymbol.SymbolString.CompareTo(touchSymbol.SymbolString) == 0)
                                    touchSymbol.AddTriggers(transitionTouchSymbol.Triggers);
                            }
                        }
                    }
                }

                foreach (var kt in transitionMap)
                {
                    var transitionMapSet = kt.Value;
                    var transitionTouchSymbol = transitionMapForTouchSymbol[kt.Key];

                    var newSet = true;
                    for (int ii = 0; newSet && ii < stateQueue.Count; ii++)
                        if (transitionMapSet == stateChecked[ii])
                            newSet = false; // TODO: break after this?

                    if (newSet)
                        stateQueue.Add(transitionMapSet);

                    var transitionMapSetString = StateSetToString(transitionMapSet);
                    var transitionMapSetInt = -1;
                    if (!dfaSetToStateMap.ContainsKey(transitionMapSetString))
                    {
                        transitionMapSetInt = dfaStateCount;
                        dfaSetToStateMap[transitionMapSetString] = transitionMapSetInt;
                        dfaStateCount++;
                    }
                    else
                        transitionMapSetInt = dfaSetToStateMap[transitionMapSetString];

                    var storedTouchSymbol = transitionTouchSymbol;
                    dfa.AddTransition(storedTouchSymbol, dfaStateInt, transitionMapSetInt);
                }

                stateChecked.Add(stateSet);
                stateQueue.RemoveAt(0);
            }

            if (dfa._transitionMap.Count > 0)
                dfa._startState = 1;

            return 1;
        }

        public bool IsAcceptState(int state)
        {
            StateTransitionMap value;
            return _transitionMap.TryGetValue(state, out value) ? 
                (value.Count == 0) ? true : false :
                false;
        }

        // use www.graphviz.org to display .dot file format
        // http://en.wikipedia.org/wiki/DOT_language        
        public void PrintDotFormat(TextWriter writer)
        {
            writer.WriteLine("digraph G {");
            foreach (var it in _transitionMap)
            {
                int state = it.Key;
                var stateMap = it.Value;
                foreach (var iit in stateMap)
                {
                    int nextState = iit.Key;
                    var transitions = iit.Value;
                    for (int i = 0; i < transitions.Count; i++)
                    {
                        var touchSymbol = transitions[i];
                        var symbolString = touchSymbol.SymbolString;
                        writer.WriteLine("\t{0} -> {1} [label=\"{2}\"]", state, nextState, symbolString);
                    }
                }
            }
            writer.WriteLine("}");
        }

        public void RemoveEmptyTransitions()
        { 
            // TODO: Continue here...
        }
    }
}