# hact-utils

Required .net framework and must be run on a machine with microsoft excel installed.

Generates a cs file from the attributes sheet of a hact excel workbook.

Generated files are not formatted so should be copied into the solution. 
HACT excel spread sheets include many duplicates between them and some only define partial object. Generated files will need to be merged and duplicate properties removed.

It is adviseable to use EF core 5+ to allow the used of owned entites without a performance impact as per [https://github.com/dotnet/efcore/issues/18299](https://github.com/dotnet/efcore/issues/18299),  Ef core 5+ is compatible with .net core 3.1+.

Before generating from a sheet it is worth going through the attributes sheets and checking there are no new built in types used. Examples are ```time```, ```date```, ```integer```. These new types should be added to the dictionary in generator with a value matching the desired generated type.

New excel files will need to be defined with copy to output in the csproj or via IDE
```
    <None Include="res\RaiseRepair.xlsx">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
```

the generated file is written inside in the directory the program executes in so by default it will be in bin/Debug/

## Current issues
Lists of ignored properties may still generate.
Generated ids will need to be removed for owned objects.
Value types are not generated as nullable.
