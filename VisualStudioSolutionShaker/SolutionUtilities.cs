// -----------------------------------------------------------------------
// <copyright file="SolutionUtilities.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018-2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionShaker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Construction;

    internal static class SolutionUtilities
    {
        internal static bool TryGetDepedenciesFolderGuid(SolutionFile targetSolution, out string dependenciesFolderGuid)
        {
            const string DEPENDENCIES_FOLDER_NAME = "Dependencies";
            bool dependenciesFolderFound = false;
            dependenciesFolderGuid = string.Empty;

            ProjectInSolution[] dependenciesFolders =
                targetSolution
                .ProjectsInOrder
                .Where(project => project.ProjectType == SolutionProjectType.SolutionFolder)
                .Where(projectSolutionFolder => projectSolutionFolder.ProjectName.Equals(DEPENDENCIES_FOLDER_NAME))
                .ToArray();

            if (dependenciesFolders.Length == 1)
            {
                // Best case is a folder already exist with this project; return its Guid
                dependenciesFolderGuid = dependenciesFolders.First().ProjectGuid;
                dependenciesFolderFound = true;
            }
            else if (dependenciesFolders.Length > 1)
            {
                string message = $"There were {dependenciesFolders.Length} `{DEPENDENCIES_FOLDER_NAME}` Folders Found; This is unexpected";
                throw new InvalidOperationException(message);
            }

            return dependenciesFolderFound;
        }

        internal static bool TryGetDepedenciesFolderGuid(string targetSolution, out string dependenciesFolderGuid)
        {
            SolutionFile solution = SolutionFile.Parse(targetSolution);
            return TryGetDepedenciesFolderGuid(solution, out dependenciesFolderGuid);
        }

        internal static IEnumerable<string> GetProjectsFromSolution(string targetSolution)
        {
            SolutionFile solution = SolutionFile.Parse(targetSolution);
            string solutionFolder = Path.GetDirectoryName(targetSolution);

            return
                solution
                .ProjectsInOrder
                .Where(project => project.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(project => PathUtilities.ExpandPath(project.AbsolutePath));
        }

        internal static IEnumerable<string> GetProjectsFromSolutionExceptDependencies(string targetSolution)
        {
            SolutionFile solution = SolutionFile.Parse(targetSolution);
            string dependenciesFolder = null;

            IEnumerable<string> dependenciesFolderProjects = new string[0];

            // It is possible that the Solution did not contain a "Dependencies" Folder
            if (TryGetDepedenciesFolderGuid(targetSolution, out dependenciesFolder))
            {
                dependenciesFolderProjects = GetProjectsNestedInSolutionFolder(targetSolution, dependenciesFolder);
            }

            return
                solution
                .ProjectsInOrder
                .Where(project => project.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(project => PathUtilities.ExpandPath(project.AbsolutePath))
                .Except(dependenciesFolderProjects);
        }

        internal static IEnumerable<string> GetProjectsNestedInSolutionFolder(string targetSolution, string solutionFolderGuid)
        {
            SolutionFile solution = SolutionFile.Parse(targetSolution);
            string solutionFolder = Path.GetDirectoryName(targetSolution);

            // Scan the Solution File looking for lines in this format:
            //  {A2CA68C7-43CB-4E31-A10E-BDF28DAFB512} = {0D1BC702-FE2E-4C44-B710-E7CC66E5D5FD}
            string regexPattern = Regex.Escape(solutionFolderGuid) + "$";

            // Extract the Guids of the Projects referenced by this folder
            IEnumerable<string> projectGuidsUnderSolutionFolder =
                File
                .ReadLines(targetSolution)
                .Where(currentLine => Regex.IsMatch(currentLine, regexPattern))
                .Select(solutionSubfolderLine => ExtractProjectGuid(solutionSubfolderLine));

            // Now we need to perform a lookup to see which projects are associated with these Guids
            IReadOnlyDictionary<string, ProjectInSolution> projectGuidLookupDictionary = solution.ProjectsByGuid;

            foreach (string projectGuidUnderSolutionFolder in projectGuidsUnderSolutionFolder)
            {
                if (projectGuidLookupDictionary.ContainsKey(projectGuidUnderSolutionFolder))
                {
                    string unexpandedAbsolutePath = projectGuidLookupDictionary[projectGuidUnderSolutionFolder].AbsolutePath;

                    // The Project API returns this using relative paths we
                    // want to expand this to the Fully Qualified Path.
                    string fullyQualifiedPath = PathUtilities.ExpandPath(unexpandedAbsolutePath);

                    yield return fullyQualifiedPath;
                }
                else
                {
                    // The referenced project did not exist in the solution; this was probably a bad solution!
                    string exception = $"The ProjectGuid `{projectGuidUnderSolutionFolder}` was listed as being under the given solution folder but did not exist in the solution. This indicates a corrupt solution file.";
                    throw new InvalidOperationException(exception);
                }
            }
        }

        private static string ExtractProjectGuid(string solutionSubfolderLine)
        {
            // We need to extract the FIRST Guid from lines in this format:
            //  {A2CA68C7-43CB-4E31-A10E-BDF28DAFB512} = {0D1BC702-FE2E-4C44-B710-E7CC66E5D5FD}
            string regexToMatchGuids = @"(?i)\{[0-9A-F]{8}-(?:[0-9A-F]{4}-){3}[0-9A-F]{12}\}";

            // Result should be {A2CA68C7-43CB-4E31-A10E-BDF28DAFB512}
            string result = Regex.Match(solutionSubfolderLine, regexToMatchGuids).Captures[0].Value;

            return result;
        }
    }
}
