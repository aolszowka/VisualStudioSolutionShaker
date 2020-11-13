namespace VisualStudioSolutionShaker.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class MSBuildUtilitiesTests
    {
        [TestCaseSource(typeof(GetMSBuildProjectReferencesRelative_ValidInput_Tests))]
        public void GetMSBuildProjectReferencesRelative_ValidInput(string project, IEnumerable<string> expected)
        {
            IEnumerable<string> actual = MSBuildUtilities.GetMSBuildProjectReferencesRelative(project);

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [TestCaseSource(typeof(GetMSBuildProjectReferencesFullPath_ValidInput_Tests))]
        public void GetMSBuildProjectReferencesFullPath_ValidInput(string project, IEnumerable<string> expected)
        {
            IEnumerable<string> actual = MSBuildUtilities.GetMSBuildProjectReferencesFullPath(project);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class GetMSBuildProjectReferencesFullPath_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AA", "net472_AA.csproj"),
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AB", "net472_AB.csproj")
                    }
                );
            yield return new TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AC", "net472_AC.csproj"),
                    Array.Empty<string>()
                );
        }
    }

    internal class GetMSBuildProjectReferencesRelative_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AA", "net472_AA.csproj"),
                    new string[]
                    {
                        Path.Combine("..", "net472_AB", "net472_AB.csproj")
                    }
                );
            yield return new TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AC", "net472_AC.csproj"),
                    Array.Empty<string>()
                );
        }
    }
}
