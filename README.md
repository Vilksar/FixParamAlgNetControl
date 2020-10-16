# FixParamAlgNetControl

## Table of contents

* [Introduction](#introduction)
* [Download](#download)
* [Usage](#usage)
  * [Help](#help)
  * [CLI](#cli)
* [Examples](#examples)

## Introduction

Welcome to the FixParamAlgNetControl repository!

This is C# / .Net Core application which implements algorithms aiming to help in solving the structural target controllability problem. The application is cross-platform, working on all modern operating systems (Windows, MacOS, Linux) and can be run through CLI (command-line interface).

## Download

The repository consists of a Visual Studio 2019 project. You can download it to run or build the application yourself. You need to have [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) installed on your computer in order to run it, or the corresponding SDK in order to also be able to build it.

## Usage

Open your operating system's terminal or console and navigate to the `FixParamAlgNetControl` folder (the one containing the file `Program.cs`). There, you can run the application with:

```
dotnet run
```

### Help

In order to find out more about the usage and possible arguments which can be used, you can launch the application with the `--Mode` argument set to `Help` (or omit it entirely), for example:

```
--Mode "Help"
```

This mode has one optional argument:

* `--GenerateParametersFile`. Use this argument to instruct the application to generate, in the current directory, a model of the parameters JSON file (containing the default parameter values) required for running the algorithm. Writing permission is needed for the directory. The default value is `False`.

### CLI

To run the application via CLI, you need to launch it from the terminal with the `--Mode` argument set to `CLI`, for example:

```
--Mode "CLI"
```

This mode has four mandatory arguments (omitting any of them will return an error) and one optional one:

* `--Edges`. Use this argument to specify the path to the file containing the edges of the network. Each edge should be on a new line, with its source and target nodes being separated by a semicolon character, for example:
  
  ```
  Node A;Node B
  Node A;Node C
  Node A;Node D
  Node C;Node D
  ```
  
  If the file is in a different format, or no nodes or edges could be found, an error will be returned. The order of the nodes is important, as the network is directed. Thus, `Node A;Node B` is not the same as `Node B;Node A`, and they can both appear in the network. Any duplicate edges will be ignored. The set of nodes in the network will be automatically inferred from the set of edges. This argument does not have a default value.

* `--Targets`. Use this argument to specify the path to the file containing the target nodes of the network. Each node should be on a new line.
  
  ```
  Node C
  Node D
  ```
  
  If the file is in a different format, or no nodes could be found in the network, an error will be returned. Only the nodes which already appear in the network will be considered. Any duplicate nodes will be ignored. This argument does not have a default value.

* `--Sources`. Use this argument to specify the path to the file containing the source nodes of the network. Each node should be on a new line.
  
  ```
  Node A
  Node C
  ```
  
  If the file is in a different format, or no nodes could be found in the network, an error will be returned. Only the nodes which already appear in the network will be considered. Any duplicate nodes will be ignored. This argument does not have a default value.
  
* `--Parameters`. Use this argument to specify the path to the file containing the parameter values for the analysis. The file should be in JSON format. You can generate a model file containing the default parameter values by running the application with the `Mode` argument set to `Help` and the `GenerateParametersFile` argument set to `True`.
  
  ```
  {
    "RandomSeed": -1,
    "MaximumPathLength": 5,
    "RankComputations": 3,
    "RunInParallel": false
  }
  ```
  
  The parameters are presented below.
  
  * `RandomSeed`. Represents the random seed for the current algorithm run. A value of `-1` will generate a random value for the seed. It must be a positive integer, and its default value is `-1`.
  * `MaximumPathLength`. Represents the maximum number of edges in a control path. It must be a positive integer, and its default value is `5`.
  * `RankComputations`. Represents the number of matrix rank computations to determine its structural rank. It must be a positive integer, and its default value is `3`.
  * `RunInParallel`. Represents a flag which enables running the current algorithm in parallel over multiple threads. It must be a boolean, and its default value is `false`.
  
  If the file is in a different format, an error will be returned. Additionally, if any of the parameters are missing, their default values will be used. This argument does not have a default value.

* `--Output`. (optional) Use this argument to specify the path to the output file where the solutions of the analysis will be written. Permission to write is needed for the corresponding folder. If a file with the same name already exists, it will be automatically overwritten. The default value is the name of the file containing the edges, followed by the current date and time.

If all the files have been successfully read and loaded, a confirmation message will be logged to the terminal and the algorithm will start running, providing constant feedback on its progress. Upon completion, all of the solutions will be written to the JSON file specified by the `--Output` argument.

## Examples

These are a few examples of possible command-line parameters for running the application.

* Help
  
  ```
  ./FixParamAlgNetControl --Mode "Help"
  ```
  
  ```
  ./FixParamAlgNetControl --Mode "Help" --GenerateParametersFile "True"
  ```
  
* CLI
    
  ```
  ./FixParamAlgNetControl --Mode "CLI" --Edges "Path/To/FileContainingEdges.extension" --Targets "Path/To/FileContainingTargetNodes.extension" --Sources "Path/To/FileContainingSourceNodes.extension" --Parameters "Path/To/FileContainingParameters.extension"
  ```
  
  ```
  ./FixParamAlgNetControl --Mode "CLI" --Edges "Path/To/FileContainingEdges.extension" --Targets "Path/To/FileContainingTargetNodes.extension" --Sources "Path/To/FileContainingSourceNodes.extension" --Parameters "Path/To/FileContainingParameters.extension" --Output "Path/To/OutputFile.extension"
  ```
