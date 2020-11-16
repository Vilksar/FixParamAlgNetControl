# FixParamAlgNetControl

## Table of contents

* [Introduction](#introduction)
* [Input](#input)
* [Output](#output)

## Introduction

This directory contains the data sets which have been used in testing the application. The source of each network is presented in the table below.

Networks | Source
:--- | :---:
Social Interaction 1, 2 | [Link](https://doi.org/10.1126/science.1089167)
Electronic Circuit 1 | [Link](https://doi.org/10.1126/science.298.5594.824)
Protein-Protein Interaction 1, 2, 3 | [Link](https://doi.org/10.1038/s41598-017-10491-y)

The protein-protein interaction networks have been generated starting from the essential genes for breast cancer (1, 3) and pancreatic cancer (2) described in the reference, using the [OmniPath](https://doi.org/10.1038/nmeth.4077) database, and considering the interactions between the essential genes (1, 2), or the interactions between the essential genes with one intermediary gene (3).

## Input

The ``Input`` directory contains examples of input data that can be read by the application. The actual networks can be found in the text files named ``Network_Name``. Each network file has several corresponding target nodes files named ``Network_Name_Target`` and source nodes files named ``Network_Name_Source``, all randomly generated. One file of each type is needed for one run of the application.

## Output

The ``Output`` directory contains the results obtained after running the application once on each of the networks in the ``Input`` directory and each of the pairs of target nodes and source nodes. Each output consists of a JSON file named ``Network_Name__Target__Source__Output``.

Given the stochastic nature of the algorithm, the results obtained after each run of the application might vary. Each file represents only one possible output.
