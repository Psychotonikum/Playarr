using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.EnvironmentInfo;
using Playarr.Core.Configuration;

namespace Playarr.Http.Frontend.Mappers
{
    public class LoginHtmlMapper : HtmlMapperBase
    {
        private readonly IConfigFileProvider _configFileProvider;

        public LoginHtmlMapper(IAppFolderInfo appFolderInfo,
                               IDiskProvider diskProvider,
                               Lazy<ICacheBreakerProvider> cacheBreakProviderFactory,
                               IConfigFileProvider configFileProvider,
                               Logger logger)
            : base(diskProvider, cacheBreakProviderFactory, logger)
        {
            _configFileProvider = configFileProvider;
            HtmlPath = Path.Combine(appFolderInfo.StartUpFolder, configFileProvider.UiFolder, "login.html");
            UrlBase = configFileProvider.UrlBase;
        }

        public override string Map(string resourceUrl)
        {
            return HtmlPath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/login");
        }

        protected override string GetHtmlText(HttpContext context)
        {
            var html = base.GetHtmlText(context);
            var theme = _configFileProvider.Theme;

            html = html.Replace("_THEME_", theme);

            return html;
        }
    }
}
