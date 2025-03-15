using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nlang.net
{
    public class NLangException : Exception
    {
        public NLangException(string message) : base(message)
        {
        }
    }
}
