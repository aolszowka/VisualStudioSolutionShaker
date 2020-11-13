// -----------------------------------------------------------------------
// <copyright file="MSBuildUtilitiesTests.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2020. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

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

        [TestCaseSource(typeof(ProjectsIncludingNOrderDependencies_ValidInput_Tests))]
        public void ProjectsIncludingNOrderDependencies_ValidInput(IEnumerable<string> projects, IEnumerable<string> expected)
        {
            IEnumerable<string> actual = MSBuildUtilities.ProjectsIncludingNOrderDependencies(projects);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class ProjectsIncludingNOrderDependencies_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData
                (
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AA", "net472_AA.csproj"),
                    },
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AA", "net472_AA.csproj"),
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AB", "net472_AB.csproj"),
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AC", "net472_AC.csproj"),
                    }
                );
            yield return new TestCaseData
                (
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_B", "net472_BA", "net472_BA.csproj"),
                    },
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_B", "net472_BA", "net472_BA.csproj")
                    }
                );
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
