namespace DSLTranslator {

    public class ScopeItem {

        private string name;
        private string typeName;

        public string Name { get { return name; } }
        public string TypeName { get { return typeName; } }

        public ScopeItem(string name, string typeName)
        {
            this.name = name;
            this.typeName = typeName;
        }
    }

}