using System;
using EdgeMq.Service.Metrics;
using Shouldly;
using Xunit;

namespace EdgeMQ.UnitTests.Service.Metrics;

public sealed class EventsPerIntervalTests
{
    [Fact]
    public void CurrentEventsPerSecond_NoEvents_ReturnsZero()
    {
        var meter = new EventsPerInterval();

        var result = meter.CurrentEventsPerSecond();

        result.ShouldBe(0);
    }

    [Fact]
    public void CurrentEventsPerSecond_AllEventsZero_ReturnsZero()
    {
        var meter = new EventsPerInterval();

        meter.AddEvents(0);
        meter.AddEvents(0);
        meter.AddEvents(0);

        var result = meter.CurrentEventsPerSecond();

        result.ShouldBe(0);
    }

    [Fact]
    public void CurrentEventsPerSecond_ThreeEventsInSecond_ReturnsResult()
    {
        var meter = new EventsPerInterval();

        meter.AddEvents(1);
        meter.AddEvents(2);
        meter.AddEvents(3);

        var result = meter.CurrentEventsPerSecond();

        result.ShouldBe(6);
    }

    [Fact]
    public void CurrentEventsPerSecond_ThreeEventsInEverySecond_ReturnsResult()
    {
        var meter = new EventsPerInterval();

        meter.AddEvents(1, DateTime.Now.AddSeconds(-3));
        meter.AddEvents(1, DateTime.Now.AddSeconds(-2));
        meter.AddEvents(1, DateTime.Now.AddSeconds(-1));

        var result = meter.CurrentEventsPerSecond();

        result.ShouldBeGreaterThan(0.99);
        result.ShouldBeLessThanOrEqualTo(1);
    }

    [Fact]
    public void CurrentEventsPerSecond_ThreeEventsIn9Seconds_ReturnsResult()
    {
        var meter = new EventsPerInterval();

        meter.AddEvents(1, DateTime.Now.AddSeconds(-9));
        meter.AddEvents(1, DateTime.Now.AddSeconds(-9));
        meter.AddEvents(1, DateTime.Now.AddSeconds(-1));

        var result = meter.CurrentEventsPerSecond();

        result.ShouldBeGreaterThan(0.3);
        result.ShouldBeLessThanOrEqualTo(0.34);
        meter.Count.ShouldBe(3);
    }

    [Fact]
    public void CurrentEventsPerSecond_90sOfEvents_CountsLast60()
    {
        var meter = new EventsPerInterval();

        for (var i = 90; i > 0; i--)
        {
            meter.AddEvents(2, DateTime.Now.AddSeconds(-i));
        }
        var result = meter.CurrentEventsPerSecond();

        result.ShouldBeGreaterThan(1.9);
        result.ShouldBeLessThanOrEqualTo(2);
        meter.Count.ShouldBe(EventsPerInterval.MaxSecondsEvaluation);
    }
}