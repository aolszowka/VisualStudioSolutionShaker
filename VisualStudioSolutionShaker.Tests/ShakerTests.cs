// -----------------------------------------------------------------------
// <copyright file="ShakerTests.cs" company="Ace Olszowka">
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
    public class ShakerTests
    {
        [TestCaseSource(typeof(ShakeSolution_ValidInput_Tests))]
        public void ShakeSolution_ValidInput(string targetSolution, IEnumerable<string> expected)
        {
            IEnumerable<string> actual = Shaker.ShakeSolution(targetSolution);

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }

    internal class ShakeSolution_ValidInput_Tests : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_A", "net472_A.sln"),
                    Array.Empty<string>()
                );
            yield return new TestCaseData
                (
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_B", "net472_B.sln"),
                    new string[]
                    {
                        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "net472_B", "net472_BB", "net472_BB.csproj"),
                    }
                );
        }
    }
}
