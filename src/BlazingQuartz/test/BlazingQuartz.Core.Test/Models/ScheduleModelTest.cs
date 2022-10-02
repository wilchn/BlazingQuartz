using System;
using FluentAssertions;
using AutoFixture;
using BlazingQuartz.Core.Models;

namespace BlazingQuartz.Core.Test.Models;

public class ScheduleModelTest
{
    int maxLength = 25;

    [Fact]
    public void GetJobTypeShortName_NameEqualMax_ReturnFullString()
    {
        var fixture = new Fixture();
        var rawJobType = string.Join(string.Empty,
            fixture.CreateMany<string>(2)).Substring(0, maxLength);
        var model = new ScheduleModel()
        {
            JobType = $"{rawJobType[..^4]}.{rawJobType[^3..]}"
        };

        var result = model.GetJobTypeShortName(maxLength);

        result.Should().Be(model.JobType);
    }

    [Fact]
    public void GetJobTypeShortName_NameGreaterMax_ReturnMaxString()
    {
        var fixture = new Fixture();
        var rawJobType = string.Join(string.Empty, fixture.CreateMany<string>(2))
            .Substring(0, maxLength + 10);
        // 12 is 10 + 2 dots
        var expected = $"{rawJobType[..^(12+4)]}...{rawJobType[^3..]}";
        var model = new ScheduleModel()
        {
            JobType = $"{rawJobType[..^4]}.{rawJobType[^3..]}"
        };

        var result = model.GetJobTypeShortName(maxLength);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetJobTypeShortName_ClassNameGreaterMax_ReturnClassNameOnly()
    {
        var fixture = new Fixture();
        var rawJobType = string.Join(string.Empty, fixture.CreateMany<string>(2))
            .Substring(0, maxLength + 10);
        var model = new ScheduleModel()
        {
            JobType = $"abc.{rawJobType}"
        };

        var result = model.GetJobTypeShortName(maxLength);

        result.Should().Be(rawJobType);
    }
}

