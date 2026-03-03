using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class NodeBaseTests
    {
        [Test]
        public void TryAddInput_RespectsMaxInputs()
        {
            var node = new NotNode(1);
            var src1 = new SourceNode(2);
            var src2 = new SourceNode(3);

            Assert.IsTrue(node.TryAddInput(src1));
            Assert.IsFalse(node.TryAddInput(src2));
        }

        [Test]
        public void TryAddOutput_RejectsWhenMaxOutputsIsZero()
        {
            var sink = new SinkNode(1);
            var src = new SourceNode(2);

            Assert.IsFalse(sink.TryAddOutput(src));
        }

        [Test]
        public void TryAddOutput_AcceptsUnlimitedWhenMaxOutputsIsNull()
        {
            var src = new SourceNode(1);
            var sink1 = new SinkNode(2);
            var sink2 = new SinkNode(3);
            var sink3 = new SinkNode(4);

            Assert.IsTrue(src.TryAddOutput(sink1));
            Assert.IsTrue(src.TryAddOutput(sink2));
            Assert.IsTrue(src.TryAddOutput(sink3));
        }

        [Test]
        public void Evaluate_FiresOnEvaluatedEvent()
        {
            var src = new SourceNode(1);
            src.setValue(true);

            bool? received = null;
            src.OnEvaluated += val => received = val;

            src.Evaluate();

            Assert.AreEqual(true, received);
        }

        [Test]
        public void Evaluate_FiresOnEvaluatedEvent_WithNullWhenUndefined()
        {
            var src = new SourceNode(1);

            bool? received = false;
            src.OnEvaluated += val => received = val;

            src.Evaluate();

            Assert.IsNull(received);
        }
    }
}
