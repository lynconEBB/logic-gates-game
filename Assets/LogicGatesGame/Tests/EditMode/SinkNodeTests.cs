using NUnit.Framework;
using LogicGatesGame.Scripts;

namespace LogicGatesGame.Tests
{
    [TestFixture]
    public class SinkNodeTests
    {
        [Test]
        public void ExecEvaluation_NoInputs_ReturnsNull()
        {
            var sink = new SinkNode(1);
            Assert.IsNull(sink.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_PropagatesToInput_True()
        {
            var src = new SourceNode(1);
            src.setValue(true);
            var sink = new SinkNode(2);
            sink.TryAddInput(src);

            Assert.AreEqual(true, sink.ExecEvaluation());
        }

        [Test]
        public void ExecEvaluation_PropagatesToInput_False()
        {
            var src = new SourceNode(1);
            src.setValue(false);
            var sink = new SinkNode(2);
            sink.TryAddInput(src);

            Assert.AreEqual(false, sink.ExecEvaluation());
        }

        [Test]
        public void TryAddInput_SecondInputRejected()
        {
            var sink = new SinkNode(1);
            var src1 = new SourceNode(2);
            var src2 = new SourceNode(3);

            Assert.IsTrue(sink.TryAddInput(src1));
            Assert.IsFalse(sink.TryAddInput(src2));
        }

        [Test]
        public void TryAddOutput_AlwaysReturnsFalse()
        {
            var sink = new SinkNode(1);
            var other = new SinkNode(2);
            Assert.IsFalse(sink.TryAddOutput(other));
        }
    }
}
