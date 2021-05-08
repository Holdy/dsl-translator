using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DSLTranslator
{
    public class DSLProcessor {

        private IList<LineDefinition> lineDefinitions = new List<LineDefinition>();

        public void LoadLineDefinitionsFromDirectory(string sourceDirectoryName) {
            if (sourceDirectoryName == null) {
                throw new ArgumentNullException(nameof(sourceDirectoryName));
            }

            foreach (string filename in Directory.GetFiles(sourceDirectoryName)) {
                LoadLineDefinitionFromFile(filename);
            }
        }


        private string GetNumberFrom(string input, int index)
        {
            int current = index;
            StringBuilder builder = new StringBuilder();
            while (current < input.Length)
            {
                char c = input[current];
                if (Char.IsDigit(c) || c == '.')
                {
                    builder.Append(c);
                } else
                {
                    break;
                }
                current++;
            }

            return builder.Length > 0 ? builder.ToString() : null;
        }
        private void RollbackScopeItems(List<ScopeItem> scopeItems, int count)
        {
            if (count > 0)
            {
                scopeItems.RemoveAt(scopeItems.Count);
                RollbackScopeItems(scopeItems, count - 1);
            }
        }

        private MatchingDefinition TestMatch(string input, LineDefinition lineDefinition)
        {
            MatchingDefinition result = new MatchingDefinition(lineDefinition);
            int inputIndex = 0;

            foreach (IMatchingStep matchingStep in lineDefinition.MatchingSteps)
            {
                if (matchingStep.GetType() == typeof(ScaffoldMatchingStep))
                {
                    ScaffoldMatchingStep scaffold = (ScaffoldMatchingStep)matchingStep;

                    if ((inputIndex + scaffold.ScaffoldText.Length) > input.Length)
                    {
                        return null; // template is longer than input - so no match.
                    }

                    var segment = input.Substring(inputIndex, scaffold.ScaffoldText.Length);

                    if (segment.Equals(scaffold.ScaffoldText))
                    {
                        inputIndex += segment.Length;
                    } else
                    {
                        return null; // No match.
                    }
                } else // its a placeholder
                {
                    Placeholder placeholder = (Placeholder)matchingStep;
                    if (placeholder.IsAlias || placeholder.IsReference)
                    {
                        int candidateNextPosition = GetPlaceholderFrom(input, inputIndex);
                        if (candidateNextPosition == -1)
                        {
                            // full placeholder not found
                            return null; // No match.
                        }

                        string placeholderContent = input.Substring(inputIndex +1, (candidateNextPosition - inputIndex)-1);
                        result.AddPlaceholderValue(placeholderContent);
                        inputIndex = candidateNextPosition + 1;

                    }
                    else if (placeholder.IsLiteral)
                    {
                        if (placeholder.TypeName.Equals("number"))
                        {
                            string number = GetNumberFrom(input, inputIndex);
                            if (number == null)
                            {
                                return null;
                            }
                            result.AddPlaceholderValue(number);
                            inputIndex += number.Length;
                        }
                        else
                        {
                            throw new NotSupportedException($"Unhandled literal type: {placeholder.TypeName}");
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            if (inputIndex != input.Length)
            {
                return null; //Somethings gone wrong.
            }
            return result;

        }

        private int FindEndFrom(string input, int startIndex, char c)
        {
            int current = startIndex;
            while (current < input.Length)
            {
                if (input[current] == c)
                {
                    return current;
                }
                current++;
            }
            return -1;
        }

        private int GetPlaceholderFrom(string input, int index)
        {
            if (index >= input.Length)
            {
                return -1;
            }
            else if (input[index] == '(')
            {
                int closeBracket = FindEndFrom(input, index + 1, ')');
                if (closeBracket == -1)
                {
                    return -1;
                } else
                {
                    return closeBracket;
                }
            }
            else
            {
                return -1;
            }
        }

        private List<MatchingDefinition> FindMatchingDefinitions(string input)
        {
            List<MatchingDefinition> result = new List<MatchingDefinition>();
      
            foreach (LineDefinition lineDefinition in lineDefinitions)
            {
                MatchingDefinition match = TestMatch(input, lineDefinition);
                if (match != null)
                {
                    result.Add(match);
                }
            }

            return result;
        }

        public DSLTranslation Process(string input) {
            DSLTranslation result = new DSLTranslation();
            List<ScopeItem> scopeItems = new List<ScopeItem>();
            List<MatchingDefinition> lineMatches = new List<MatchingDefinition>();

            foreach(string rawLine in input.Split('\n', StringSplitOptions.RemoveEmptyEntries)) {
                string line = rawLine.Trim();
                if (line.Length > 0) {
                    MatchingDefinition match = ProcessLine(line, result, scopeItems);
                    lineMatches.Add(match);
                }
            }

            // render step
            foreach (MatchingDefinition lineMatch in lineMatches)
            {
                lineMatch.RenderInto(result);
            }

            return result;
        }

        private ScopeItem FindScopeItemByName(List<ScopeItem> items, string name)
        {
            ScopeItem result = null;
            foreach(ScopeItem test in items)
            {
                if (test.Name.Equals(name))
                {
                    result = test;
                }
            }
            return result;
        }

        private String PreflightCheck(MatchingDefinition definition, List<ScopeItem> scopeItems, bool dryRun)
        {
            string errorMessage = null;

            int placeholderIndex = 0;
            foreach (Placeholder placeholder in definition.Placeholders)
            {
                string value = definition.GetPlaceholderValue(placeholderIndex);
                if (placeholder.IsAlias)
                {
                    // trying to create 'value' - check it doesn't already exist
                    ScopeItem existingItem = FindScopeItemByName(scopeItems, value);
                    if (existingItem != null)
                    {
                        errorMessage = $"Already exists: {value}";
                    } else if (!dryRun)
                    {
                        scopeItems.Add(new ScopeItem(value, placeholder.TypeName));
                    }
                }
                placeholderIndex++;
            }

            return errorMessage;
        }

        private MatchingDefinition ProcessLine(string line, DSLTranslation translation, List<ScopeItem> scopeItems) {
            // match the line to a lineDefinition
            List<MatchingDefinition> matchingDefinitions = FindMatchingDefinitions(line);
            List<MatchingDefinition> errorFreeMatch = new List<MatchingDefinition>();

            foreach(MatchingDefinition testMatch in matchingDefinitions)
            {
                String errorMessage = PreflightCheck(testMatch, scopeItems, true); // true = dryRun;
                if (errorMessage != null)
                {
                    testMatch.ErrorMessage = errorMessage;
                } 
                else
                {
                    errorFreeMatch.Add(testMatch);
                }
            }

            if (errorFreeMatch.Count == 0)
            {
                throw new DSLTemplateException("No match for line");
            } 
            else if (errorFreeMatch.Count > 1)
            {
                throw new DSLTemplateException("Multiple matches for line");
            }

            // Single match!
            PreflightCheck(errorFreeMatch[0], scopeItems, false); // false = not a dry run.
            return errorFreeMatch[0];
        }

        public void LoadLineDefinitionFromFile(string fullFilename)
        {
            string filename = Path.GetFileName(fullFilename);
            int extensionIndex = filename.LastIndexOf('.');
            string lineTemplate = Formatting.SimplifySpacing(filename.Substring(0, extensionIndex));

            IList<OutputItem> outputItems = new List<OutputItem>();
            LineDefinition definition = new LineDefinition(lineTemplate, outputItems);

            int lineNumber = 0;
            foreach (string rawLine in File.ReadAllLines(fullFilename))
            {
                string line = rawLine.Trim();
                if (line.Length > 0)
                {
                    int separatorIndex = line.IndexOf(':');
                    if (separatorIndex == -1)
                    {
                        throw new DSLTemplateException($"Output line:{lineNumber} of {lineTemplate} has no ':' separator");
                    }

                    string outputCollection = line.Substring(0, separatorIndex).Trim();
                    string outputTemplate = line.Substring(separatorIndex + 1).Trim();
                    OutputItem outputItem = new OutputItem(definition, lineNumber, outputCollection, outputTemplate);

                    outputItems.Add(outputItem);
                }
                lineNumber++;
            }

            lineDefinitions.Add(definition);
        }

    }
}


