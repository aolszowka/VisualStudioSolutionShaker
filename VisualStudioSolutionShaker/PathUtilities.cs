// -----------------------------------------------------------------------
// <copyright file="PathUtilities.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2017-2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionShaker
{
    using System;
    using System.IO;

    /// <summary>
    /// Utility class for dealing with Paths.
    /// </summary>
    public static class PathUtilities
    {

        /// <summary>
        /// Resolve a relative path given the base directory in which it is based.
        /// </summary>
        /// <param name="baseDirectory">The base path.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>The "expanded" path.</returns>
        public static string ResolveRelativePath(string baseDirectory, string relativePath)
        {
            string absolutePath = Path.Combine(baseDirectory, relativePath);
            return Path.GetFullPath((new Uri(absolutePath)).LocalPath);
        }

        public static string ExpandPath(string relativePath)
        {
            return Path.GetFullPath((new Uri(relativePath)).LocalPath);
        }
    }
}
