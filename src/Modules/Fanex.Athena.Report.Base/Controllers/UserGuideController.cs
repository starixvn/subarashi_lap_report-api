using System;
using System.IO;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Core.Utility.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.Base.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UserGuideController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        protected const string DefaultLanguage = "en-US";

        private readonly IContextService _contextService;
        private readonly IFileSystem _fileSystem;

        public UserGuideController(
            IContextService contextService,
            IFileSystem fileSystem, 
            IWebHostEnvironment hostEnvironment)
        {
            _contextService = contextService;
            _fileSystem = fileSystem;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public ActionResult<string> Index(string resourceName)
        {
            return GetFileContent(resourceName);
        }

        private string GetDefaultFilePathIfNotExist(string language, string filePath)
        {
            if (!language.Equals(DefaultLanguage, StringComparison.OrdinalIgnoreCase) && !_fileSystem.Exists(filePath))
            {
                return filePath.Replace($"{language}.htm", $"{DefaultLanguage}.htm");
            }

            return filePath;
        }

        private string GetFileContent(string resourceName)
        {
            var language = _contextService.Language;

            var fullFilePath = Path.GetFullPath(Path.Join(_hostEnvironment.ContentRootPath,
                                                          $@"/App_Data/UserGuide/{resourceName}/{language}.htm"));
            var filePath = GetDefaultFilePathIfNotExist(language, fullFilePath);

            return _fileSystem.ReadFile(filePath);
        }
    }
}