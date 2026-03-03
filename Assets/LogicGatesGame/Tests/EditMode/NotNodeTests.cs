using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class NotNodeTests
    {
        [Test]
        public void ExecEvaluation_NoInput_ReturnsNull()
        {
            var node = new NotNode(1);
            Assert.IsNull(node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_InvertsTrue_ReturnsFalse()
        {
            var src = new SourceNode(1);
            src.setValue(true);
            var node = new NotNode(2);
            node.TryAddInput(src);

            Assert.AreEqual(false, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_InvertsFalse_ReturnsTrue()
        {
            var src = new SourceNode(1);
            src.setValue(false);
            var node = new NotNode(2);
            node.TryAddInput(src);

            Assert.AreEqual(true, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_NullInput_ReturnsNull()
        {
            var src = new SourceNode(1); // value not set → null
            var node = new NotNode(2);
            node.TryAddInput(src);

            Assert.IsNull(node.ExecEvaluation());
        }

        [Test]
        public void TryAddInput_SecondInputRejected()
        {
            var src1 = new SourceNode(1);
            var src2 = new SourceNode(2);
            var node = new NotNode(3);

            Assert.IsTrue(node.TryAddInput(src1));
            Assert.IsFalse(node.TryAddInput(src2));
        }
    }
}
