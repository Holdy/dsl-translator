using System.Collections.Generic;

namespace DSLTranslator {

    public class DSLTranslation {

        private Dictionary<string, IList<OutputFragment>> outputMap = new Dictionary<string, IList<OutputFragment>>();

        private IList<OutputFragment> EnsureCollection(string collectionName) {

            if (!outputMap.ContainsKey(collectionName))
            {
                outputMap[collectionName] = new List<OutputFragment>();
            }

            return outputMap[collectionName];
        }

        public IList<OutputFragment> GetCollection(string collectionName)
        {
            return EnsureCollection(collectionName); 
        }

        public void AddToCollection(string collectionName, OutputFragment line) {
            IList<OutputFragment> lines = EnsureCollection(collectionName);


            OutputFragment existingFragment = null;
            foreach(OutputFragment testFragment in lines)
            {
                if (testFragment.RenderedOutput.Equals(line.RenderedOutput))
                {
                    existingFragment = testFragment;
                }
            }
            if (existingFragment == null)
            {
                lines.Add(line);
            }
            // if line not already present
        }

    }

}