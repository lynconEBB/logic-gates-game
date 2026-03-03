using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class SimpleNodeTests
    {
        [Test]
        public void ExecEvaluation_NoInputs_ReturnsNull()
        {
            var node = new SimpleNode(1);
            Assert.IsNull(node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_PassesThroughTrue()
        {
            var src = new SourceNode(1);
            src.setValue(true);
            var node = new SimpleNode(2);
            node.TryAddInput(src);

            Assert.AreEqual(true, node.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_PassesThroughFalse()
        {
            var src = new SourceNode(1);
            src.setValue(false);
            var node = new SimpleNode(2);
            node.TryAddInput(src);

            Assert.AreEqual(false, node.ExecEvaluation());
        }
    }
}
