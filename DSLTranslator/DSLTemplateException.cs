using System;
namespace DSLTranslator
{
    public class DSLTemplateException : Exception
    {
        public DSLTemplateException(String message)
            :base(message)
        {
        }
    }
}
