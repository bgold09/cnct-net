using System;
using System.Collections.Generic;
using Cnct.Core.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Cnct.Core.Tests
{
    public class CloneGitRepositoryTaskSpecificationTests
    {
        [Fact]
        public void CanDeserializeCloneGitRepositoryTaskSpec()
        {
            var expectedRepos = new Dictionary<string, string>
            {
                ["https://example.com/org/repo-a"] = "~/dev/repo-a",
            };

            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "cloneGitRepository",
                ["repos"] = expectedRepos,
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<CloneGitRepositoryTaskSpecification>(specInterface);
            Assert.Equal(expectedRepos.Count, spec.Repos.Count);
            Assert.Equal(expectedRepos["https://example.com/org/repo-a"], spec.Repos["https://example.com/org/repo-a"]);
        }

        [Fact]
        public void CanDeserializeCloneGitRepositoryTaskSpecWithMultipleRepos()
        {
            var expectedRepos = new Dictionary<string, string>
            {
                ["https://example.com/org/repo-a"] = "~/dev/repo-a",
                ["https://example.com/org/repo-b"] = "~/dev/repo-b",
            };

            var json = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["actionType"] = "cloneGitRepository",
                ["repos"] = expectedRepos,
            });

            var specInterface = JsonConvert.DeserializeObject<ICnctActionSpec>(json);
            var spec = Assert.IsType<CloneGitRepositoryTaskSpecification>(specInterface);
            Assert.Equal(expectedRepos.Count, spec.Repos.Count);
            foreach (var kvp in expectedRepos)
            {
                Assert.Equal(kvp.Value, spec.Repos[kvp.Key]);
            }
        }

        [Fact]
        public void Validate_ThrowsWhenReposIsNull()
        {
            var spec = new CloneGitRepositoryTaskSpecification { Repos = null };
            Assert.Throws<InvalidOperationException>(() => spec.Validate());
        }

        [Fact]
        public void Validate_ThrowsWhenReposIsEmpty()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>(),
            };

            Assert.Throws<InvalidOperationException>(() => spec.Validate());
        }

        [Fact]
        public void Validate_DoesNotThrowWithValidSpec()
        {
            var spec = new CloneGitRepositoryTaskSpecification
            {
                Repos = new Dictionary<string, string>
                {
                    ["https://example.com/org/repo-a"] = "~/dev/repo-a",
                },
            };

            spec.Validate();
        }
    }
}
