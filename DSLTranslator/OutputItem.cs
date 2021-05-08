using System;

namespace DSLTranslator
{
    /// <summary>
    /// Represents one line from a definition file eg
    /// "join : JOIN X on Y"
    /// </summary>
    public class OutputItem
    {
        private LineDefinition lineDefinition;
        private int sourceTemplateLineNumber;
        private string outputCollection;
        private string outputPattern;

        public string OutputPattern { get { return outputPattern; } }
        public string CollectionName { get { return outputCollection; } }

        public OutputItem(
            LineDefinition lineDefinition,
            int sourceTemplateLineNumber,
            string outputCollection,
            string outputPattern)
        {
            this.lineDefinition = lineDefinition;
            this.sourceTemplateLineNumber = sourceTemplateLineNumber;
            this.outputCollection = outputCollection;
            this.outputPattern = outputPattern;
        }
    }
}
