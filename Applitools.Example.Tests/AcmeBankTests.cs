using System;
using System.Threading.Tasks;
using Applitools;
using Applitools.Playwright;
using Applitools.Playwright.Fluent;
using Applitools.Utils.Geometry;
using Applitools.VisualGrid;
using Microsoft.Playwright;
using NUnit.Framework;
using BrowserType = Applitools.BrowserType;

namespace ExamplePlaywrightCSharp;

public class AcmeBankTests
{
    // This NUnit test case class contains everything needed to run a full visual test against the ACME bank site.
    // It runs the test once locally.
    // If you use the Ultrafast Grid, then it performs cross-browser testing against multiple unique browsers.

    #pragma warning disable CS8618

    // Runner Settings.
    // These could be set by environment variables or other input mechanisms.
    // They are hard-coded here to keep the example project simple.
    private static readonly bool UseUltrafastGrid = true;

    // Test control inputs to read once and share for all tests
    private static string? ApplitoolsApiKey;
    private static bool Headless;

    // Applitools objects to share for all tests
    private static BatchInfo Batch;
    private static Configuration Config;
    private static PlaywrightEyesRunner Runner;

    // Test-specific objects
    private static IPlaywright Playwright;
    private static IBrowser Browser;
    private IBrowserContext Context;
    private IPage Page;
    private Eyes Eyes;

    #pragma warning restore CS8618

    [OneTimeSetUp]
    public static async Task SetUpConfigAndRunner()
    {
        // This method sets up the configuration for running visual tests.
        // The configuration is shared by all tests in a test suite, so it belongs in a `BeforeAll` method.
        // If you have more than one test class, then you should abstract this configuration to avoid duplication.

        // Read the Applitools API key from an environment variable.
        ApplitoolsApiKey = Environment.GetEnvironmentVariable("APPLITOOLS_API_KEY");

        // Read the headless mode setting from an environment variable.
        // Use headless mode for Continuous Integration (CI) execution.
        // Use headed mode for local development.
        Headless = bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "true");

        if (UseUltrafastGrid)
        {
            // Create the runner for the Ultrafast Grid.
            // Concurrency refers to the number of visual checkpoints Applitools will perform in parallel.
            // Warning: If you have a free account, then concurrency will be limited to 1.
            Runner = new VisualGridRunner(new RunnerOptions().TestConcurrency(5));
        }
        else
        {
            // Create the Classic runner.
            Runner = new ClassicRunner();
        }

        // Create a new batch for tests.
        // A batch is the collection of visual checkpoints for a test suite.
        // Batches are displayed in the Eyes Test Manager, so use meaningful names.
        string runnerName = (UseUltrafastGrid) ? "Ultrafast Grid" : "Classic runner";
        Batch = new BatchInfo("Example: Playwright C# with the " + runnerName);

        // Create a configuration for Applitools Eyes.
        Config = new Configuration();

        // Set the Applitools API key so test results are uploaded to your account.
        // If you don't explicitly set the API key with this call,
        // then the SDK will automatically read the `APPLITOOLS_API_KEY` environment variable to fetch it.
        Config.SetApiKey(ApplitoolsApiKey);

        // Set the batch for the config.
        Config.SetBatch(Batch);

        // If running tests on the Ultrafast Grid, configure browsers.
        if (UseUltrafastGrid)
        {
            // Add 3 desktop browsers with different viewports for cross-browser testing in the Ultrafast Grid.
            // Other browsers are also available, like Edge and IE.
            Config.AddBrowser(800, 600, BrowserType.CHROME);
            Config.AddBrowser(1600, 1200, BrowserType.FIREFOX);
            Config.AddBrowser(1024, 768, BrowserType.SAFARI);

            // Add 2 mobile emulation devices with different orientations for cross-browser testing in the Ultrafast Grid.
            // Other mobile devices are available, including iOS.
            Config.AddDeviceEmulation(DeviceName.Pixel_2, ScreenOrientation.Portrait);
            Config.AddDeviceEmulation(DeviceName.Nexus_10, ScreenOrientation.Landscape);
        }

        // Start Playwright and launch the browser.
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium
            .LaunchAsync(new BrowserTypeLaunchOptions { Headless = Headless });
    }

    [SetUp]
    public async Task OpenBrowserAndEyes()
    {
        // This method sets up each test with its own Page and Applitools Eyes objects.

        // Get a new context from the browser
        Context = await Browser.NewContextAsync();

        // Create a new page in the context.
        // Creating a new context is not mandatory and a new page can be created from the browser instance.
        // page_ = await browser_.NewPageAsync();
        Page = await Context.NewPageAsync();

        // Create the Applitools Eyes object connected to the runner and set its configuration.
        Eyes = new Eyes(Runner);
        Eyes.SetConfiguration(Config);

        // Open Eyes to start visual testing.
        // It is a recommended practice to set all four inputs:
        Eyes.Open(
            
            // The page to "watch".
            Page,

            // The name of the application under test.
            // All tests for the same app should share the same app name.
            // Set this name wisely: Applitools features rely on a shared app name across tests.
            "ACME Bank Web App",

            // The name of the test case for the given application.
            // Additional unique characteristics of the test may also be specified as part of the test name,
            // such as localization information ("Home Page - EN") or different user permissions ("Login by admin"). 
            TestContext.CurrentContext.Test.Name,

            // The viewport size for the local browser.
            // Eyes will resize the web browser to match the requested viewport size.
            // This parameter is optional but encouraged in order to produce consistent results.
            new RectangleSize(1200, 600));
    }

    [Test]
    public async Task LogIntoBankAccount()
    {
        // This test covers login for the Applitools demo site, which is a dummy banking app.
        // The interactions use typical Selenium WebDriver calls,
        // but the verifications use one-line snapshot calls with Applitools Eyes.
        // If the page ever changes, then Applitools will detect the changes and highlight them in the Eyes Test Manager.
        // Traditional assertions that scrape the page for text values are not needed here.

        // Load the login page.
        await Page.GotoAsync("https://demo.applitools.com");

        // Verify the full login page loaded correctly.
        Eyes.Check(Target.Window().Fully().WithName("Login page"));

        // Perform login.
        await Page.Locator("#username").FillAsync("andy");
        await Page.Locator("#password").FillAsync("i<3pandas");
        await Page.Locator("#log-in").ClickAsync();

        // Verify the full main page loaded correctly.
        // This snapshot uses LAYOUT match level to avoid differences in closing time text.
        Eyes.Check(Target.Window().Fully().WithName("Main page").Layout());
    }

    [TearDown]

    public async Task CleanUpTest()
    {
        // Close Eyes to tell the server it should display the results.
        Eyes.CloseAsync();

        // Close the page.
        await Page.CloseAsync();

        // Warning: `eyes_.CloseAsync()` will NOT wait for visual checkpoints to complete.
        // You will need to check the Eyes Test Manager for visual results per checkpoint.
        // Note that "unresolved" and "failed" visual checkpoints will not cause the JUnit test to fail.

        // If you want the JUnit test to wait synchronously for all checkpoints to complete, then use `eyes_.Close()`.
        // If any checkpoints are unresolved or failed, then `eyes_.Close()` will make the JUnit test fail.
    }

    [OneTimeTearDown]
    public static void PrintResults()
    {
        // Close the Playwright instance.
        Playwright.Dispose();

        // Close the batch and report visual differences to the console.
        // Note that it forces JUnit to wait synchronously for all visual checkpoints to complete.
        TestResultsSummary allTestResults = Runner.GetAllTestResults();
        TestContext.Out.WriteLine(allTestResults);
    }
}
