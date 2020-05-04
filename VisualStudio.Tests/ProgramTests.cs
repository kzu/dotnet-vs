﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace VisualStudio.Tests
{
    public class ProgramTests
    {
        readonly TextWriter output;

        public ProgramTests(ITestOutputHelper output) =>
            this.output = new OutputHelperTextWriter(output);


        [Theory]
        [InlineData(null)]
        [InlineData("/help")]
        [InlineData("/?")]
        [InlineData("-?")]
        [InlineData("/h")]
        public async Task when_running_without_args_or_with_help_arg_then_usage_is_shown(params string[] args)
        {
            var program = new ProgramTest(output, new CommandFactory(), args ?? new string[0]);

            var exitCode = await program.RunAsync();

            Assert.Equal(0, exitCode);
            Assert.True(program.UsageShown);
        }

        [Fact]
        public async Task when_running_command_then_command_is_executed()
        {
            var command = Mock.Of<Command>();
            var commandFactory = new CommandFactory();
            commandFactory.RegisterCommand("test", () => Mock.Of<CommandDescriptor>(), x => command);

            var program = new Program(output, commandFactory, "test");

            var exitCode = await program.RunAsync();

            Assert.Equal(0, exitCode);
            Mock.Get(command).Verify(x => x.ExecuteAsync(output));
        }

        [Fact]
        public async Task when_descriptor_throws_show_usage_exception_then_command_usage_is_shown()
        {
            var commandDescriptor = new Mock<CommandDescriptor>();
            commandDescriptor.Setup(x => x.Parse(It.IsAny<IEnumerable<string>>())).Throws(new ShowUsageException(commandDescriptor.Object));

            var commandFactory = new CommandFactory();
            commandFactory.RegisterCommand("test", () => commandDescriptor.Object, x => null);

            var program = new Program(output, commandFactory, "test");

            var exitCode = await program.RunAsync();

            Assert.Equal(ErrorCodes.ShowUsage, exitCode);
            commandDescriptor.Verify(x => x.ShowUsage(It.IsAny<ITextWriter>()));
        }

        [Fact]
        public async Task when_command_throws_then_error_code_is_returned()
        {
            var command = new Mock<Command>();
            command.Setup(x => x.ExecuteAsync(output)).Throws(new InvalidOperationException());

            var commandFactory = new CommandFactory();
            commandFactory.RegisterCommand("test", () => Mock.Of<CommandDescriptor>(), x => command.Object);

            var program = new Program(output, commandFactory, "test");

            var exitCode = await program.RunAsync();

            Assert.Equal(ErrorCodes.Error, exitCode);
        }

        [Fact]
        public async Task when_command_throws_and_debug_is_specified_then_throws()
        {
            var command = new Mock<Command>();
            command.Setup(x => x.ExecuteAsync(output)).Throws(new InvalidOperationException());

            var commandFactory = new CommandFactory();
            commandFactory.RegisterCommand("test", () => Mock.Of<CommandDescriptor>(), x => command.Object);

            var program = new Program(output, commandFactory, "test", "--debug");

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await program.RunAsync());
        }

        class ProgramTest : Program
        {
            public ProgramTest(TextWriter output, CommandFactory commandFactory, params string[] args)
                : base(output, commandFactory, args)
            {
            }

            public bool UsageShown { get; set; }

            protected override void ShowUsage()
            {
                base.ShowUsage();

                UsageShown = true;
            }
        }
    }
}
