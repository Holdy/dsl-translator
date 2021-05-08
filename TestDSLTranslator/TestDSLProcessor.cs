using System;
using Xunit;
using DSLTranslator;

namespace TestDSLTranslator
{
    public class TestDSLProcessor
    {
        [Fact]
        public void TestLoadingOfModel()
        {
            DSLProcessor sut = new DSLProcessor();
            sut.LoadLineDefinitionsFromDirectory("../../../valid-language");
        
            String input = @"
                each active customer (cust) where
                (cust) placed an order (order_1)
                (order_1) total was between £10 and £100
            ";

            DSLTranslation translation = sut.Process(input);

            // test translation bits

            string sql = DSLSqlFormatter.Format(translation);
            Console.WriteLine(sql);
        }
    }
}
