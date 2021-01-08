# GeckoUI 
[![Build Status](https://travis-ci.com/el3ctr1c1ty/geckoui.svg?branch=master)](https://travis-ci.com/el3ctr1c1ty/geckoui)

GeckoUI is a powerful and lightweight tool to build user interfaces with Html/Css, based on GeckoFX which is .NET implementation of Netscape (Firefox) Gecko web engine.

## How to use
 - Download binaries and files from Releases
 - Navigate to Project Properties, in "Application tab set "Output type" to "Windows Application" 
 - In "Build" tab, set "Platform target" to "x86" and close Project Properties.
## Example

```csharp    
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(true);
var form = new GeckoUI.GeckoForm { EnableBorder = false, EnableEffects = true, RoundRadius = 10 };
form.LoadPage("https://electricity.su/geckoui", 800, 510);
Application.Run(form);
```
    
    
