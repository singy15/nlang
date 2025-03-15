using NUnit.Framework;
using System;

namespace nlang.net.test
{
    public class NLangTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test__Eval__1()
        {
            var nlang = new NLang();

            var ac1 = nlang.Eval(@"(+ 1 2)");
            Assert.That(ac1, Is.EqualTo(Convert.ToDecimal(3)));

            var ac2 = nlang.Eval(@"
(let helloworld (fn (x) (print (& 'hello, ' x))))
(helloworld 'user1')
");

            var ac3 = nlang.Eval(@"
(let fact (fn (k n)
    (for ((i 1 m k) (< i n) (++ i))
        (= m (* m k)))))
(fact 2 3)
");
            Assert.That(ac3, Is.EqualTo(Convert.ToDecimal(8)));

            var ac4 = nlang.Eval(@"
(let fact2 (fn (k n) (if (== n 0) 1 (* k (fact2 k (- n 1))))))
(fact2 3 3)
");
            Assert.That(ac4, Is.EqualTo(Convert.ToDecimal(27)));

        }
    }
}