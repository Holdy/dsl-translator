using System;
namespace DSLTranslator
{
    public class ScaffoldMatchingStep : IMatchingStep
    {
        private string text;

        public string ScaffoldText
        {
            get { return text; }
        }
        public ScaffoldMatchingStep(string text)
        {
            this.text = text;
        }
    }
}
