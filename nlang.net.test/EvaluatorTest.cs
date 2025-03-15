using NUnit.Framework;
using System;

namespace nlang.net.test
{
    public class EvaluatorTest
    {
        [SetUp]
        public void Setup()
        {
        }

        private object EvaluateScript(string script)
        {
            return (new Evaluator()).Evaluate((new Parser()).Parse((new Tokenizer()).Tokenize(script)));
        }

        [Test]
        public void Test__Evaluate__1()
        {
            Assert.That(EvaluateScript("(+ 2 3)"), Is.EqualTo(Convert.ToDecimal(5)));
        }

        [Test]
        public void Test__Evaluate__ArithmeticOperators()
        {
            Assert.That(EvaluateScript("(+ 2 3)"), Is.EqualTo(Convert.ToDecimal(5)));
            Assert.That(EvaluateScript("(- 2 3)"), Is.EqualTo(Convert.ToDecimal(-1)));
            Assert.That(EvaluateScript("(/ 3 2)"), Is.EqualTo(Convert.ToDecimal(1.5)));
            Assert.That(EvaluateScript("(* 3 2)"), Is.EqualTo(Convert.ToDecimal(6)));
            Assert.That(EvaluateScript("(% 4 3)"), Is.EqualTo(Convert.ToDecimal(1)));
            Assert.That(EvaluateScript("(** 2 3)"), Is.EqualTo(Convert.ToDecimal(8)));
        }

        [Test]
        public void Test__Evaluate__CompareOperators()
        {
            Assert.That(EvaluateScript("(< 1 2)"), Is.EqualTo(true));
            Assert.That(EvaluateScript("(< 2 1)"), Is.EqualTo(false));

            Assert.That(EvaluateScript("(> 1 2)"), Is.EqualTo(false));
            Assert.That(EvaluateScript("(> 2 1)"), Is.EqualTo(true));

            Assert.That(EvaluateScript("(<= 1 2)"), Is.EqualTo(true));
            Assert.That(EvaluateScript("(<= 2 1)"), Is.EqualTo(false));
            Assert.That(EvaluateScript("(<= 2 2)"), Is.EqualTo(true));

            Assert.That(EvaluateScript("(>= 1 2)"), Is.EqualTo(false));
            Assert.That(EvaluateScript("(>= 2 1)"), Is.EqualTo(true));
            Assert.That(EvaluateScript("(>= 2 2)"), Is.EqualTo(true));

            Assert.That(EvaluateScript("(== 1 2)"), Is.EqualTo(false));
            Assert.That(EvaluateScript("(== 2 2)"), Is.EqualTo(true));

            Assert.That(EvaluateScript("(!= 1 2)"), Is.EqualTo(true));
            Assert.That(EvaluateScript("(!= 2 2)"), Is.EqualTo(false));
        }

        [Test]
        public void Test__Evaluate__SpecialForms()
        {
            Assert.That(EvaluateScript("(if (== 1 1) 'yes' 'no')"), Is.EqualTo("yes"));
            Assert.That(EvaluateScript("(if (== 1 2) 'yes' 'no')"), Is.EqualTo("no"));
            Assert.That(EvaluateScript("(if (== 1 2) 'yes')"), Is.EqualTo(Statics.Undefined));

            Assert.That(EvaluateScript("(progn (let x 1) x)"), Is.EqualTo(Convert.ToDecimal(1)));
            Assert.That(EvaluateScript("(progn (let x) (= x 1) x)"), Is.EqualTo(Convert.ToDecimal(1)));
            Assert.Throws<NLangException>(() =>
            {
                EvaluateScript("(progn (progn (let x) (= x 1) x) x)");
            });
            Assert.Throws<NLangException>(() =>
            {
                EvaluateScript("(progn (= x 1) x)");
            });
        }

        [Test]
        public void Test__Evaluate__SpecialForms__fn()
        {
            Assert.That(EvaluateScript("(progn (let f (fn (x) (+ x 1))) (f 1))"), Is.EqualTo(Convert.ToDecimal(2)));
            
            var src1 = @"
(progn
    (let f null)
    (progn
        (let y 2)
        (= f (fn (x) (* x y)))
        (print (f 2)))
    (let y 10)
    (= y 8)
    (print (f 3)))
";
            EvaluateScript(src1);
        }

    }
}