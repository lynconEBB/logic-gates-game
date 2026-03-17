using System.Collections.Generic;
using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class ExpressionEvaluatorTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static bool Eval(string expr, Dictionary<string, bool> assignment)
            => ExpressionEvaluator.Parse(expr).Evaluate(assignment);

        // ── Happy path ────────────────────────────────────────────────────────

        [Test]
        public void SingleVariable_True()
        {
            Assert.IsTrue(Eval("A", new Dictionary<string, bool> { ["A"] = true }));
        }

        [Test]
        public void SingleVariable_False()
        {
            Assert.IsFalse(Eval("A", new Dictionary<string, bool> { ["A"] = false }));
        }

        [Test]
        public void Not_InvertsTrue()
        {
            Assert.IsFalse(Eval("!A", new Dictionary<string, bool> { ["A"] = true }));
        }

        [Test]
        public void Not_InvertsFalse()
        {
            Assert.IsTrue(Eval("!A", new Dictionary<string, bool> { ["A"] = false }));
        }

        [Test]
        public void And_BothTrue_ReturnsTrue()
        {
            Assert.IsTrue(Eval("A * B", new Dictionary<string, bool> { ["A"] = true, ["B"] = true }));
        }

        [Test]
        public void And_OneFalse_ReturnsFalse()
        {
            Assert.IsFalse(Eval("A * B", new Dictionary<string, bool> { ["A"] = true, ["B"] = false }));
        }

        [Test]
        public void Or_OneFalseOneTrue_ReturnsTrue()
        {
            Assert.IsTrue(Eval("A + B", new Dictionary<string, bool> { ["A"] = false, ["B"] = true }));
        }

        [Test]
        public void Or_BothFalse_ReturnsFalse()
        {
            Assert.IsFalse(Eval("A + B", new Dictionary<string, bool> { ["A"] = false, ["B"] = false }));
        }

        [Test]
        public void DoubleNot_ReturnsSameValue()
        {
            Assert.IsTrue(Eval("!!A",  new Dictionary<string, bool> { ["A"] = true  }));
            Assert.IsFalse(Eval("!!A", new Dictionary<string, bool> { ["A"] = false }));
        }

        // ── Precedence ────────────────────────────────────────────────────────

        [Test]
        public void Not_BindsTighterThan_And()
        {
            // !A * B  →  (!A) * B
            Assert.IsTrue( Eval("!A * B", new Dictionary<string, bool> { ["A"] = false, ["B"] = true  }));
            Assert.IsFalse(Eval("!A * B", new Dictionary<string, bool> { ["A"] = true,  ["B"] = true  }));
        }

        [Test]
        public void And_BindsTighterThan_Or()
        {
            // A + B * C  →  A + (B * C)
            // A=false, B=true, C=true  → false + true = true
            Assert.IsTrue( Eval("A + B * C", new Dictionary<string, bool> { ["A"] = false, ["B"] = true,  ["C"] = true  }));
            // A=false, B=true, C=false → false + false = false
            Assert.IsFalse(Eval("A + B * C", new Dictionary<string, bool> { ["A"] = false, ["B"] = true,  ["C"] = false }));
        }

        [Test]
        public void Parens_OverridePrecedence()
        {
            // (A + B) * C  →  (A + B) * C
            // A=false, B=true, C=true → true * true = true
            Assert.IsTrue( Eval("(A + B) * C", new Dictionary<string, bool> { ["A"] = false, ["B"] = true, ["C"] = true  }));
            // A=false, B=false, C=true → false * true = false
            Assert.IsFalse(Eval("(A + B) * C", new Dictionary<string, bool> { ["A"] = false, ["B"] = false, ["C"] = true }));
        }

        // ── Variables property ────────────────────────────────────────────────

        [Test]
        public void Variables_OrderOfFirstAppearance()
        {
            var ev = ExpressionEvaluator.Parse("B + A");
            Assert.AreEqual(new[] { "B", "A" }, ev.Variables);
        }

        [Test]
        public void Variables_Deduplicated()
        {
            var ev = ExpressionEvaluator.Parse("A * B + !A");
            Assert.AreEqual(new[] { "A", "B" }, ev.Variables);
        }

        [Test]
        public void Variables_MultiCharNames()
        {
            var ev = ExpressionEvaluator.Parse("foo * bar");
            Assert.AreEqual(new[] { "foo", "bar" }, ev.Variables);
        }

        // ── Parse errors ──────────────────────────────────────────────────────

        [Test]
        public void ParseError_EmptyString()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse(""));
        }

        [Test]
        public void ParseError_WhitespaceOnly()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse("   "));
        }

        [Test]
        public void ParseError_TrailingOperator()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse("A +"));
        }

        [Test]
        public void ParseError_LeadingBinaryOperator()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse("+ A"));
        }

        [Test]
        public void ParseError_UnclosedParen()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse("(A + B"));
        }

        [Test]
        public void ParseError_ExtraClosingParen()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse("A + B)"));
        }

        [Test]
        public void ParseError_InvalidCharacter()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse("A & B"));
        }

        [Test]
        public void ParseError_TwoIdentifiersNoOperator()
        {
            Assert.Throws<ExpressionParseException>(() => ExpressionEvaluator.Parse("A B"));
        }

        // ── Truth table integration ───────────────────────────────────────────

        private static (SourceNode a, SourceNode b, SinkNode sink) BuildAndCircuit()
        {
            var srcA = new SourceNode(1);
            var srcB = new SourceNode(2);
            var and  = new AndNode(3);
            var sink = new SinkNode(4);

            and.TryAddInput(srcA);
            and.TryAddInput(srcB);
            srcA.TryAddOutput(and);
            srcB.TryAddOutput(and);

            sink.TryAddInput(and);
            and.TryAddOutput(sink);

            return (srcA, srcB, sink);
        }

        [Test]
        public void TruthTable_AndCircuit_MatchesAndExpression()
        {
            var (srcA, srcB, sink) = BuildAndCircuit();
            var ev = ExpressionEvaluator.Parse("A * B");
            var vars = new bool[2];

            for (int combo = 0; combo < 4; combo++)
            {
                bool a = (combo & 1) != 0;
                bool b = (combo >> 1 & 1) != 0;
                srcA.setValue(a);
                srcB.setValue(b);
                vars[0] = a;
                vars[1] = b;

                bool circuitResult = sink.ExecEvaluation() ?? false;
                bool exprResult    = ev.Evaluate(vars);

                Assert.AreEqual(exprResult, circuitResult,
                    $"Mismatch at A={a}, B={b}");
            }
        }

        [Test]
        public void TruthTable_OrNotCircuit_MatchesOrNotExpression()
        {
            // !A + B
            var srcA = new SourceNode(1);
            var srcB = new SourceNode(2);
            var not  = new NotNode(3);
            var or   = new OrNode(4);
            var sink = new SinkNode(5);

            not.TryAddInput(srcA);  srcA.TryAddOutput(not);
            or.TryAddInput(not);    not.TryAddOutput(or);
            or.TryAddInput(srcB);   srcB.TryAddOutput(or);
            sink.TryAddInput(or);   or.TryAddOutput(sink);

            var ev   = ExpressionEvaluator.Parse("!A + B");
            var vars = new bool[2];

            for (int combo = 0; combo < 4; combo++)
            {
                bool a = (combo & 1) != 0;
                bool b = (combo >> 1 & 1) != 0;
                srcA.setValue(a);
                srcB.setValue(b);
                vars[0] = a;
                vars[1] = b;

                bool circuitResult = sink.ExecEvaluation() ?? false;
                bool exprResult    = ev.Evaluate(vars);

                Assert.AreEqual(exprResult, circuitResult,
                    $"Mismatch at A={a}, B={b}");
            }
        }

        [Test]
        public void TruthTable_AndCircuit_DoesNotMatch_OrExpression()
        {
            var (srcA, srcB, sink) = BuildAndCircuit();
            var ev = ExpressionEvaluator.Parse("A + B");

            bool mismatch = false;
            for (int combo = 0; combo < 4 && !mismatch; combo++)
            {
                bool a = (combo & 1) != 0;
                bool b = (combo >> 1 & 1) != 0;
                srcA.setValue(a);
                srcB.setValue(b);

                bool circuitResult = sink.ExecEvaluation() ?? false;
                bool exprResult    = ev.Evaluate(new bool[] { a, b });
                if (circuitResult != exprResult) mismatch = true;
            }
            Assert.IsTrue(mismatch, "AND circuit should NOT match OR expression.");
        }

        [Test]
        public void TruthTable_DisconnectedSink_ReturnsNull()
        {
            var sink = new SinkNode(1);
            Assert.IsNull(sink.ExecEvaluation());
        }

        [Test]
        public void TruthTable_SourceValuesRestoredAfterWalk()
        {
            var (srcA, srcB, sink) = BuildAndCircuit();
            srcA.setValue(true);
            srcB.value = null;   // simulate undefined initial state

            bool?[] buffer = new bool?[] { srcA.value, srcB.value };

            // simulate truth-table walk
            for (int combo = 0; combo < 4; combo++)
            {
                srcA.setValue((combo & 1) != 0);
                srcB.setValue((combo >> 1 & 1) != 0);
                _ = sink.ExecEvaluation();
            }

            // restore
            srcA.value = buffer[0];
            srcB.value = buffer[1];

            Assert.AreEqual(true, srcA.value,  "srcA should be restored to true");
            Assert.IsNull(srcB.value,           "srcB should be restored to null");
        }
    }
}
