using System;
namespace DSLTranslator
{
    public class OutputFragment
    {
        private string renderedOutput;

        public string RenderedOutput { get { return renderedOutput; } }

        public OutputFragment(string renderedOutput)
        {
            this.renderedOutput = renderedOutput;
        }
    }
}
