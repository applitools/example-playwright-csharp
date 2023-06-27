# Applitools Example: Playwright in C#

This is the example project for the [Playwright C# tutorial](https://applitools.com/tutorials/quickstart/web/playwright/csharp).
It shows how to start automating visual tests
with [Applitools Eyes](https://applitools.com/platform/eyes/)
and [Playwright](https://playwright.dev/dotnet) in C#.

It uses:

* [C#](https://learn.microsoft.com/en-us/dotnet/csharp/) as the programming language
* [Playwright](https://playwright.dev/dotnet) for browser automation
* [Chromium](https://www.chromium.org/chromium-projects/) as the local browser for testing
* [NUnit](https://nunit.org/) as the core test framework
* [Applitools Eyes](https://applitools.com/platform/eyes/) for visual testing

It can also run tests with:

* [Applitools Ultrafast Grid](https://applitools.com/platform/ultrafast-grid/) for cross-browser execution

To run this example project, you'll need:

1. An [Applitools account](https://auth.applitools.com/users/register), which you can register for free.
2. A good C# editor, such as [Microsoft Visual Studio](https://visualstudio.microsoft.com/)
   or [Visual Studio Code](https://code.visualstudio.com/docs/languages/csharp).
3. The [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) SDK (which may come bundled with Visual Studio).

The main test case is [`AcmeBankTests.cs`](Applitools.Example.Tests/AcmeBankTests.cs).
By default, the project will run tests with Ultrafast Grid.
You can change these settings in the test class.

To execute tests, set the `APPLITOOLS_API_KEY` environment variable
to your [account's API key](https://applitools.com/tutorials/guides/getting-started/registering-an-account),
and then run:

```
dotnet build
pwsh Applitools.Example.Tests/bin/Debug/net7.0/playwright.ps1 install
dotnet test
```

If `pwsh` is not available,
you must [install PowerShell](https://docs.microsoft.com/powershell/scripting/install/installing-powershell).

**For full instructions on running this project, take our
[Playwright C# tutorial](https://applitools.com/tutorials/quickstart/web/playwright/csharp)!**
