using NUnit.Framework;
using System;

namespace nlang.net.test
{
    public class ParserTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test__Parse__1()
        {
            var tn = new Tokenizer();
            var tokens = tn.Tokenize("(print (+ var1 123))");
            var ps = new Parser();
            var ast = ps.Parse(tokens);

            Action<object, TokenClass, string> assertToken = (obj, cls, text) =>
            {
                var token = (Token)obj;
                Assert.That(token.Cls, Is.EqualTo(cls));
                Assert.That(token.Text, Is.EqualTo(text));
            };

            Action<object, int> assertNodeChildrenCount = (obj, count) =>
            {
                var node = (Node)obj;
                Assert.That(node.Children.Count, Is.EqualTo(count));
            };

            var root = ast;
            assertNodeChildrenCount(root, 2);
            assertToken(root.Children[0], TokenClass.SYM, "progn");

            var rootCdr = (Node)root.Children[1];
            assertNodeChildrenCount(rootCdr, 2);
            assertToken(rootCdr.Children[0], TokenClass.SYM, "print");

            var rootCddr = (Node)rootCdr.Children[1];
            assertNodeChildrenCount(rootCddr, 3);
            assertToken(rootCddr.Children[0], TokenClass.SYM, "+");
            assertToken(rootCddr.Children[1], TokenClass.SYM, "var1");
            assertToken(rootCddr.Children[2], TokenClass.NUM, "123");
        }
    }
}