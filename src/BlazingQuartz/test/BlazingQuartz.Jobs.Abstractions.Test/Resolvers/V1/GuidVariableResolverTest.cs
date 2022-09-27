using System;
using BlazingQuartz.Jobs.Abstractions.Resolvers;
using BlazingQuartz.Jobs.Abstractions.Resolvers.V1;
using FluentAssertions;

namespace BlazingQuartz.Jobs.Abstractions.Test.Resolvers.V1
{
    public class GuidVariableResolverTest
    {
        private IResolver _resolver;

        public GuidVariableResolverTest()
        {
            _resolver = new GuidVariableResolver();
        }

        [Fact]
        public void Resolve_Guid()
        {
            var now = DateTimeOffset.Now;
            var input = "{{$guid}}";

            var result = _resolver.Resolve(input);

            result.Should().NotBe(input);
            result.Should().Contain("-", "Guid value contain dash");
        }
    }
}

