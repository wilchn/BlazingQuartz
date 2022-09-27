using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Quartz;
using RichardSzalay.MockHttp;

namespace BlazingQuartz.Test.Shared
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpAutoDataAttribute : AutoDataAttribute
    {
        public HttpAutoDataAttribute() : base(CreateFixture) { }

        private static IFixture CreateFixture()
        {
            var mockHttp = new MockHttpMessageHandler();
            var fixture = new Fixture();

            fixture.AddMockHttp();

            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });

            return fixture;
        }
    }
}

