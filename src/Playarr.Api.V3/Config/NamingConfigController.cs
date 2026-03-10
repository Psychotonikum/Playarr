using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Playarr.Common.Extensions;
using Playarr.Core.Organizer;
using Playarr.Http;
using Playarr.Http.REST;
using Playarr.Http.REST.Attributes;

namespace Playarr.Api.V3.Config
{
    [V3ApiController("config/naming")]
    public class NamingConfigController : RestController<NamingConfigResource>
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IFilenameSampleService _filenameSampleService;
        private readonly IFilenameValidationService _filenameValidationService;
        private readonly IBuildFileNames _filenameBuilder;

        public NamingConfigController(INamingConfigService namingConfigService,
                                  IFilenameSampleService filenameSampleService,
                                  IFilenameValidationService filenameValidationService,
                                  IBuildFileNames filenameBuilder)
        {
            _namingConfigService = namingConfigService;
            _filenameSampleService = filenameSampleService;
            _filenameValidationService = filenameValidationService;
            _filenameBuilder = filenameBuilder;

            SharedValidator.RuleFor(c => c.MultiEpisodeStyle).InclusiveBetween(0, 5);
            SharedValidator.RuleFor(c => c.StandardEpisodeFormat).ValidEpisodeFormat();
            SharedValidator.RuleFor(c => c.GameFolderFormat).ValidGameFolderFormat();
            SharedValidator.RuleFor(c => c.PlatformFolderFormat).ValidPlatformFolderFormat();
            SharedValidator.RuleFor(c => c.SpecialsFolderFormat).ValidSpecialsFolderFormat();
            SharedValidator.RuleFor(c => c.CustomColonReplacementFormat).ValidCustomColonReplacement().When(c => c.ColonReplacementFormat == (int)ColonReplacementFormat.Custom);
        }

        protected override NamingConfigResource GetResourceById(int id)
        {
            return GetNamingConfig();
        }

        [HttpGet]
        public NamingConfigResource GetNamingConfig()
        {
            var nameSpec = _namingConfigService.GetConfig();
            var resource = nameSpec.ToResource();

            return resource;
        }

        [RestPutById]
        public ActionResult<NamingConfigResource> UpdateNamingConfig([FromBody] NamingConfigResource resource)
        {
            var nameSpec = resource.ToModel();
            ValidateFormatResult(nameSpec);

            _namingConfigService.Save(nameSpec);

            return Accepted(resource.Id);
        }

        [HttpGet("examples")]
        public object GetExamples([FromQuery]NamingConfigResource settings)
        {
            if (settings.Id == 0)
            {
                settings = GetNamingConfig();
            }

            var nameSpec = settings.ToModel();
            var sampleResource = new NamingExampleResource();

            var singleEpisodeSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
            var multiEpisodeSampleResult = _filenameSampleService.GetMultiEpisodeSample(nameSpec);

            sampleResource.SingleEpisodeExample = _filenameValidationService.ValidateStandardFilename(singleEpisodeSampleResult) != null
                    ? null
                    : singleEpisodeSampleResult.FileName;

            sampleResource.MultiEpisodeExample = _filenameValidationService.ValidateStandardFilename(multiEpisodeSampleResult) != null
                    ? null
                    : multiEpisodeSampleResult.FileName;

            sampleResource.GameFolderExample = nameSpec.GameFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetGameFolderSample(nameSpec);

            sampleResource.PlatformFolderExample = nameSpec.PlatformFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetPlatformFolderSample(nameSpec);

            sampleResource.SpecialsFolderExample = nameSpec.SpecialsFolderFormat.IsNullOrWhiteSpace()
                ? null
                : _filenameSampleService.GetSpecialsFolderSample(nameSpec);

            return sampleResource;
        }

        private void ValidateFormatResult(NamingConfig nameSpec)
        {
            var singleEpisodeSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
            var multiEpisodeSampleResult = _filenameSampleService.GetMultiEpisodeSample(nameSpec);

            var singleEpisodeValidationResult = _filenameValidationService.ValidateStandardFilename(singleEpisodeSampleResult);
            var multiEpisodeValidationResult = _filenameValidationService.ValidateStandardFilename(multiEpisodeSampleResult);

            var validationFailures = new List<ValidationFailure>();

            validationFailures.AddIfNotNull(singleEpisodeValidationResult);
            validationFailures.AddIfNotNull(multiEpisodeValidationResult);

            if (validationFailures.Any())
            {
                throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
            }
        }
    }
}
