using System;
using FluentAssertions;
using BlazingQuartz.Jobs.Abstractions.Resolvers;
using BlazingQuartz.Jobs.Abstractions.Resolvers.V1;

namespace BlazingQuartz.Jobs.Abstractions.Test.Resolvers.V1
{
    public class LocalDateTimeVariableResolverTest
    {
        private IResolver _resolver;

        public LocalDateTimeVariableResolverTest()
        {
            _resolver = new LocalDateTimeVariableResolver();
        }

        [Fact]
        public void Resolve_LocalDateTime_LocalTime()
        {
            var now = DateTimeOffset.Now;
            var input = "{{$localDatetime 'HH:mm'}}";
            var expected = $"{now:HH:mm}";

            var result = _resolver.Resolve(input);

            result.Should().Be(expected);
        }

        [Fact]
        public void Resolve_Minus1Day_LocalDateLess1Day()
        {
            var now = DateTimeOffset.Now;
            var input = "{{$localDatetime 'dd' -1 d}}";
            var expected = $"{now.AddDays(-1):dd}";

            var result = _resolver.Resolve(input);

            result.Should().Be(expected);
        }
    }
}

