using System;
namespace DSLTranslator
{
    public class Placeholder : IMatchingStep
    {
        private string typeName;

        private int index;
        private bool isAlias;
        private bool isReference;
        private bool isLiteral;

        public string TypeName { get { return typeName; } }
        public bool IsAlias { get { return isAlias; } }
        public bool IsReference { get { return isReference; } }

        public bool IsLiteral { get { return isLiteral;  } }

        public Placeholder(int index, string typeName, bool alias, bool reference, bool literal)
        {
            this.index = index;
            this.typeName = typeName;
            this.isAlias = alias;
            this.isReference = reference;
            this.isLiteral = literal;
        }
    }
}
