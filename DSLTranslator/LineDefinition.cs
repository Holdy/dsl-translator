using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DSLTranslator
{
    public class LineDefinition
    {

        private static string[] internalTypes = {"string", "number"};
        private static char[] placeholderSeparators = { '(', ' ', ')' };

        private string inputPattern;
        private IList<IMatchingStep> matchingSteps;
        private IList<Placeholder> placeholders = new List<Placeholder>();
        private IList<OutputItem> outputItems;

        public IList<Placeholder> Placeholders { get { return placeholders; } }
        public IEnumerable<IMatchingStep> MatchingSteps { get { return matchingSteps; } }

        public IEnumerable<OutputItem> OutputItems { get { return outputItems; } }

        public LineDefinition(string inputPattern, IList<OutputItem> sourceOutputItems)
        {
            this.inputPattern = inputPattern;
            this.outputItems = sourceOutputItems;

            UnpackPlaceholders(inputPattern);
        }

        private IList<IMatchingStep> BuildMatchingSteps(string input) {
            bool inPlaceholder = false;
            StringBuilder builder = new StringBuilder();
            IList<IMatchingStep> result = new List<IMatchingStep>();
            int placeholderIndex = 0;

            foreach (char c in input) {
                if (c == '(') {
                    if (inPlaceholder) {
                        throw new DSLTemplateException("Bracket '(' invalid in placeholder");
                    }
                    inPlaceholder = true;
                    if (builder.Length > 0) {
                        // must be a string 
                        result.Add(new ScaffoldMatchingStep(builder.ToString()));
                        builder.Length  = 0;
                    }
                    builder.Append(c);
                } 
                else if (c == ')') 
                {
                    if (!inPlaceholder) {
                        throw new DSLTemplateException("Bracket ')' invalid outside placeholder");
                    }
                    inPlaceholder = false;
                    builder.Append(c);
                    result.Add(ParsePlaceholderContent(builder.ToString(), placeholderIndex));
                    builder.Length = 0;
                    placeholderIndex++;
                } else // adding to the placeholder or scaffold text
                {
                    builder.Append(c);
                }
            }
            // input has ended.
            if (inPlaceholder)
            {
                throw new DSLTemplateException($"Line ended with an open placeholder: {builder.ToString()}");
            }
            if (builder.Length > 0)
            {
                result.Add(new ScaffoldMatchingStep(builder.ToString()));
            }
            return result;
        }

        // Takes "(customer alias)" etc
        private Placeholder ParsePlaceholderContent(string placeholderText, int index) {
            string[] pair = Formatting.SimplifySpacing(placeholderText).Split(placeholderSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (pair.Length != 2)
            {
                throw new DSLTemplateException($"Placeholders should contain two parts: {placeholderText}");
            }

            if (pair[1].Equals("alias")) {
                if (IsInternalType(pair[0])) {
                    throw new DSLTemplateException($"Cannot alias an internal type, as attempted in {placeholderText}");
                }
                return new Placeholder(index, pair[0], true, false, false);
            }
            else if (pair[1].Equals("reference"))
            {
                if (IsInternalType(pair[0])) {
                    throw new DSLTemplateException($"Cannot reference an internal type, as attempted in {placeholderText}");
                }
                return new Placeholder(index, pair[0], false, true, false);
            }
            else if (pair[1].Equals("literal"))
            {
                if (!IsInternalType(pair[0]))
                {
                    throw new DSLTemplateException($"Literal placeholder {placeholderText} must use an internal type (ie string / number) {pair[0]} is not valid here.");
                }
                return new Placeholder(index, pair[0], false, false, true);
            }
            else
            {
                throw new DSLTemplateException($"Placeholder {placeholderText} uses an unknown keyword '{pair[1]}' where alias, reference or literal are expected.");
            }

        }


        private void UnpackPlaceholders(string inputPattern) {
            MatchCollection templatePlaceholders = Formatting.FindPlaceholders(inputPattern);
            this.matchingSteps = BuildMatchingSteps(inputPattern);

            int index = 0;
            foreach(Match match in templatePlaceholders)
            {
                Placeholder placeholder = ParsePlaceholderContent(match.Value, index);
                placeholders.Add(placeholder);
                index++;
            }

        }

        private static bool IsInternalType(string typeName)
        {
            return Array.Exists(internalTypes, type => type.Equals(typeName));
        }

    }
}
