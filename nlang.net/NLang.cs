using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nlang.net
{
    public class NLang
    {
        private Evaluator _evaluator = new Evaluator();

        private Parser _parser = new Parser();

        private Tokenizer _tokenizer = new Tokenizer();


        public NLang(Dictionary<string, object> additionalContext = null)
        {
            if (null != additionalContext)
            {
                foreach (KeyValuePair<string, object> pair in additionalContext)
                {
                    _evaluator.Env.CurContext.Set(pair.Key, pair.Value);
                }
            }
        }

        public object Eval(string script)
        {
            return _evaluator.Evaluate(_parser.Parse(_tokenizer.Tokenize(script)));
        }
    }
}
