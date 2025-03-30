# Aghili.Logging

Aghili.Logging is a Dotnet library for Add simple way to log with compatibility of write in side thread.

## Installation

[![](https://img.shields.io/nuget/dt/Aghili.Logging.svg?style=flat-square)](https://www.nuget.org/packages/Aghili.Logging)
[![](https://img.shields.io/nuget/v/Aghili.Logging?style=flat-square)](https://www.nuget.org/packages/Aghili.Logging)
Use the package manager [nuget.org](https://www.nuget.org/packages/Aghili.Logging/) to install Aghili.Logging.

```pm
NuGet\Install-Package Aghili.Logging -Version latest
```

## How can I support ?
[![](https://img.shields.io/badge/shetab-ZarinPal-8a00a3.svg?style=flat-square)](https://zarinp.al/@maghili)

**--OR--**

You can always donate your time by introducing it to others.

## Usage

Using Aghili.Logging namespace
```cs
using Aghili.Logging;
```

### Log with multi line details
```cs
Log.Instance.WriteNew(LogMesssageType.Information, "Information log detail.", "detail2", "detail3","...");
```

### Log with different categories
```cs
Log.Instance.WriteNew(LogMesssageType.Information, "Information log detail.");
Log.Instance.WriteNew(LogMesssageType.Warrning, "Warrning log detail.");
Log.Instance.WriteNew(LogMesssageType.DataLog, "DataLog log detail.");
Log.Instance.WriteNew(LogMesssageType.Exception, "Exception log detail.");
```

### Log Exceptions
```cs
try
{
    throw new Exception("Error");
}
catch (Exception ex)
{
    Log.Instance.WriteNew(ex);
    Log.Instance.WriteNew(ex, "Detail1", "Detail2", "...");
}  
```

### Set settings for logger
For set setting for logger you most set some property from `Log.Instance.Settings`:
1- Enable logger
```cs
Log.Instance.Settings.Enabled = true;
```
2- Set message type that we want to logger log , this property is enum that can be set as flags.
```cs
Log.Instance.Settings.MesssageTypes = LogMesssageType.Information|LogMesssageType.Warrning|LogMesssageType.Exception|LogMesssageType.DataLog;
```

3- Set logger write adapter
```cs
Log.Instance.Settings.LogType = LogType.WindowsLogEvent;
#or
Log.Instance.Settings.LogType = LogType.File;
```

4- Set logger log rotation settings
```cs
Log.Instance.Settings.LogKeepDataOnLimitRichedPercent = 80;
Log.Instance.Settings.LogRowLimitPerContainer = 10000;
```

### Notice
This settings also exists in Log folder that created in application folder.

## Contributing

Stay update and please open an issue for improve this package
and for what you would like to change.

Please make sure to update tests as appropriate.

## License

MIT License

Copyright (c) 2010-2023 mostafa aghili

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
