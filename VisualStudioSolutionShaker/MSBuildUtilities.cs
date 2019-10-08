// -----------------------------------------------------------------------
// <copyright file="MSBuildUtilities.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2017-2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// -----------------------------------------------------------------------
// <copyright file="MSBuildUtilities.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2017-2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionShaker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public static class MSBuildUtilities
    {
        internal static XNamespace msbuildNS = @"http://schemas.microsoft.com/developer/msbuild/2003";

        /// <summary>
        ///   Parses an MSBuild Project; Returning all DIRECT ProjectReferences
        /// with their relative paths resolved to full system paths.
        /// </summary>
        /// <param name="targetProject">The MSBuild Project to parse for project references.</param>
        /// <returns>An Enumerable of project references with the full system path.</returns>
        public static IEnumerable<string> GetMSBuildProjectReferencesFullPath(string targetProject)
        {
            return GetMSBuildProjectReferencesRelative(targetProject).Select(relativePath => PathUtilities.ResolveRelativePath(Path.GetDirectoryName(targetProject), relativePath));
        }

        /// <summary>
        ///   Parses an MSBuild Project; Returning all DIRECT ProjectReference
        ///  Include Tags (which are relative to the project).
        /// </summary>
        /// <param name="targetProject">The MSBuild Project to parse for project references.</param>
        /// <returns>An Enumerable of project references relative to the target project.</returns>
        public static IEnumerable<string> GetMSBuildProjectReferencesRelative(string targetProject)
        {
            XDocument synprojXml = XDocument.Load(targetProject);
            return synprojXml.Descendants(msbuildNS + "ProjectReference").Select(projectReferenceNode => projectReferenceNode.Attribute("Include").Value);
        }

        /// <summary>
        ///   Parses an MSBuild Project; Returning all RuntimeReference Elements.
        /// </summary>
        /// <param name="targetProject">The MSBuild Project to parse for RuntimeReferences.</param>
        /// <returns>An enumerable that contains all of the RuntimeReference Include Values.</returns>
        public static IEnumerable<string> GetRuntimeReferences(string targetProject)
        {
            XDocument projXml = XDocument.Load(targetProject);
            return
                projXml.Descendants(msbuildNS + "RuntimeReference")
                .Select(runtimeReference => runtimeReference.Attribute("Include").Value)
                .Select(relativePath => PathUtilities.ResolveRelativePath(Path.GetDirectoryName(targetProject), relativePath));
        }

        /// <summary>
        ///   Gets all Projects INCLUDING their N-Order Dependencies based off
        /// a project listing. Also Includes RuntimeReference Tags.
        /// </summary>
        /// <param name="projectList">A list of MSBuild Projects.</param>
        /// <returns>All Projects in the List INCLUDING their N-Order Dependencies AND Runtime References.</returns>
        public static IEnumerable<string> ProjectsIncludingNOrderDependencies(IEnumerable<string> projectList)
        {
            // Have our Resolved References
            SortedSet<string> resolvedReferences = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);

            // Start Spinning for References
            Stack<string> projectsToResolve = new Stack<string>(projectList.Distinct());

            while (projectsToResolve.Count > 0)
            {
                // Start resolving the current project
                string currentProjectToResolve = projectsToResolve.Pop();

                // Don't attempt to resolve projects which have already been resolved
                if (!resolvedReferences.Contains(currentProjectToResolve))
                {
                    // Add the current project to the list of resolved projects
                    resolvedReferences.Add(currentProjectToResolve);

                    // Get a list of all MSBuild ProjectReferences
                    IEnumerable<string> projectReferences = GetMSBuildProjectReferencesFullPath(currentProjectToResolve);

                    // But only add those which have not already been resolved
                    foreach (string projectReference in projectReferences)
                    {
                        if (!resolvedReferences.Contains(projectReference))
                        {
                            projectsToResolve.Push(projectReference);
                        }
                    }

                    // Also get a list of all RuntimeReferences
                    IEnumerable<string> runtimeReferences = MSBuildUtilities.GetRuntimeReferences(currentProjectToResolve);

                    // But only add those which have not already been resolved
                    foreach (string runtimeReference in runtimeReferences)
                    {
                        if (!resolvedReferences.Contains(runtimeReference))
                        {
                            projectsToResolve.Push(runtimeReference);
                        }
                    }
                }
            }

            return resolvedReferences;
        }
    }
}
