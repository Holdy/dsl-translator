using System;
using System.Collections.Generic;
using System.Text;

namespace DSLTranslator
{

    public class DSLSqlFormatter
    {
        private const string LINE_SEPARATOR = "\n";

        public DSLSqlFormatter()
        {
        }

        public static string Format(DSLTranslation translation)
        {
            StringBuilder builder = new StringBuilder();

            ApplyFrom(translation, builder);
            ApplyJoin(translation, builder);
            ApplyWhere(translation, builder);

            return builder.ToString();
        }

        private static void ApplyFrom(DSLTranslation translation, StringBuilder builder)
        {
            IList<OutputFragment> fromCollection = translation.GetCollection("from");
            if (fromCollection.Count != 1)
            {
                throw new DSLTemplateException($"Currently, only one 'initial' item is allowed.");
            }

            builder.Append(fromCollection[0].RenderedOutput);
        }

        private static void ApplyJoin(DSLTranslation translation, StringBuilder builder)
        {
            IList<OutputFragment> joinCollection = translation.GetCollection("join");
            if (joinCollection.Count > 0)
            {
                builder.Append(LINE_SEPARATOR);
                // builder.Append("  -------- JOINs ---------");
                foreach(OutputFragment fragment in joinCollection)
                {
                    builder.Append(LINE_SEPARATOR);
                    builder.Append(fragment.RenderedOutput);
                }
            }
        }

        private static void ApplyWhere(DSLTranslation translation, StringBuilder builder)
        {
            IList<OutputFragment> joinCollection = translation.GetCollection("where");
            if (joinCollection.Count > 0)
            {
                builder.Append(LINE_SEPARATOR);
                // builder.Append("  -------- WHERE ---------");
                int item_count = 0;

                foreach (OutputFragment fragment in joinCollection)
                {
                    builder.Append(LINE_SEPARATOR);
                    if (item_count == 0)
                    {
                        builder.Append(" WHERE ");
                    }
                    else
                    {
                        builder.Append("   AND ");
                    }
                    builder.Append(fragment.RenderedOutput);
                    item_count++;
                }
            }
        }
    }
}
