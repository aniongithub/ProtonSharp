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
using ProtonSharp.Core;

namespace ProtonSharp.Core
{
    public enum TouchAction
    {
        Down,
        Move,
        Up,
        Other,
    };
    
    public sealed class TouchSymbol
    {
        private readonly List<string> _attributeValues = new List<string>();
        private string _symbolString;
        private readonly List<int> _triggers;

        public void AddAttributeValue(string attributeValue)
        {
            _attributeValues.Add(attributeValue);
        }

        public void AddTriggers(IEnumerable<int> triggers)
        { 
            foreach (int trigger in triggers)
                if (!_triggers.Contains(trigger))
                    _triggers.Add(trigger);

            _triggers.Sort();
        }

        public void AddTrigger(int trigger)
        {
            if (!_triggers.Contains(trigger))
                _triggers.Add(trigger);
            
            _triggers.Sort();
        }

        public int AttributeValueCount
        {
            get { return _attributeValues.Count; }
        }

        public string AttributeValueAtIndex(int index)
        {
            return _attributeValues[index];
        }

        public bool IsEqualToString(string str)
        {
            return _symbolString.CompareTo(str) == 0;
        }

        public int TouchId
        {
            get
            {
                return _symbolString.Length < 2 ? -1 :
                    int.Parse(_symbolString.Substring(1, _symbolString.Length - 1));
            }
            set
            {
                _symbolString = string.Format("{0}{1}", _symbolString.Substring(0, 1), value);
            }
        }

        public TouchAction TouchAction
        {
            get 
            {
                if (_symbolString.Length < 2) return TouchAction.Other;
                switch (_symbolString[0])
                { 
                    case 'D': return TouchAction.Down;
                    case 'M': return TouchAction.Move;
                    case 'U': return TouchAction.Up;

                    default:
                        return TouchAction.Other;
                }
            }
        }

        public List<int> Triggers { get { return _triggers; } }

        public string SymbolString
        {
            get 
            {
                var symbolString = new StringBuilder(_symbolString);
                for (int i = 0; i < _attributeValues.Count; i++)
                {
                    symbolString.Append(":");
                    symbolString.Append(_attributeValues[i]);
                }

                return symbolString.ToString();
            }
        }

        private void SetSymbolString(string symbolString)
        {
            var actionId = false;

            int i = 0;
            while (symbolString.Length - i > 0)
            {
                string subSymbolString;
                int found = symbolString.IndexOf(':', i);
                if (found == -1)
                {
                    subSymbolString = symbolString.Substring(i, symbolString.Length - i);
                    i = symbolString.Length;
                }
                else
                {
                    subSymbolString = symbolString.Substring(i, found - i);
                    i = found + 1; // + delimiter length, which is 1 for ':'
                }

                if (!actionId)
                {
                    symbolString = subSymbolString;
                    actionId = true;
                }
                else
                    AddAttributeValue(subSymbolString);
            }
        }

        public TouchSymbol(string symbolString)
        {
            SetSymbolString(symbolString);
        }

        public TouchSymbol(string symbolString, List<int> triggers)
            : this(symbolString)
        {
            _triggers = triggers;
        }
    }

    public sealed class TouchSymbols : List<TouchSymbols>
    {}
}