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

    // Runner Settings.
    // These could be set by environment variables or other input mechanisms.
    // They are hard-coded here to keep the example project simple.
    private static readonly bool USE_ULTRAFAST_GRID = true;

    // Test control inputs to read once and share for all tests
    private static string applitoolsApiKey_;
    private static bool headless_;

    // Applitools objects to share for all tests
    private static BatchInfo batch_;
    private static Configuration config_;
    private static PlaywrightEyesRunner runner_;

    // Test-specific objects
    private static IPlaywright playwright_;
    private static IBrowser browser_;
    private IBrowserContext context_;
    private IPage page_;
    private Eyes eyes_;

    [SetUp]
    public void Setup()
    {
    }

    [OneTimeSetUp]
    public static async Task SetUpConfigAndRunner()
    {
        // This method sets up the configuration for running visual tests.
        // The configuration is shared by all tests in a test suite, so it belongs in a `BeforeAll` method.
        // If you have more than one test class, then you should abstract this configuration to avoid duplication.

        // Read the Applitools API key from an environment variable.
        applitoolsApiKey_ = Environment.GetEnvironmentVariable("APPLITOOLS_API_KEY");

        // Read the headless mode setting from an environment variable.
        // Use headless mode for Continuous Integration (CI) execution.
        // Use headed mode for local development.
        headless_ = bool.Parse(Environment.GetEnvironmentVariable("HEADLESS") ?? "true");

        if (USE_ULTRAFAST_GRID)
        {
            // Create the runner for the Ultrafast Grid.
            // Concurrency refers to the number of visual checkpoints Applitools will perform in parallel.
            // Warning: If you have a free account, then concurrency will be limited to 1.
            runner_ = new VisualGridRunner(new RunnerOptions().TestConcurrency(5));
        }
        else
        {
            // Create the Classic runner.
            runner_ = new ClassicRunner();
        }

        // Create a new batch for tests.
        // A batch is the collection of visual checkpoints for a test suite.
        // Batches are displayed in the Eyes Test Manager, so use meaningful names.
        string runnerName = (USE_ULTRAFAST_GRID) ? "Ultrafast Grid" : "Classic runner";
        batch_ = new BatchInfo("Example: Playwright C# NUnit with the " + runnerName);

        // Create a configuration for Applitools Eyes.
        config_ = new Configuration();

        // Set the Applitools API key so test results are uploaded to your account.
        // If you don't explicitly set the API key with this call,
        // then the SDK will automatically read the `APPLITOOLS_API_KEY` environment variable to fetch it.
        config_.SetApiKey(applitoolsApiKey_);

        // Set the batch for the config.
        config_.SetBatch(batch_);

        // If running tests on the Ultrafast Grid, configure browsers.
        if (USE_ULTRAFAST_GRID)
        {
            // Add 3 desktop browsers with different viewports for cross-browser testing in the Ultrafast Grid.
            // Other browsers are also available, like Edge and IE.
            config_.AddBrowser(800, 600, BrowserType.CHROME);
            config_.AddBrowser(1600, 1200, BrowserType.FIREFOX);
            config_.AddBrowser(1024, 768, BrowserType.SAFARI);

            // Add 2 mobile emulation devices with different orientations for cross-browser testing in the Ultrafast Grid.
            // Other mobile devices are available, including iOS.
            config_.AddDeviceEmulation(DeviceName.Pixel_2, ScreenOrientation.Portrait);
            config_.AddDeviceEmulation(DeviceName.Nexus_10, ScreenOrientation.Landscape);
        }

        // Start Playwright and launch the browser.
        playwright_ = await Playwright.CreateAsync();
        browser_ = await playwright_.Chromium
            .LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless_ });
    }

    [SetUp]
    public async Task OpenBrowserAndEyes()
    {
        // This method sets up each test with its own Page and Applitools Eyes objects.

        // Get a new context from the browser
        context_ = await browser_.NewContextAsync();

        // Create a new page in the context.
        // Creating a new context is not mandatory and a new page can be created from the browser instance.
        // page_ = await browser_.NewPageAsync();
        page_ = await context_.NewPageAsync();

        // Create the Applitools Eyes object connected to the runner and set its configuration.
        eyes_ = new Eyes(runner_);
        eyes_.SetConfiguration(config_);

        // Open Eyes to start visual testing.
        // It is a recommended practice to set all four inputs:
        eyes_.Open(
            // The page to "watch".
            page_,

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
            new RectangleSize(1024, 768));
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
        await page_.GotoAsync("https://demo.applitools.com");

        // Verify the full login page loaded correctly.
        eyes_.Check(Target.Window().Fully().WithName("Login page"));

        // Perform login.
        await page_.Locator("#username").FillAsync("andy");
        await page_.Locator("#password").FillAsync("i<3pandas");
        await page_.Locator("#log-in").ClickAsync();

        // Verify the full main page loaded correctly.
        // This snapshot uses LAYOUT match level to avoid differences in closing time text.
        eyes_.Check(Target.Window().Fully().WithName("Main page").Layout());
    }

    [TearDown]

    public async Task CleanUpTest()
    {
        // Close Eyes to tell the server it should display the results.
        eyes_.CloseAsync();

        // Close the page.
        await page_.CloseAsync();

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
        playwright_.Dispose();

        // Close the batch and report visual differences to the console.
        // Note that it forces JUnit to wait synchronously for all visual checkpoints to complete.
        TestResultsSummary allTestResults = runner_.GetAllTestResults();
        TestContext.Out.WriteLine(allTestResults);
    }
}