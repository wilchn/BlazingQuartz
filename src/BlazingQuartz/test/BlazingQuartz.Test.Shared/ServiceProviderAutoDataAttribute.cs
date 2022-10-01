using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BlazingQuartz.Test.Shared
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceProviderAutoDataAttribute : AutoDataAttribute
    {
        public ServiceProviderAutoDataAttribute() : base(CreateFixture) { }

        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });

            var serviceScopeMock = fixture.Create<Mock<IServiceScope>>();

            var serviceScopeFactory = fixture.Create<Mock<IServiceScopeFactory>>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object);

            var serviceProviderMock = fixture.Freeze<Mock<IServiceProvider>>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            return fixture;
        }
    }
}

