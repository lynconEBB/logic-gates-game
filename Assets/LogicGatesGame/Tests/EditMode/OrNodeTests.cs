using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class OrNodeTests
    {
        [Test]
        public void ExecEvaluation_NoInputs_ReturnsNull()
        {
            var node = new OrNode(1);
            Assert.IsNull(node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_SingleFalseInput_ReturnsFalse()
        {
            var src = new SourceNode(1);
            src.setValue(false);
            var node = new OrNode(2);
            node.TryAddInput(src);

            Assert.AreEqual(false, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_AllFalse_ReturnsFalse()
        {
            var src1 = new SourceNode(1);
            src1.setValue(false);
            var src2 = new SourceNode(2);
            src2.setValue(false);
            var node = new OrNode(3);
            node.TryAddInput(src1);
            node.TryAddInput(src2);

            Assert.AreEqual(false, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_AnyTrue_ReturnsTrue()
        {
            var src1 = new SourceNode(1);
            src1.setValue(false);
            var src2 = new SourceNode(2);
            src2.setValue(true);
            var node = new OrNode(3);
            node.TryAddInput(src1);
            node.TryAddInput(src2);

            Assert.AreEqual(true, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_AnyNull_ReturnsNull()
        {
            var src1 = new SourceNode(1);
            src1.setValue(false);
            var src2 = new SourceNode(2); // value not set → null
            var node = new OrNode(3);
            node.TryAddInput(src1);
            node.TryAddInput(src2);

            Assert.IsNull(node.ExecEvaluation());
        }
    }
}
