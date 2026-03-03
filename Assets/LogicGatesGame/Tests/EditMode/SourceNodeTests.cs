using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class SourceNodeTests
    {
        [Test]
        public void ExecEvaluation_InitiallyReturnsNull()
        {
            var src = new SourceNode(1);
            Assert.IsNull(src.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_ReturnsTrueAfterSetValueTrue()
        {
            var src = new SourceNode(1);
            src.setValue(true);
            Assert.AreEqual(true, src.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_ReturnsFalseAfterSetValueFalse()
        {
            var src = new SourceNode(1);
            src.setValue(false);
            Assert.AreEqual(false, src.ExecEvaluation());
        }

        [Test]
        public void TryAddInput_AlwaysReturnsFalse()
        {
            var src = new SourceNode(1);
            var other = new SourceNode(2);
            Assert.IsFalse(src.TryAddInput(other));
        }
    }
}
