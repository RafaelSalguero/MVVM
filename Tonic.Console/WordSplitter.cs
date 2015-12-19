using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Console
{
    public static class WordSplitter
    {
        enum WState
        {
            OnCode,
            OnString
        }
        public static List<List<string>> SplitLines(string Code)
        {
            StringBuilder B = new StringBuilder();
            List<List<string>> result = new List<List<string>>();
            result.Add(new List<string>());

            Action AddWord = () =>
            {
                var W = B.ToString().Trim();
                B.Clear();
                if (W != "")
                {
                    result[result.Count - 1].Add(W);
                }
            };

            Action AddLine = () =>
            {
                AddWord();
                if (result[result.Count - 1].Count > 0)
                    result.Add(new List<string>());
            };

            WState stateM = WState.OnCode;
            for (int i = 0; i < Code.Length; i++)
            {
                switch (stateM)
                {
                    case WState.OnCode:
                        {
                            if (Code[i] == '"' || Code[i] == '\'')
                            {
                                AddWord();
                                B.Append("str");
                                AddWord();

                                stateM = WState.OnString;
                                AddWord();
                            }
                            else if (Code[i] == '(' || Code[i] == ')')
                            {
                                AddLine();
                                B.Append(Code[i]);
                                AddWord();
                                AddLine();

                            }
                            else if (Code[i] == ',')
                            {
                                AddLine();
                            }
                            else if (char.IsWhiteSpace(Code[i]))
                            {
                                AddWord();
                            }
                            else
                                B.Append(Code[i]);
                            break;
                        }
                    case WState.OnString:
                        {
                            if (Code[i] == '"' || Code[i] == '\'')
                            {
                                AddWord();
                                stateM = WState.OnCode;
                            }
                            else
                                B.Append(Code[i]);
                            break;
                        }
                }
            }
            AddWord();
            if (result[result.Count - 1].Count == 0)
                result.RemoveAt(result.Count - 1);

            return result;
        }
    }
}
