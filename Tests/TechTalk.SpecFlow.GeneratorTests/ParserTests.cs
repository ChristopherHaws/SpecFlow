using System;
using System.Globalization;
using System.IO;
using FluentAssertions;
using TechTalk.SpecFlow.Parser;
using Xunit;

namespace TechTalk.SpecFlow.GeneratorTests
{
    public class ParserTests
    {
        [Fact]
        public void Parser_handles_empty_feature_file_without_error()
        {
            var parser = new SpecFlowGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(""), null);

            act.ShouldNotThrow();
        }
    }
}
