using System;
using System.Collections.Generic;

namespace LogicGatesGame.Scripts
{
    public class ExpressionParseException : Exception
    {
        public ExpressionParseException(string message) : base(message) { }
    }

    public class ExpressionEvaluator
    {
        // ── AST ──────────────────────────────────────────────────────────────

        private abstract class Expr { }
        private class VarExpr : Expr { public string Name; }
        private class NotExpr : Expr { public Expr Operand; }
        private class AndExpr : Expr { public Expr Left, Right; }
        private class OrExpr  : Expr { public Expr Left, Right; }

        // ── Tokenizer ─────────────────────────────────────────────────────────

        private enum TokenKind { Identifier, Not, And, Or, LParen, RParen, EOF }

        private struct Token
        {
            public TokenKind Kind;
            public string    Text;

            public Token(TokenKind kind, string text)
            {
                Kind = kind; 
                Text = text;
            }
        }

        private static List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            int i = 0;
            while (i < expression.Length)
            {
                char c = expression[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }

                if      (c == '!') { tokens.Add(new Token(TokenKind.Not,    "!")); i++; }
                else if (c == '*') { tokens.Add(new Token(TokenKind.And,    "*")); i++; }
                else if (c == '+') { tokens.Add(new Token(TokenKind.Or,     "+")); i++; }
                else if (c == '(') { tokens.Add(new Token(TokenKind.LParen, "(")); i++; }
                else if (c == ')') { tokens.Add(new Token(TokenKind.RParen, ")")); i++; }
                else if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                        i++;
                    tokens.Add(new Token(TokenKind.Identifier, expression.Substring(start, i - start)));
                }
                else
                {
                    throw new ExpressionParseException($"Invalid character '{c}' in expression.");
                }
            }
            tokens.Add(new Token(TokenKind.EOF, ""));
            return tokens;
        }

        // ── Parser ────────────────────────────────────────────────────────────

        private class Parser
        {
            private readonly List<Token> _tokens;
            private int _pos;
            private readonly List<string> _variables;

            public Token Current => _tokens[_pos];

            public Parser(List<Token> tokens, List<string> variables)
            {
                _tokens    = tokens;
                _pos       = 0;
                _variables = variables;
            }

            private Token Consume() => _tokens[_pos++];

            // expression = or_expr
            public Expr ParseExpression() => ParseOrExpr();

            // or_expr = and_expr ('+' and_expr)*
            private Expr ParseOrExpr()
            {
                Expr left = ParseAndExpr();
                while (Current.Kind == TokenKind.Or)
                {
                    Consume();
                    Expr right = ParseAndExpr();
                    left = new OrExpr { Left = left, Right = right };
                }
                return left;
            }

            // and_expr = not_expr ('*' not_expr)*
            private Expr ParseAndExpr()
            {
                Expr left = ParseNotExpr();
                while (Current.Kind == TokenKind.And)
                {
                    Consume();
                    Expr right = ParseNotExpr();
                    left = new AndExpr { Left = left, Right = right };
                }
                return left;
            }

            // not_expr = '!' not_expr | atom
            private Expr ParseNotExpr()
            {
                if (Current.Kind == TokenKind.Not)
                {
                    Consume();
                    return new NotExpr { Operand = ParseNotExpr() };
                }
                return ParseAtom();
            }

            // atom = '(' expression ')' | IDENTIFIER
            private Expr ParseAtom()
            {
                if (Current.Kind == TokenKind.LParen)
                {
                    Consume();
                    Expr inner = ParseExpression();
                    if (Current.Kind != TokenKind.RParen)
                        throw new ExpressionParseException("Expected closing ')'.");
                    Consume();
                    return inner;
                }

                if (Current.Kind == TokenKind.Identifier)
                {
                    string name = Current.Text;
                    Consume();
                    if (!_variables.Contains(name))
                        _variables.Add(name);
                    return new VarExpr { Name = name };
                }

                throw new ExpressionParseException(
                    $"Expected identifier or '(' but got '{Current.Text}'.");
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        private readonly Expr        _root;
        private readonly List<string> _variables;

        public IReadOnlyList<string> Variables => _variables;

        private ExpressionEvaluator(Expr root, List<string> variables)
        {
            _root      = root;
            _variables = variables;
        }

        public static ExpressionEvaluator Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ExpressionParseException("Expression cannot be empty.");

            List<Token>  tokens    = Tokenize(expression);
            var          variables = new List<string>();
            var          parser    = new Parser(tokens, variables);
            Expr         root      = parser.ParseExpression();

            if (parser.Current.Kind != TokenKind.EOF)
                throw new ExpressionParseException(
                    $"Unexpected token '{parser.Current.Text}' after expression.");

            return new ExpressionEvaluator(root, variables);
        }

        public bool Evaluate(IReadOnlyDictionary<string, bool> assignment)
            => EvalDict(_root, assignment);

        public bool Evaluate(bool[] values)
            => EvalPositional(_root, values);

        // ── Private eval ──────────────────────────────────────────────────────

        private static bool EvalDict(Expr expr, IReadOnlyDictionary<string, bool> assignment)
        {
            switch (expr)
            {
                case VarExpr v: return assignment[v.Name];
                case NotExpr n: return !EvalDict(n.Operand, assignment);
                case AndExpr a: return EvalDict(a.Left, assignment) && EvalDict(a.Right, assignment);
                case OrExpr  o: return EvalDict(o.Left, assignment) || EvalDict(o.Right, assignment);
                default:        throw new InvalidOperationException("Unknown expression node.");
            }
        }

        private bool EvalPositional(Expr expr, bool[] values)
        {
            switch (expr)
            {
                case VarExpr v: return values[_variables.IndexOf(v.Name)];
                case NotExpr n: return !EvalPositional(n.Operand, values);
                case AndExpr a: return EvalPositional(a.Left, values) && EvalPositional(a.Right, values);
                case OrExpr  o: return EvalPositional(o.Left, values) || EvalPositional(o.Right, values);
                default:        throw new InvalidOperationException("Unknown expression node.");
            }
        }
    }
}
