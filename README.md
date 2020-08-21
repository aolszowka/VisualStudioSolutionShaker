# VisualStudioSolutionShaker
Utility program to "Shake" Visual Studio Solutions (SLN) of "stale" Dependencies.

## When To Use This Tool
This tool is a sister tool to [GitHub aolszowka/VisualSudioSolutionUpdater](https://github.com/aolszowka/VisualStudioSolutionUpdater). That tool is purely additive and makes no attempt to "shake" the dependency tree. Unfortunately you will soon find yourself in a situation where a dependency was added somewhere deep in the tree but then later removed. The Visual Studio Solution Updater (VSSU) will continue to happily add more Dependencies for this "dead" dependency.

In order to address the short comings of that tool this tool was written.

This tool is helpful in "shaking" or "pruning" references that are considered "stale" by its ruleset.

## Operation
This tool will:

* (If Given a Single Solution) Perform the below operation for just this Solution
* (If Given a Directory) Will scan the given directory and all subdirectories for Visual Studio Solution files (*.sln) and perform the operation below.

For Each Solution File

* Load all Projects from the Solution File into a List. (`AllExistingProjects`)
* Load all projects from the Solution File *EXCEPT* those contained within the Virtual Solution Folder `Dependencies` into a List. (`AllProjectsExceptDependencies`)
* Crawl each project in `AllProjectsExceptDependencies` resolving its N-Order [Microsoft Docs: Common MSBuild Project Items - ProjectReference](https://docs.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-items?view=vs-2017#projectreference) into a List. (`ResolvedProjectList`)
* Delta `ResolvedProjectList` to `AllExistingProjects` to determine which projects can be "shaken".

Then depending on the operation chosen (`validate` or "Processing") one of two things will occur.

For `validate` output on the console will look similar to the following:

```text
`90` Projects Can Be Removed From Solution `S:\BadSolution.sln`
```

The exit code will be the number of projects that will be removed from the solution files.

For Processing the output will generate the appropriate [Microsoft Docs: dotnet sln](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-sln) commands to be ran to remove these "stale" references. The intent is to pipe this output into a batch file and then execute it. To change this behavior see Hacking: Modifying Solutions below.

```text
dotnet sln "s:\BadSolution.sln" remove "s:\badproject.csproj"
REM `1` Projects Can Be Removed From Solution `s:\BadSolution.sln`
```

The exit code will be non-zero to indicate that projects would be removed from the solution files.

## Usage
```text
Usage: VisualStudioSolutionShaker [validate] directory/solution [ignore.txt]

Given either a Visual Studio Solution (*.sln) or a Directory to Scan; identify
any solution file that contains "extra" Projects. Extra Projects are defined as
projects that exist in a Virtual Solution Folder called "Dependencies" but are
not referenced by any project (via ProjectReference or RuntimeReference) outside
of the "Dependencies" folder.

Invalid Command/Arguments. Valid commands are:

Directory-Solution [IgnorePatterns.txt]
    [READS] If given a solution file or a directory find all solution files.
    Then opening each solution, grab all projects contained within the solution,
    removing those contained within the "Dependencies" Virtual Solution Folder.
    Then find all N-Order ProjectReference projects for those that remain.
    Compare this list to the existing list of projects in the Solution. Those
    projects that are missing from this new list are then output as "dotnet sln
    remove" commands for further processing to remove these projects.

validate Directory-Solution [IgnorePatterns.txt]
    [READS] Performs the above operation but instead the return code represents
    the number of projects that would be removed from the given solution files.

In all cases you can provide an optional argument of IgnorePatterns.txt (you
can use any filename) which should be a plain text file of regular expression
filters of solution files you DO NOT want this tool to operate on.

When ran in "Process Mode" The output of this tool is expected to be piped to a
batch file that can be executed to perform the actual removes. This batch file
utilizes the "dotnet sln" tool to perform removals individually from each
solution file. Solution files which fail to load are printed as comments within
the batch file. The return code is non-zero if the resulting batch file would
result in modified Solution Files; otherwise, it is zero.

For the Validate Command special rules apply to projects which fail to load. If
you are validating a single Solution file and it fails to load you will have -1
returned. However if you validate an entire Directory these projects are
ignored. The return code is the number of projects that would be removed from
the solution files, or zero if no projects would be removed from solutions.

In all cases this tool DOES NOT modify the existing Solution files.
```

## Hacking
### Dependencies (Solution Folder)
Currently this tool only looks for projects to shake that are in the `Dependencies` folder. Most of this logic is stored in `SolutionUtilities.GetProjectsFromSolutionExceptDependencies(string)` more specifically the logic in `SolutionUtilities.TryGetDepedenciesFolderGuid(SolutionFile, out string)` should be investigated.

### Modifying Solutions
Instead of attempting to reinvent the square wheel this tool instead provides output that is intended to be piped to a batch file which then invokes [Microsoft Docs: dotnet sln](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-sln) at some point in the future it might be nice to have this more tightly integrated into the tool.

## Contributing
Pull requests and bug reports are welcomed so long as they are MIT Licensed.

## License
This tool is MIT Licensed.

## Third Party Licenses
This project uses other open source contributions see [LICENSES.md](LICENSES.md) for a comprehensive listing.
