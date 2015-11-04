using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using disruptorpretest.Support ;
using Disruptor.Scheduler;
using NUnit.Framework;
using Disruptor;
using Disruptor.Dsl;

namespace disruptorpretest.Pipeline3StepLatency
{
    [TestFixture]
    public sealed class Pipeline3StepLatencyDisruptorWithAffinityPerfTest : AbstractPipeline3StepLatencyPerfTest
    {
        private readonly RingBuffer<ValueEvent> _ringBuffer;

        private readonly LatencyStepEventHandler _stepOneFunctionEventHandler;
        private readonly LatencyStepEventHandler _stepTwoFunctionEventHandler;
        private readonly LatencyStepEventHandler _stepThreeFunctionEventHandler;
        private readonly Disruptor<ValueEvent> _disruptor;
        private readonly ManualResetEvent _mru;
        private readonly RoundRobinThreadAffinedTaskScheduler _scheduler;

        public Pipeline3StepLatencyDisruptorWithAffinityPerfTest()
            : base(2 * Million)
        {
            _scheduler = new RoundRobinThreadAffinedTaskScheduler(4);

            _disruptor = new Disruptor<ValueEvent>(() => new ValueEvent(), Size, _scheduler, ProducerType.SINGLE, new YieldingWaitStrategy());

            _mru = new ManualResetEvent(false);
            _stepOneFunctionEventHandler = new LatencyStepEventHandler(FunctionStep.One, Histogram, StopwatchTimestampCostInNano, TicksToNanos, Iterations, _mru);
            _stepTwoFunctionEventHandler = new LatencyStepEventHandler(FunctionStep.Two, Histogram, StopwatchTimestampCostInNano, TicksToNanos, Iterations, _mru);
            _stepThreeFunctionEventHandler = new LatencyStepEventHandler(FunctionStep.Three, Histogram, StopwatchTimestampCostInNano, TicksToNanos, Iterations, _mru);

            _disruptor.HandleEventsWith(_stepOneFunctionEventHandler)
                .Then(_stepTwoFunctionEventHandler)
                .Then(_stepThreeFunctionEventHandler);
            _ringBuffer = _disruptor.GetRingBuffer;
        }

        public override void RunPass()
        {
            _disruptor.Start();

            Task.Factory.StartNew(
                () =>
                {
                    for (long i = 0; i < Iterations; i++)
                    {
                        var sequence = _ringBuffer.Next();
                        _ringBuffer[sequence].Value = Stopwatch.GetTimestamp();
                        _ringBuffer.Publish(sequence);

                        var pauseStart = Stopwatch.GetTimestamp();
                        while (PauseNanos > (Stopwatch.GetTimestamp() - pauseStart) * TicksToNanos)
                        {
                            // busy spin
                        }
                    }
                }, CancellationToken.None, TaskCreationOptions.None, _scheduler);

            _mru.WaitOne();

            _disruptor.Shutdown();
            _scheduler.Dispose();
        }

        [Test]
        public override void RunPerformanceTest()
        {
            RunAsUnitTest();
        }
    }
}