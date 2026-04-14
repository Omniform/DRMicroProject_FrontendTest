using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Xunit;
using Xunit.Abstractions;

namespace DRMicroProject_FrontendTest
{
    public class SeleniumTestGet : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly ITestOutputHelper _output;

        public SeleniumTestGet(ITestOutputHelper output)
        {
            _output = output;

            // Create Firefox driver service pointing to the folder that contains geckodriver.
            // Use "geckodriver" or "geckodriver.exe" depending on your file name.
            var service = FirefoxDriverService.CreateDefaultService(@"C:\webDrivers", "geckodriver.exe");
            service.HideCommandPromptWindow = true;

            var options = new FirefoxOptions();
            options.AcceptInsecureCertificates = true;

            _driver = new FirefoxDriver(service, options, TimeSpan.FromSeconds(60));
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        // Simple record type for expected albums
        private sealed record Album(int Id, string Title, string Artist, int LengthSeconds, int Year);

        [Fact]
        public void AlbumsAreDisplayedOnPage()
        {
            // Configure the URL of the app under test via environment or change the default below.
            var baseUrl = Environment.GetEnvironmentVariable("C:\\Users\\Bruger\\source\\repos\\DRMicroProject_Frontend");
            _output.WriteLine($"Navigating to: {baseUrl}");
            _driver.Navigate().GoToUrl(baseUrl);

            var expectedRecords = new List<Album>
            {
                new Album(1, "Bohemian Rhapsody", "Queen", 354, 1975),
                new Album(2, "Imagine", "John Lennon", 183, 1971),
                new Album(3, "Stairway to Heaven", "Led Zeppelin", 482, 1971)
            };

            foreach (var album in expectedRecords)
            {
                // Build an XPath that looks for any element containing all four pieces of text.
                // This is tolerant to different DOM structures (rows, cards, etc.) as long as the texts
                // are visible within the same containing element.
                var xpath =
                    $"//*[contains(normalize-space(.), '{EscapeQuotes(album.Title)}') and contains(normalize-space(.), '{EscapeQuotes(album.Artist)}') and contains(normalize-space(.), '{album.LengthSeconds}') and contains(normalize-space(.), '{album.Year}')]";

                var found = _driver.FindElements(By.XPath(xpath));
                var message = $"Album not found: Title='{album.Title}', Artist='{album.Artist}', Length='{album.LengthSeconds}', Year='{album.Year}'";
                Assert.True(found.Count > 0, message);
                _output.WriteLine($"Found album: {album.Title} (matches: {found.Count})");
            }
        }

        // Helper to escape single quotes inside XPath string when needed (simple replacement).
        private static string EscapeQuotes(string input) =>
            input?.Replace("'", "\"") ?? string.Empty;

        public void Dispose()
        {
            try
            {
                _driver?.Quit();
            }
            catch
            {
                // swallow exceptions on cleanup
            }
            finally
            {
                _driver?.Dispose();
            }
        }
    }
}