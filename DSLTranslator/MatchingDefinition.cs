using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DSLTranslator
{
    /// <summary>
    /// models a matching definition for an input line
    /// and captures any placeholders.
    /// </summary>
    public class MatchingDefinition
    {
        private List<String> placeholderValues = new List<String>();
        private LineDefinition lineDefinition;

        public MatchingDefinition(LineDefinition lineDefinition)
        {
            this.lineDefinition = lineDefinition;
        }

        public IEnumerable<Placeholder> Placeholders { get { return lineDefinition.Placeholders; } }

        public string ErrorMessage { get; internal set; }

        public void AddPlaceholderValue(string value)
        {
            placeholderValues.Add(value);
        }

        internal string GetPlaceholderValue(int placeholderIndex)
        {
            return placeholderValues[placeholderIndex];
        }

        internal void RenderInto(DSLTranslation result)
        {
            foreach(OutputItem outputItem in lineDefinition.OutputItems)
            {
                String workingOutput = outputItem.OutputPattern;

                int placeholderIndex = 0;
                foreach(String placeholderValue in placeholderValues)
                {
                    String placeholderPattern = $"\\({placeholderIndex}\\)";
                    workingOutput = Regex.Replace(workingOutput, placeholderPattern, placeholderValue);
                    placeholderIndex++;
                }

                result.AddToCollection(outputItem.CollectionName, new OutputFragment(workingOutput));
            }
        }
    }
}
