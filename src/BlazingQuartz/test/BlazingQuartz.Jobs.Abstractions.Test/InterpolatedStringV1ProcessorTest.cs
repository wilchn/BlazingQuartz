using BlazingQuartz.Jobs.Abstractions.Processors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazingQuartz.Jobs.Abstractions.Test;

public class InterpolatedStringV1ProcessorTest
{
    [Fact]
    public void Process_MultipleVariableBlocks()
    {
        var now = DateTime.UtcNow;
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            @"Today is {{$datetime ""yyyy-MM-dd""}} and {{$datetime 'yyyy-MM'}}",
            1);
        var processor = new InterpolatedStringV1Processor(logger);
        var expected = $"Today is {now:yyyy-MM-dd} and {now:yyyy-MM}";


        var result = processor.Process(input);

        result.Should().Be(expected);
    }

    [Fact]
    public void Process_MultipleLines()
    {
        var now = DateTime.UtcNow;
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            "This is new line" + Environment.NewLine +
            @"Today is {{$datetime ""yyyy-MM-dd""}}" + Environment.NewLine + "",
            1);
        var processor = new InterpolatedStringV1Processor(logger);
        var expected = "This is new line" + Environment.NewLine +
            $"Today is {now:yyyy-MM-dd}" + Environment.NewLine + "";

        var result = processor.Process(input);

        result.Should().Be(expected);
    }

    [Fact]
    public void Process_InvalidInterpolatedString_ReturnOriginalValue()
    {
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var inputStr = @"This is {{should not replace}}";
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            inputStr,
            1);
        var processor = new InterpolatedStringV1Processor(logger);

        var result = processor.Process(input);

        result.Should().Be(inputStr);
    }

    [Fact]
    public void Process_InvalidInterpolatedStringNoClosingBracket_ReturnOriginalValue()
    {
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var inputStr = @"This is {{$datetime not replace";
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            inputStr,
            1);
        var processor = new InterpolatedStringV1Processor(logger);

        var result = processor.Process(input);

        result.Should().Be(inputStr);
    }

    [Fact]
    public void Process_InvalidPattern_ThrowFormatException()
    {
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var inputStr = @"This is {{$datetime}}";
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            inputStr,
            1);
        var processor = new InterpolatedStringV1Processor(logger);

        Action process = () => processor.Process(input);

        process.Should().Throw<FormatException>();
    }

    [Fact]
    public void Process_NestedValidSameVariable_ThrowFormatException()
    {
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var inputStr = @"{{$datetime {{$datetime 'yyyy-MM'}}}}";
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            inputStr,
            1);
        var processor = new InterpolatedStringV1Processor(logger);

        Action process = () => processor.Process(input);

        process.Should().Throw<FormatException>("Outer $datetime pattern is invalid.");
    }

    [Fact]
    public void Process_NestedValidDifferentVariable_ThrowFormatException()
    {
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var inputStr = @"{{$datetime {{$guid}}}}";
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            inputStr,
            1);
        var processor = new InterpolatedStringV1Processor(logger);

        Action process = () => processor.Process(input);

        process.Should().Throw<FormatException>("$datetime pattern is invalid");
    }

    [Fact]
    public void Process_NestedValidNoParameterVariableInOuterBlock_ThrowFormatException()
    {
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var inputStr = @"{{$guid {{$datetime iso8601}}}}";
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            inputStr,
            1);
        var processor = new InterpolatedStringV1Processor(logger);

        Action process = () => processor.Process(input);

        process.Should().Throw<FormatException>("$guid pattern is invalid");
    }

    [Fact]
    public void Process_NestedBlockInnerValidVariable_ProcessInnerBlock()
    {
        var now = DateTime.UtcNow;
        var logger = new Mock<ILogger<InterpolatedStringV1Processor>>().Object;
        var inputStr = @"{{dummy {{$datetime 'yyyy-MM'}}}}";
        var input = new DataMapValue(DataMapValueType.InterpolatedString,
            inputStr,
            1);
        var processor = new InterpolatedStringV1Processor(logger);
        var expected = $"{{{{dummy {now:yyyy-MM}}}}}";

        var result = processor.Process(input);

        result.Should().Be(expected);
    }
}
