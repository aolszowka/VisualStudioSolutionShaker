// -----------------------------------------------------------------------
// <copyright file="Shaker.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionShaker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    static class Shaker
    {
        internal static IEnumerable<string> ShakeSolution(string targetSolution)
        {
            IEnumerable<string> projectsInSolution = SolutionUtilities.GetProjectsFromSolution(targetSolution);
            IEnumerable<string> primeProjects = SolutionUtilities.GetProjectsFromSolutionExceptDependencies(targetSolution);
            IEnumerable<string> primeProjectsAndDependencies = MSBuildUtilities.ProjectsIncludingNOrderDependencies(primeProjects);

            // These projects need to be removed
            IEnumerable<string> projectsToRemove = projectsInSolution.Except(primeProjectsAndDependencies, StringComparer.OrdinalIgnoreCase);

            return projectsToRemove;
        }
    }
}
