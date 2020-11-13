// -----------------------------------------------------------------------
// <copyright file="SolutionUtilitiesTests.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2020. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionShaker.Tests
{
    using System.Collections;

    using NUnit.Framework;

    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SolutionUtilitiesTests
    {
        [TestCaseSource(typeof(ExtractProjectGuid_ValidInput_Tests))]
        public void ExtractProjectGuid_ValidInput(string solutionSubfolderLine, string expected)
        {
            var actual = SolutionUtilities.ExtractProjectGuid(solutionSubfolderLine);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    internal class ExtractProjectGuid_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData("  {A2CA68C7-43CB-4E31-A10E-BDF28DAFB512} = {0D1BC702-FE2E-4C44-B710-E7CC66E5D5FD}", "{A2CA68C7-43CB-4E31-A10E-BDF28DAFB512}");
        }
    }
}
