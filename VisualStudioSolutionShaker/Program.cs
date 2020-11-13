// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2019-2020. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace VisualStudioSolutionShaker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using NDesk.Options;

    using VisualStudioSolutionShaker.Properties;

    class Program
    {

        /// <summary>
        /// Utility to "shake" Visual Studio Solution Files (SLN), scans the
        /// given solution or directory for solution files, and then removes
        /// any projects in the "Dependencies" Solution Folder. The Solution
        /// is then resolved again for N-Order Dependencies.
        /// </summary>
        /// <param name="args">See <see cref="ShowUsage"/></param>
        public static void Main(string[] args)
        {
            string targetArgument = string.Empty;
            string ignoreFileArgument = string.Empty;
            bool validateOnly = false;
            bool showHelp = false;

            OptionSet p = new OptionSet()
            {
                { "<>", Strings.TargetArgumentDescription, v => targetArgument = v },
                { "validate", Strings.ValidateDescription, v => validateOnly = v != null },
                { "ignore=", Strings.IgnoreDescription, v => ignoreFileArgument = v },
                { "?|h|help", Strings.HelpDescription, v => showHelp = v != null },
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine(Strings.ShortUsageMessage);
                Console.WriteLine($"Try `{Strings.ProgramName} --help` for more information.");
                Environment.ExitCode = 21;
                return;
            }

            if (showHelp || string.IsNullOrEmpty(targetArgument))
            {
                ShowUsage(p);
            }
            else if (!Directory.Exists(targetArgument) && !File.Exists(targetArgument))
            {
                Console.WriteLine(Strings.InvalidTargetArgument, targetArgument);
                Environment.ExitCode = 9009;
            }
            else if (!string.IsNullOrEmpty(ignoreFileArgument) && !File.Exists(ignoreFileArgument))
            {
                Console.WriteLine(Strings.InvalidIgnoreFileArgument, ignoreFileArgument);
                Environment.ExitCode = 9009;
            }
            else
            {
                // First see if we have an ignore file
                string[] ignoredSolutionPatterns = new string[0];

                if (!string.IsNullOrEmpty(ignoreFileArgument))
                {
                    // Because we're going to constantly use this for lookups save it off
                    ignoredSolutionPatterns = _GetIgnoredSolutionPatterns(ignoreFileArgument).ToArray();
                }

                if (validateOnly)
                {
                    if (Directory.Exists(targetArgument))
                    {
                        if (ignoredSolutionPatterns.Any())
                        {
                            string validatingAllSolutions = $"Validating all solutions in `{targetArgument}` except those filtered by `{ignoreFileArgument}`";
                            Console.WriteLine(validatingAllSolutions);

                            Console.WriteLine($"These are the ignored patterns (From: {ignoreFileArgument})");
                            foreach (string ignoredSolutionPattern in ignoredSolutionPatterns)
                            {
                                Console.WriteLine("{0}", ignoredSolutionPattern);
                            }
                        }
                        else
                        {
                            string validatingAllSolutions = $"Validating all solutions in `{targetArgument}`";
                            Console.WriteLine(validatingAllSolutions);
                        }

                        Environment.ExitCode = ShakeSolutionsInDirectoryCount(targetArgument, ignoredSolutionPatterns);
                    }
                    else if (File.Exists(targetArgument))
                    {
                        string validatingSingleFile = $"Validating solution `{targetArgument}`";
                        Console.WriteLine(validatingSingleFile);
                        Environment.ExitCode = ShakeSolutionCount(targetArgument);
                    }
                    else
                    {
                        // It should not be possible to reach this point
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    if (Directory.Exists(targetArgument))
                    {
                        if (ignoredSolutionPatterns.Any())
                        {
                            string shakingAllSolutionsInDirectory = $"REM Shaking all solutions in `{targetArgument}` except those filtered by `{ignoreFileArgument}`";
                            Console.WriteLine(shakingAllSolutionsInDirectory);
                        }
                        else
                        {
                            string shakingAllSolutionsInDirectory = $"REM Shaking all Visual Studio Solutions (*.sln) in `{targetArgument}`";
                            Console.WriteLine(shakingAllSolutionsInDirectory);

                            Console.WriteLine($"REM These are the ignored patterns (From: {ignoreFileArgument})");
                            foreach (string ignoredSolutionPattern in ignoredSolutionPatterns)
                            {
                                Console.WriteLine("REM {0}", ignoredSolutionPattern);
                            }
                        }

                        bool projectsToRemove = ShakeSolutionsInDirectoryToConsole(targetArgument, ignoredSolutionPatterns);

                        // If there are any Solutions that need to be modified
                        // then we return a non-zero exit code.
                        if (projectsToRemove)
                        {
                            Environment.ExitCode = 1;
                        }
                        else
                        {
                            Environment.ExitCode = 0;
                        }
                    }
                    else if (File.Exists(targetArgument))
                    {
                        string updatingSingleFile = $"Shaking solution `{targetArgument}`";
                        Console.WriteLine(updatingSingleFile);
                        Environment.ExitCode = ShakeSolutionToConsole(targetArgument);
                    }
                    else
                    {
                        // It should not be possible to reach this point
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        /// <summary>
        /// Load the Solution Ignore Patterns from the given Text File.
        /// </summary>
        /// <param name="targetIgnoreFile">The Text File that contains the ignore patterns.</param>
        /// <returns>An IEnumerable of strings that contain the patterns for solutions to ignore.</returns>
        private static IEnumerable<string> _GetIgnoredSolutionPatterns(string targetIgnoreFile)
        {
            if (!File.Exists(targetIgnoreFile))
            {
                string exceptionMessage = $"The specified ignore pattern file at `{targetIgnoreFile}` did not exist or was not accessible.";
                throw new InvalidOperationException(exceptionMessage);
            }

            IEnumerable<string> ignoredPatterns =
                File
                .ReadLines(targetIgnoreFile)
                .Where(currentLine => !currentLine.StartsWith("#"));

            return ignoredPatterns;
        }

        /// <summary>
        /// Given a Solution File and a list of Patterns determine if the solution matches any of the patterns.
        /// </summary>
        /// <param name="targetSolution">The solution to evaluate.</param>
        /// <param name="ignoredSolutionPatterns">The RegEx of patterns to ignore.</param>
        /// <returns><c>true</c> if the solution should be processed; otherwise, <c>false</c>.</returns>
        private static bool _ShouldProcessSolution(string targetSolution, IEnumerable<string> ignoredSolutionPatterns)
        {
            bool shouldProcessSolution = true;

            bool isSolutionIgnored =
                ignoredSolutionPatterns
                .Any(ignoredPatterns => Regex.IsMatch(targetSolution, ignoredPatterns));

            if (isSolutionIgnored)
            {
                shouldProcessSolution = false;
            }

            return shouldProcessSolution;
        }

        /// <summary>
        /// Prints the Usage of this Utility to the Console.
        /// </summary>
        /// <param name="p">The <see cref="OptionSet"/> for this program.</param>
        /// <returns>An Exit Code Indicating that Help was Shown</returns>
        private static int ShowUsage(OptionSet p)
        {
            Console.WriteLine(Strings.ShortUsageMessage);
            Console.WriteLine();
            Console.WriteLine(Strings.LongDescription);
            Console.WriteLine();
            Console.WriteLine($"               <>            {Strings.TargetArgumentDescription}");
            p.WriteOptionDescriptions(Console.Out);
            return 21;
        }

        /// <summary>
        /// Given a path to a Solution determine how many projects would be "shaken" out.
        /// </summary>
        /// <param name="targetSolution">The solution to evaluate.</param>
        /// <returns>The number of projects that would be removed.</returns>
        /// <remarks>
        /// If the solution fails to load a value of -1 is returned.
        /// </remarks>
        internal static int ShakeSolutionCount(string targetSolution)
        {
            int projectRemovalCount = -1;

            try
            {
                projectRemovalCount = Shaker.ShakeSolution(targetSolution).Count();

                if (projectRemovalCount > 0)
                {
                    Console.WriteLine($"`{projectRemovalCount}` Projects Can Be Removed From Solution `{targetSolution}`");
                }
            }
            catch
            {
                // We don't care other than we got an error
                Console.WriteLine($"Failed to Load `{targetSolution}`");
            }

            return projectRemovalCount;
        }

        /// <summary>
        ///   Given a directory scan for any solution files that are not in the
        /// <paramref name="ignoredSolutionPatterns"/> then determine how many
        /// projects would be "shaken" out from all projects.
        /// </summary>
        /// <param name="targetDirectory">The directory to scan for solutions.</param>
        /// <param name="ignoredSolutionPatterns">The solutions to ignore.</param>
        /// <returns>The number of projects that would be removed.</returns>
        /// <remarks>
        /// Solutions that fail to load are not taken into account by this overload.
        /// </remarks>
        internal static int ShakeSolutionsInDirectoryCount(string targetDirectory, IEnumerable<string> ignoredSolutionPatterns)
        {
            IEnumerable<string> filteredSolutions =
                Directory
                .EnumerateFiles(targetDirectory, "*.sln", SearchOption.AllDirectories)
                .Where(targetSolution => _ShouldProcessSolution(targetSolution, ignoredSolutionPatterns));

            // Note this command ignores any load failures when calculating
            // the result and nothing is given to the end user to indicate this
            int result =
                 filteredSolutions
                 .AsParallel()
                 .Select(currentSolution => ShakeSolutionCount(currentSolution))
                 .Where(removalCount => removalCount >= 0)
                 .Sum();

            return result;
        }

        /// <summary>
        ///    Given a Target Directory find all solutions, expect those that
        /// match the patterns in <paramref name="ignoredSolutionPatterns"/>,
        /// and generate the appropriate "dotnet sln" commands to "shake" the
        /// solutions.
        /// </summary>
        /// <param name="targetDirectory">The directory to scan for solution files.</param>
        /// <param name="ignoredSolutionPatterns">A set of RegEx patterns to ignore.</param>
        /// <returns>
        /// <c>true</c> if any solution needs to have removals performed; otherwise, <c>false</c>.
        /// </returns>
        internal static bool ShakeSolutionsInDirectoryToConsole(string targetDirectory, IEnumerable<string> ignoredSolutionPatterns)
        {
            bool projectsToRemoveFromSolutions = false;

            IEnumerable<string> filteredSolutions =
                Directory
                .EnumerateFiles(targetDirectory, "*.sln", SearchOption.AllDirectories)
                .Where(targetSolution => _ShouldProcessSolution(targetSolution, ignoredSolutionPatterns));

            Parallel.ForEach(filteredSolutions, targetSolution =>
            {
                int removalCountForSolution = ShakeSolutionToConsole(targetSolution);

                    // Note that we do not care about solutions that failed to load
                    if (removalCountForSolution > 0)
                {
                        // If ANY Project would fail let us know about it
                        projectsToRemoveFromSolutions = true;
                }
            }
            );

            return projectsToRemoveFromSolutions;
        }

        /// <summary>
        ///   Given a single solution generate the appropriate "dotnet sln"
        /// commands to "shake" this solution.
        /// </summary>
        /// <param name="targetSolution">The solution file to modify.</param>
        /// <returns>The number of projects that would be shaken out of the given solution.</returns>
        internal static int ShakeSolutionToConsole(string targetSolution)
        {
            int removalCountForSolution = 0;

            try
            {
                IEnumerable<string> projectsToRemove = Shaker.ShakeSolution(targetSolution);

                if (projectsToRemove.Any())
                {
                    foreach (string projectToRemove in projectsToRemove)
                    {
                        removalCountForSolution++;
                        Console.WriteLine($"dotnet sln \"{targetSolution}\" remove \"{projectToRemove}\"");
                    }
                    Console.WriteLine($"REM `{removalCountForSolution}` Projects Can Be Removed From Solution `{targetSolution}`");
                }
            }
            catch
            {
                Console.WriteLine($"REM Failed to Load `{targetSolution}`. This solution file will not be processed.");
            }

            return removalCountForSolution;
        }
    }
}
