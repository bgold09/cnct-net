using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnct.Core.Configuration;
using Moq;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CnctConfigTagFilteringTests
    {
        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAction_WhenMachineHasNoTags()
        {
            var action = new TestActionSpec();
            var config = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAction_WhenMachineHasTags()
        {
            var action = new TestActionSpec();
            var config = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenMachineTagMatches()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenNoMachineTags()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(Array.Empty<string>(), action);

            await config.ExecuteAsync();

            Assert.False(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsTaggedAction_WhenMachineTagsDoNotMatch()
        {
            var action = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(new[] { "work" }, action);

            await config.ExecuteAsync();

            Assert.False(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_CaseInsensitiveMatch()
        {
            var action = new TestActionSpec { Tags = new[] { "Personal" } };
            var config = MakeConfig(new[] { "personal" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsTaggedAction_WhenAnyTagMatches()
        {
            var action = new TestActionSpec { Tags = new[] { "personal", "home" } };
            var config = MakeConfig(new[] { "work", "home" }, action);

            await config.ExecuteAsync();

            Assert.True(action.WasExecuted);
        }

        [Fact]
        public async Task ExecuteAsync_RunsUntaggedAndSkipsTagged_Mixed()
        {
            var untaggedAction = new TestActionSpec();
            var taggedAction = new TestActionSpec { Tags = new[] { "personal" } };
            var config = MakeConfig(Array.Empty<string>(), untaggedAction, taggedAction);

            await config.ExecuteAsync();

            Assert.True(untaggedAction.WasExecuted);
            Assert.False(taggedAction.WasExecuted);
        }

        private static CnctConfig MakeConfig(IReadOnlyCollection<string> machineTags, params ICnctActionSpec[] actions)
        {
            return new CnctConfig
            {
                Logger = Mock.Of<ILogger>(),
                ConfigRootDirectory = "/",
                MachineTags = machineTags,
                Actions = actions,
            };
        }

        private class TestActionSpec : CnctActionSpecBase
        {
            public bool WasExecuted { get; private set; }

            public override string ActionType => "test";

            public override void Validate()
            {
            }

            public override Task ExecuteAsync(ILogger logger, string configDirectoryRoot)
            {
                this.WasExecuted = true;
                return Task.CompletedTask;
            }
        }
    }
}
