# Introduction

`px-swatch` generates a palette from a jpg or png image using Magick.NET. Can output to png for visualization and sampling or gpl format for importing into GIMP or Krita.

# Building

1. Install Magick.NET package
    - `dotnet add package Magick.NET-Q8-AnyCPU`
2. Build Project
    - `dotnet publish -c Release -r <RID>`
    - Linux RID: `linux-x64`
    - Windows RID: `win-x64`

# Run
`./bin/Release/net8.0/<RID>/publish/px-swatch`


