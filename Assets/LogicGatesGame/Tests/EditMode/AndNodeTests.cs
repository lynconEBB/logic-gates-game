using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class AndNodeTests
    {
        [Test]
        public void ExecEvaluation_NoInputs_ReturnsNull()
        {
            var node = new AndNode(1);
            Assert.IsNull(node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_SingleTrueInput_ReturnsTrue()
        {
            var src = new SourceNode(1);
            src.setValue(true);
            var node = new AndNode(2);
            node.TryAddInput(src);

            Assert.AreEqual(true, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_AllTrue_ReturnsTrue()
        {
            var src1 = new SourceNode(1);
            src1.setValue(true);
            var src2 = new SourceNode(2);
            src2.setValue(true);
            var node = new AndNode(3);
            node.TryAddInput(src1);
            node.TryAddInput(src2);

            Assert.AreEqual(true, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_AnyFalse_ReturnsFalse()
        {
            var src1 = new SourceNode(1);
            src1.setValue(true);
            var src2 = new SourceNode(2);
            src2.setValue(false);
            var node = new AndNode(3);
            node.TryAddInput(src1);
            node.TryAddInput(src2);

            Assert.AreEqual(false, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_AnyNull_ReturnsNull()
        {
            var src1 = new SourceNode(1);
            src1.setValue(true);
            var src2 = new SourceNode(2); // value not set → null
            var node = new AndNode(3);
            node.TryAddInput(src1);
            node.TryAddInput(src2);

            Assert.IsNull(node.ExecEvaluation());
        }
    }
}
