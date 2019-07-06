using System;
using System.Json;
using System.Collections.Generic;
using Xunit;

namespace JsonParserTests
{
    public class JsonParserTest
    {
        class JsonTestDataset
        {
            public readonly string m_original;
            public readonly string m_expected;
            public readonly Action<string, string> m_fx;


            public JsonTestDataset(string original) : this(original, original, (a, b) => Assert.Equal(a, b)) { }
            public JsonTestDataset(string original, string expected) : this(original, expected, (a, b) => Assert.Equal(a, b)) { }

            public JsonTestDataset(string original, string expected, Action<string, string> fx)
            {
                m_original = original;
                m_expected = expected;
                m_fx = fx;
            }

            public void Check(string actual)
            {
                m_fx(m_expected, actual);
            }
        };

        [Fact]
        public void JsonParserTestSuccess()
        {
            dynamic ret;
            List<JsonTestDataset> tests = new List<JsonTestDataset>();

            tests.Add(new JsonTestDataset("null"));
            tests.Add(new JsonTestDataset("true"));
            tests.Add(new JsonTestDataset("   false  ", "false"));
            tests.Add(new JsonTestDataset("2.99e8", "299000000"));
            tests.Add(new JsonTestDataset("1.602e-19"));
            tests.Add(new JsonTestDataset("1.602e+19"));
            tests.Add(new JsonTestDataset("2.9e01", "29"));
            tests.Add(new JsonTestDataset("0.000000000000000000001", "1e-21"));
            tests.Add(new JsonTestDataset("\"hoge\""));
            tests.Add(new JsonTestDataset("\"!@#\""));
            tests.Add(new JsonTestDataset(" [   1, 2 ]   ", "[1, 2]"));
            tests.Add(new JsonTestDataset("[ \"123\", 456]", "[\"123\", 456]"));
            tests.Add(new JsonTestDataset("{ \"hoge\": false }", "{\"hoge\": false}"));
            tests.Add(new JsonTestDataset("{\"a\":\"b\"}", "{\"a\": \"b\"}"));
            tests.Add(new JsonTestDataset("99999999999999999999", "1e+20"));


            foreach (JsonTestDataset test in tests)
            {
                ret = JsonParser.Parse(test.m_original);
                string retStr = JsonParser.ToJsonString(ret);
                Console.WriteLine(retStr);
                test.Check(retStr);
            }
        }
    }
}
