namespace Playarr.Core.CustomFormats
{
    public class ReleaseTitleSpecification : RegexSpecificationBase
    {
        public override int Order => 1;
        public override string ImplementationName => "Release Title";
        public override string InfoLink => "https://wiki.servarr.com/playarr/settings#custom-formats-2";

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return MatchString(input.RomInfo?.ReleaseTitle) || MatchString(input.Filename);
        }
    }
}
