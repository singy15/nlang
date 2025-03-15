using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nlang.net
{

    public class Statics
    {
        public static readonly object Undefined = new object();
    }

    public class Context
    {
        public Dictionary<string, object> Contents { get; set; } = new Dictionary<string, object>();

        public Context() { }

        public void Set(string key, object val)
        {
            Contents[key] = val;
        }

        public static Context CreateBaseContext()
        {
            var ctx = new Context();

            ctx.Contents = new Dictionary<string, object>()
            {
                // Built-in constants
                ["true"] = true,
                ["false"] = false,
                ["undefined"] = Statics.Undefined,
                ["null"] = null,

                // Built-in functions
                ["print"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    Console.WriteLine(args[0]);
                    return Statics.Undefined;
                }),

                // Arithmetic Operators
                ["+"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return args.Skip(1).Aggregate(Convert.ToDecimal(args[0]), (m, x) => m + Convert.ToDecimal(x));
                }),
                ["-"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return args.Skip(1).Aggregate(Convert.ToDecimal(args[0]), (m, x) => m - Convert.ToDecimal(x));
                }),
                ["*"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return args.Skip(1).Aggregate(Convert.ToDecimal(args[0]), (m, x) => m * Convert.ToDecimal(x));
                }),
                ["/"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return args.Skip(1).Aggregate(Convert.ToDecimal(args[0]), (m, x) => m / Convert.ToDecimal(x));
                }),
                ["%"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return args.Skip(1).Aggregate(Convert.ToDecimal(args[0]), (m, x) => m % Convert.ToDecimal(x));
                }),
                ["**"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    var cnt = Convert.ToInt32(args[1]);
                    if (cnt == 0) return Convert.ToDecimal(0);

                    var num = Convert.ToDecimal(args[0]);
                    if (cnt == 1) return num;

                    var cur = num;
                    for (var i = 0; i < (cnt - 1); i++)
                    {
                        cur = cur * num;
                    }
                    return cur;
                }),

                // Compare Operators
                ["=="] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return args[0].Equals(args[1]);
                }),
                ["!="] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return !(args[0].Equals(args[1]));
                }),
                [">"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return Convert.ToDecimal(args[0]) > Convert.ToDecimal(args[1]);
                }),
                [">="] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return Convert.ToDecimal(args[0]) >= Convert.ToDecimal(args[1]);
                }),
                ["<"] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return Convert.ToDecimal(args[0]) < Convert.ToDecimal(args[1]);
                }),
                ["<="] = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    return Convert.ToDecimal(args[0]) <= Convert.ToDecimal(args[1]);
                }),

            };

            return ctx;
        }
    }

    public class EvalEnv
    {
        public List<Context> Contexts { get; set; } = new List<Context>();

        public Context CurContext { get { return Contexts[0]; } }

        public EvalEnv()
        {
            Contexts.Add(Context.CreateBaseContext());
        }
    }

    public class Evaluator
    {
        public EvalEnv Env { get; set; }

        public object Evaluate(object obj)
        {
            if (null == Env) Env = new EvalEnv();

            if (obj is Node)
            {
                return EvaluateNode((Node)obj);
            }
            else if (obj is Token)
            {
                return EvaluateToken((Token)obj);
            }
            else
            {
                throw new Exception($"Can't evaluate type { obj.GetType().FullName }");
            }
        }

        public object EvaluateNode(Node node)
        {
            var car = (Token)node.Children[0];
            var cdr = node.Children.Skip(1).ToList();

            if (car.Cls == TokenClass.SYM && car.Text == "progn")
            {
                /* SPECIAL FORM: progn */

                PushContext();
                object ret = Statics.Undefined;
                cdr.ForEach(obj =>
                {
                    ret = Evaluate(obj);
                });
                PopContext();
                return ret;
            }
            else if (car.Cls == TokenClass.SYM && car.Text == "if")
            {
                /* SPECIAL FORM: if */

                object cond = Evaluate(cdr[0]);
                if (!(cond is bool)) throw new NLangException(
                    $"Condition expression is not a boolean value");

                if ((bool)cond == true)
                {
                    return Evaluate(WrapProgn(cdr[1]));
                }
                else if (cdr.Count == 3)
                {
                    return Evaluate(WrapProgn(cdr[2]));
                }
                else
                {
                    return Statics.Undefined;
                }
            }
            else if (car.Cls == TokenClass.SYM && car.Text == "let")
            {
                /* SPECIAL FORM: let */

                var sym = (Token)cdr[0];
                Intern(sym.Text);
                if (cdr.Count == 2)
                {
                    Assign(sym.Text, Evaluate(cdr[1]));
                }
                return Statics.Undefined;
            }
            else if (car.Cls == TokenClass.SYM && car.Text == "=")
            {
                /* SPECIAL FORM: = */

                Assign(((Token)cdr[0]).Text, Evaluate(cdr[1]));
                return Statics.Undefined;
            }
            else if (car.Cls == TokenClass.SYM && car.Text == "fn")
            {
                /* SPECIAL FORM: fn */

                var prms = (Node)(cdr[0]);
                var body = (Node)(cdr[1]);
                var lexicalContext = Env.Contexts.Select(c => c).ToList();
                var fn = new Func<EvalEnv, List<object>, object>((env, args) =>
                {
                    var dynamicContext = Env.Contexts;
                    Env.Contexts = lexicalContext;
                    PushContext();
                    int i = 0;
                    prms.Children.ForEach((x) =>
                    {
                        var symbolName = ((Token)x).Text;
                        Intern(symbolName);
                        Assign(symbolName, args[i]);
                        i++;
                    });
                    var ret = Evaluate(body);
                    PopContext();
                    Env.Contexts = dynamicContext;
                    return ret;
                });
                return fn;
            }
            else
            {
                /* NORMAL FORM */

                return ((Func<EvalEnv, List<object>, object>)Evaluate(car))(
                    Env, cdr.Select(obj => Evaluate(obj)).ToList());
            }
        }

        private void Intern(string symbolName)
        {
            Env.CurContext.Set(symbolName, Statics.Undefined);
        }

        private object Assign(string symbolName, object val)
        {
            for (var i = 0; i < Env.Contexts.Count; i++)
            {
                var c = Env.Contexts[i];
                if (c.Contents.ContainsKey(symbolName))
                {
                    c.Set(symbolName, val);
                    return val;
                }
            }

            throw new NLangException($"Can't assign value, the symbol '{symbolName}' is not defined");
        }

        private Context PushContext()
        {
            var newCtx = new Context();
            Env.Contexts.Insert(0, newCtx);
            return newCtx;
        }

        private void PopContext()
        {
            Env.Contexts.RemoveAt(0);
        }

        private Node WrapProgn(object obj)
        {
            var node = new Node();
            node.Children.Add(new Token(TokenClass.SYM, "progn"));
            node.Children.Add(obj);
            return node;
        }

        public object EvaluateToken(Token token)
        {
            if (token.Cls == TokenClass.SYM)
            {
                return Resolve(token);
            }
            else if (token.Cls == TokenClass.NUM)
            {
                return Convert.ToDecimal(token.Text);
            }
            else if (token.Cls == TokenClass.STR)
            {
                return token.Text;
            }
            else if (token.Cls == TokenClass.QTE)
            {
                throw new Exception("Quote is not supported");
            }
            else
            {
                throw new Exception($"Unknown token class '{token.Cls.ToString()}'");
            }
        }

        public object Resolve(Token token)
        {
            if (token.Cls != TokenClass.SYM)
            {
                throw new Exception($"Can't resolve token '{token.Text}' type {token.Cls.ToString()}");
            }

            for (var i = 0; i < Env.Contexts.Count; i++)
            {
                var c = Env.Contexts[i];
                if (c.Contents.ContainsKey(token.Text)) return c.Contents[token.Text];
            }

            throw new NLangException($"Symbol '{token.Text}' is not defined");
        }
    }
}
