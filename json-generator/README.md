# Json schema generator

Runs on .net core

Runs a against hact json file to generate a collection of classes and Newtonsoft JSON attributes for validation for reequests

Make sure asp.netcore 3+ apps are using newtonsoft as by defeault they wont
[https://dotnetcoretutorials.com/2019/12/19/using-newtonsoft-json-in-net-core-3-projects/](https://dotnetcoretutorials.com/2019/12/19/using-newtonsoft-json-in-net-core-3-projects/)

be aware of lines 68 - 71 that strip out certain properties. This is done as they related is so large it massively impacts the performance of deserialisation validation of the json.
the lines will cause a string to be inserted instead that can be parsed manually if provided. 
This has been developed to as an MVP so if anyone wants to convert it into a cli accepting the file from args and the config from a json file please do.

