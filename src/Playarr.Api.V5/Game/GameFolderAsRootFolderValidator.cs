using FluentValidation.Validators;
using Playarr.Common.Extensions;
using Playarr.Core.Organizer;

namespace Playarr.Api.V5.Game;

public class GameFolderAsRootFolderValidator : PropertyValidator
{
    private readonly IBuildFileNames _fileNameBuilder;

    public GameFolderAsRootFolderValidator(IBuildFileNames fileNameBuilder)
    {
        _fileNameBuilder = fileNameBuilder;
    }

    protected override string GetDefaultMessageTemplate() => "Root folder path '{rootFolderPath}' contains game folder '{seriesFolder}'";

    protected override bool IsValid(PropertyValidatorContext context)
    {
        if (context.PropertyValue == null)
        {
            return true;
        }

        if (context.InstanceToValidate is not GameResource seriesResource)
        {
            return true;
        }

        var rootFolderPath = context.PropertyValue.ToString();

        if (rootFolderPath.IsNullOrWhiteSpace())
        {
            return true;
        }

        var rootFolder = new DirectoryInfo(rootFolderPath!).Name;
        var game = seriesResource.ToModel();
        var seriesFolder = _fileNameBuilder.GetGameFolder(game);

        context.MessageFormatter.AppendArgument("rootFolderPath", rootFolderPath);
        context.MessageFormatter.AppendArgument("seriesFolder", seriesFolder);

        if (seriesFolder == rootFolder)
        {
            return false;
        }

        var distance = seriesFolder.LevenshteinDistance(rootFolder);

        return distance >= Math.Max(1, seriesFolder.Length * 0.2);
    }
}
