using NUnit.Framework;

namespace nlang.net.test
{
    public class TokenizerTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test__Tokenize__1()
        {
            var tn = new Tokenizer();
            var ac1 = tn.Tokenize("(print 123 -123 0.12 -0.12\r\n\"str1\" \"st\\\"r2\" 'str3' 'st\\'r4')");
            Assert.That(ac1.Count, Is.EqualTo(11));
            Assert.That(ac1[0].Cls, Is.EqualTo(TokenClass.LP));
            Assert.That(ac1[1].Cls, Is.EqualTo(TokenClass.SYM));
            Assert.That(ac1[2].Cls, Is.EqualTo(TokenClass.NUM));
            Assert.That(ac1[3].Cls, Is.EqualTo(TokenClass.NUM));
            Assert.That(ac1[4].Cls, Is.EqualTo(TokenClass.NUM));
            Assert.That(ac1[5].Cls, Is.EqualTo(TokenClass.NUM));
            Assert.That(ac1[6].Cls, Is.EqualTo(TokenClass.STR));
            Assert.That(ac1[7].Cls, Is.EqualTo(TokenClass.STR));
            Assert.That(ac1[8].Cls, Is.EqualTo(TokenClass.STR));
            Assert.That(ac1[9].Cls, Is.EqualTo(TokenClass.STR));
            Assert.That(ac1[10].Cls, Is.EqualTo(TokenClass.RP));
        }
    }
}