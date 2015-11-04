﻿using System.Collections.Generic;
using NUnit.Framework ;
using Moq;
using Disruptor;

namespace DisruptorGeneralTest
{
    [TestFixture]
    public class AggregateEventHandlerTests
    {
        private Mock<ILifecycleAwareEventHandler<IEnumerable<int>>> _eventHandlerMock1;
        private Mock<ILifecycleAwareEventHandler<IEnumerable<int>>> _eventHandlerMock2;
        private Mock<ILifecycleAwareEventHandler<IEnumerable<int>>> _eventHandlerMock3;

        [SetUp]
        public void SetUp()
        {
            _eventHandlerMock1 = new Mock<ILifecycleAwareEventHandler<IEnumerable<int>>>();
            _eventHandlerMock2 = new Mock<ILifecycleAwareEventHandler<IEnumerable<int>>>();
            _eventHandlerMock3 = new Mock<ILifecycleAwareEventHandler<IEnumerable<int>>>();
        }


        [Test]
        public void ShouldCallOnEvent()
        {
            var evt = new[] { 7 };
            const long sequence = 3L;
            const bool endOfBatch = true;

            var aggregateEventHandler = new AggregateEventHandler<IEnumerable<int>>(_eventHandlerMock1.Object,
                                                                         _eventHandlerMock2.Object,
                                                                         _eventHandlerMock3.Object);

            _eventHandlerMock1.Setup(eh => eh.OnEvent(evt, sequence, endOfBatch)).Verifiable("event handler 1 was not called");
            _eventHandlerMock2.Setup(eh => eh.OnEvent(evt, sequence, endOfBatch)).Verifiable("event handler 2 was not called");
            _eventHandlerMock3.Setup(eh => eh.OnEvent(evt, sequence, endOfBatch)).Verifiable("event handler 3 was not called");

            aggregateEventHandler.OnEvent(evt, sequence, endOfBatch);

            _eventHandlerMock1.Verify();
            _eventHandlerMock2.Verify();
            _eventHandlerMock3.Verify();
        }

        [Test]
        public void ShouldCallOnStart()
        {
            var aggregateEventHandler = new AggregateEventHandler<IEnumerable<int>>(_eventHandlerMock1.Object,
                                                                         _eventHandlerMock2.Object,
                                                                         _eventHandlerMock3.Object);

            _eventHandlerMock1.Setup(eh => eh.OnStart()).Verifiable("event handler 1 was not called");
            _eventHandlerMock2.Setup(eh => eh.OnStart()).Verifiable("event handler 2 was not called");
            _eventHandlerMock3.Setup(eh => eh.OnStart()).Verifiable("event handler 3 was not called");

            aggregateEventHandler.OnStart();

            _eventHandlerMock1.Verify();
            _eventHandlerMock2.Verify();
            _eventHandlerMock3.Verify();
        }

        [Test]
        public void ShouldCallOnShutdown()
        {
            var aggregateEventHandler = new AggregateEventHandler<IEnumerable<int>>(_eventHandlerMock1.Object,
                                                                         _eventHandlerMock2.Object,
                                                                         _eventHandlerMock3.Object);

            _eventHandlerMock1.Setup(eh => eh.OnShutdown()).Verifiable("event handler 1 was not called");
            _eventHandlerMock2.Setup(eh => eh.OnShutdown()).Verifiable("event handler 2 was not called");
            _eventHandlerMock3.Setup(eh => eh.OnShutdown()).Verifiable("event handler 3 was not called");

            aggregateEventHandler.OnShutdown();

            _eventHandlerMock1.Verify();
            _eventHandlerMock2.Verify();
            _eventHandlerMock3.Verify();
        }

        [Test]
        public void ShouldHandleEmptyListOfEventHandlers()
        {
            var aggregateEventHandler = new AggregateEventHandler<int[]>();

            aggregateEventHandler.OnEvent(new[] { 7 }, 0L, true);
            aggregateEventHandler.OnStart();
            aggregateEventHandler.OnShutdown();
        }

        public interface ILifecycleAwareEventHandler<T> : IEventHandler<T>, ILifecycleAware
        {
        }
    }
}
