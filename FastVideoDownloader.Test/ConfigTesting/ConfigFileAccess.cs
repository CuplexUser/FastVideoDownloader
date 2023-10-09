using FastVideoDownloader.Models;
using FastVideoDownloader.Service;
using FastVideoDownloader.Test.TestHelpers;

namespace FastVideoDownloader.Test.ConfigTesting
{
    [TestClass]
    public class ConfigFileAccess
    {
        private static TestContext _context = null;
        private string _filePath;

        public ConfigFileAccess()
        {

        }

        [ClassInitialize]
        public static void TestClassInit(TestContext context)
        {
            _context = context;

        }

        [ClassCleanup]
        public static void TestClassCleanup()
        {
            _context = null;
        }

        [TestInitialize]
        public void InitTestData()
        {
            _filePath = Path.Join(_context.TestRunDirectory, "testConfig.json");
            var settings = ConfigFileHelper.GetDefaultSettings();

            // Write Json File
            ConfigFileHelper.SaveSettingsFile(settings, _filePath);
        }


        [TestMethod]
        public void LoadConfigTest()
        {
            AppSettingsService appSettingsService = new AppSettingsService(_filePath);
            AppSettings settings = LoadAppSettings(appSettingsService);
            var origninalSettings = ConfigFileHelper.GetDefaultSettings();

            Assert.IsNotNull(settings, "LoadAppSettings returned null");

            AppSettingsComparer comparer = new AppSettingsComparer();

            Assert.IsTrue(comparer.Equals(settings, origninalSettings), "The Settings loaded from the config file does not match the saved settings");

        }

        [TestMethod]
        public void ConfigFileChangedTest()
        {

        }

        private AppSettings LoadAppSettings(AppSettingsService appSettingsService)
        {
            var LoadSettingsTask = Task.Run(appSettingsService.LoadAppSettingsAsync);
            return LoadSettingsTask.GetAwaiter().GetResult();
        }
    }
}
