// -----------------------------------------------------------------------
// <copyright file="SolutionUtilitiesTests.cs" company="Ace Olszowka">
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
    public class SolutionUtilitiesTests
    {
        [TestCaseSource(typeof(ExtractProjectGuid_ValidInput_Tests))]
        public void ExtractProjectGuid_ValidInput(string solutionSubfolderLine, string expected)
        {
            string actual = SolutionUtilities.ExtractProjectGuid(solutionSubfolderLine);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(typeof(GetProjectsFromSolutionExceptDependencies_ValidInput_Tests))]
        public void GetProjectsFromSolutionExceptDependencies_ValidInput(string solutionFile, IEnumerable<string> expected)
        {
            IEnumerable<string> actual = SolutionUtilities.GetProjectsFromSolutionExceptDependencies(solutionFile);

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [TestCaseSource(typeof(TryGetDepedenciesFolderGuid_ValidInput_Tests))]
        public void TryGetDepedenciesFolderGuid_ValidInput(string solutionFile, string expected)
        {
            string actual = string.Empty;
            SolutionUtilities.TryGetDepedenciesFolderGuid(solutionFile, out actual);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    internal class GetProjectsFromSolutionExceptDependencies_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_A.sln"),
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_AA", "net472_AA.csproj")
                    }
                );
        }
    }

    internal class TryGetDepedenciesFolderGuid_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_A.sln"), "{5AD1934C-0503-4768-9385-B27CC2DE023F}");
            yield return new TestCaseData(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_B", "net472_B.sln"), "{15A19F88-B2A7-457A-8251-DE41846A74BB}");
        }
    }

    internal class ExtractProjectGuid_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData("  {A2CA68C7-43CB-4E31-A10E-BDF28DAFB512} = {0D1BC702-FE2E-4C44-B710-E7CC66E5D5FD}", "{A2CA68C7-43CB-4E31-A10E-BDF28DAFB512}");
            yield return new TestCaseData("{65A803D1-AA8B-43CB-9BEB-CBFE282EB653} = {15A19F88-B2A7-457A-8251-DE41846A74BB}", "{65A803D1-AA8B-43CB-9BEB-CBFE282EB653}");
        }
    }
}
