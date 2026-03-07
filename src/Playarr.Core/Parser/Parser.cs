using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using Playarr.Common.Extensions;
using Playarr.Common.Instrumentation;
using Playarr.Core.MediaFiles;
using Playarr.Core.Parser.Model;
using Playarr.Core.Games;

namespace Playarr.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger = PlayarrLogger.GetLogger(typeof(Parser));

        private static readonly Regex[] ReportTitleRegex = new[]
            {
                // Anime - Absolute Rom Number + Title + Platform+Rom
                // Todo: This currently breaks game that start with numbers
//                new Regex(@"^(?:(?<absoluteepisode>\d{2,3})(?:_|-|\s|\.)+)+(?<title>.+?)(?:\W|_)+(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<rom>\d{2}(?!\d+)))+)",
//                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Daily rom with year in game title and air time after date (Plex DVR format)
                new Regex(@"^^(?<title>.+?\((?<titleyear>\d{4})\))[-_. ]+(?<airyear>19[4-9]\d|20\d\d)(?<sep>[-_]?)(?<airmonth>0\d|1[0-2])\k<sep>(?<airday>[0-2]\d|3[01])[-_. ]\d{2}[-_. ]\d{2}[-_. ]\d{2}",
                RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Daily roms without title (2018-10-12, 20181012) (Strict pattern to avoid false matches)
                new Regex(@"^(?<airyear>19[6-9]\d|20\d\d)(?<sep>[-_]?)(?<airmonth>0\d|1[0-2])\k<sep>(?<airday>[0-2]\d|3[01])(?!\d)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-Part roms without a title (S01E05.S01E06)
                new Regex(@"^(?:\W*S(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:e{1,2}(?<rom>\d{1,3}(?!\d+)))+){2,}",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-Part roms without a title (1x05.1x06)
                new Regex(@"^(?:\W*(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:x{1,2}(?<rom>\d{1,3}(?!\d+)))+){2,}",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms without a title, Multi (S01E04E05, 1x04x05, etc)
                new Regex(@"^(?:S?(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[-_]|[ex]){1,2}(?<rom>\d{2,3}(?!\d+))){2,})",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Split roms (S01E05a, S01E05b, etc)
                new Regex(@"^(?<title>.+?)(?:S?(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[-_ ]?[ex])(?<rom>\d{2,3}(?!\d+))(?<splitepisode>[a-d])(?:[ _.])))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms without a title, Single (S01E05, 1x05)
                new Regex(@"^(?:S?(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[-_ ]?[ex])(?<rom>\d{2,3}(?!\d+))))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Absolute (Platform+Rom)
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+))(?:[-_. ])+\((?:S(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]|\W[ex]){1,2}(?<rom>\d{2}(?!\d+))))(?:v\d+)?(?:\)(?!\d+)).*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Absolute - Platform+Rom
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+))(?:[-_. ](?<![()\[!]))+(?:S(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]|\W[ex]){1,2}(?<rom>\d{2}(?!\d+))))(?:v\d+)?(?:[_. ](?!\d+)).*?(?<hash>[(\[]\w{8}[)\]])$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Platform+Rom
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)(?:[-_\W](?<![()\[!]))+(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]|\W[ex]){1,2}(?<rom>\d{2}(?!\d+)))+)(?:v\d+)?(?:[_. ](?!\d+)).*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Rom Absolute Rom Number ([SubGroup] Game Title Rom 01)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>.+?)[-_. ]+?(?:Rom)(?:[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+)))+.*?(?<hash>[(\[]\w{8}[)\]])?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Absolute Rom Number + Platform+Rom
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+(?<absoluteepisode>\d{2,3}(\.\d{1,2})?))+(?:_|-|\s|\.)+(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<rom>\d{2}(?!\d+)))+).*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Platform+Rom + Absolute Rom Number
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.)?)(?<title>.+?)(?:[-_\W](?<![()\[!]))+(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<rom>\d{2}(?!\d+)))+)(?:(?:_|-|\s|\.)+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+|\-[a-z])))+.*?(?<hash>[(\[]\w{8}[)\]])?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title with trailing number Absolute Rom Number - Batch separated with tilde
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?[^-]+?)(?:(?<![-_. ]|\b[0]\d+) - )[-_. ]?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+))\s?~\s?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+))(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title with platform number in brackets Absolute Rom Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>[^-]+?)[_. ]+?\(Platform[_. ](?<platform>\d+)\)[-_. ]+?(?:[-_. ]?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title with trailing 3-digit number and sub title - Absolute Rom Number
                new Regex(@"^\[(?<subgroup>[^\]]+?)\][-_. ]?(?<title>[^]]+?)(?:[-_. ]{3}?(?<absoluteepisode>\d{2}(\.\d{1,2})?(?!-?\d+|-[a-z]+)))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title with trailing number Absolute Rom Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>[^-]+?)(?:(?<![-_. ]|\b[0]\d+) - )(?:[-_. ]?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title with trailing number S## (Full platform)
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)[-_. ]+(?:S(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?![ex]?\d+))).+?(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title with trailing number Absolute Rom Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>[^-]+?)(?:(?<![-_. ]|\b[0]\d+)[_ ]+)(?:[-_. ]?(?<absoluteepisode>\d{3}(\.\d{1,2})?(?!\d+|-[a-z]+)))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title - Absolute Rom Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)(?:(?<!\b[0]\d+))(?:[. ]-[. ](?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-])))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Absolute Rom Number - Absolute Rom Number (batches without full separator between title and absolute rom numbers)
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)(?:(?<!\b[0]\d+))(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-]))[. ]-[. ](?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-]))(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - [SubGroup] Title Absolute Rom Number
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)[-_. ]+\(?(?:[-_. ]?#?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|-[a-z]+)))+\)?(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom Repeated (S01E05 - S01E06)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:e|[-_. ]e){1,2}(?<rom>\d{1,3}(?!\d+)))+){2,}",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom Repeated (1x05 - 1x06)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:x{1,2}(?<rom>\d{1,3}(?!\d+)))+){2,}",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Single roms with a title (S01E05, 1x05, etc) followed by ".5 [SP]"
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]|_){1,2}(?<rom>\d{2,3}(?!\d+|(?:[ex]|\W[ex]|_|-){1,2}\d+))(?<special>\.5[ .]\[SP\]))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Single roms with a title (S01E05, 1x05, etc) and trailing info in slashes
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]|_){1,2}(?<rom>\d{2,3}(?!\d+|(?:[ex]|\W[ex]|_|-){1,2}\d+))).+?(?:\[.+?\])(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Platform EpisodeNumber + Absolute Rom Number [SubGroup]
                new Regex(@"^(?<title>.+?)(?:[-_\W](?<![()\[!]))+(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]|\W[ex]|-){1,2}(?<rom>(?<!\d+)\d{2}(?!\d+)))+)[-_. (]+?(?:[-_. ]?(?<absoluteepisode>(?<!\d+)\d{3}(\.\d{1,2})?(?!\d+|[pi])))+.+?\[(?<subgroup>.+?)\](?:$|\.mkv)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-Rom with a title (S01E05E06, S01E05-06, S01E05 E06, etc) and trailing info in slashes
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]|_){1,2}(?<rom>\d{2,3}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<rom>\d{2,3}(?!\d+)))+).+?(?:\[.+?\])(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Absolute Rom Number [SubGroup] [Hash]? (Game Title Rom 99-100 [RlsGroup] [ABCD1234])
                new Regex(@"^(?<title>.+?)[-_. ]Rom(?:[-_. ]+(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:.+?)\[(?<subgroup>.+?)\].*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Absolute Rom Number [SubGroup] [Hash]
                new Regex(@"^(?<title>.+?)(?:(?:_|-|\s|\.)+(?<absoluteepisode>\d{3}(\.\d{1,2})(?!\d+)))+(?:.+?)\[(?<subgroup>.+?)\].*?(?<hash>[(\[]\w{8}[)\]])?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Absolute Rom Number (Year) [SubGroup]
                new Regex(@"^(?<title>.+?)[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2}(?!\d+))[-_. ](\(\d{4}\))[-_. ]\[(?<subgroup>.+?)\]",
                            RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title with trailing number, Absolute Rom Number and hash
                new Regex(@"^(?<title>[^-]+?)(?:(?<![-_. ]|\b[0]\d+) - )(?:[-_. ]?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:[-_. ]+(?<special>special|ova|ovd))?.*?(?<hash>[(\[]\w{8}[)\]])(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Absolute Rom Number [Hash]
                new Regex(@"^(?<title>.+?)(?:(?:_|-|\s|\.)+(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+)))+(?:[-_. ]+(?<special>special|ova|ovd))?[-_. ]+.*?(?<hash>[(\[]\w{8}[)\]])$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with airdate AND platform/rom number, capture platform/rom only
                new Regex(@"^(?<title>.+?)?\W*(?<airdate>\d{4}\W+[0-1][0-9]\W+[0-3][0-9])(?!\W+[0-3][0-9])[-_. ](?:s?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+)))(?:[ex](?<rom>(?<!\d+)(?:\d{1,3})(?!\d+)))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with airdate AND platform/rom number
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})\W+(?<airmonth>[0-1][0-9])\W+(?<airday>[0-3][0-9])(?!\W+[0-3][0-9]).+?(?:s?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+)))(?:[ex](?<rom>(?<!\d+)(?:\d{1,3})(?!\d+)))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with absolute rom number AND airdate (TJET Wrestling)
                new Regex(@"^(?<title>.+?)?[-_. ](?:e\d{2,3}(?!\d+))[-_. ](?<airyear>\d{4})\W+(?<airmonth>[0-1][0-9])\W+(?<airday>[0-3][0-9])(?!\W+[0-3][0-9]).+?(?:\[[a-z]+\])",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Single or multi rom releases with multiple titles, then platform and rom numbers after the last title. (Title1 / Title2 / ... / S1E1-2 of 6)
                new Regex(@"^((?<title>.*?)[ ._]\/[ ._])+\(?S(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:\W|_)?E?[ ._]?(?<rom>(?<!\d+)\d{1,2}(?!\d+))(?:-(?<rom>(?<!\d+)\d{1,2}(?!\d+)))?(?:[ ._]of[ ._](?<episodecount>\d{1,2}))?\)?[ ._][\(\[]",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom with title (S01E99-100, S01E05-06)
                new Regex(@"^(?<title>.+?)(?:[-_\W](?<![()\[!]))+S(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))E(?<rom>\d{2,3}(?!\d+))(?:-(?<rom>\d{2,3}(?!\d+)))+(?:[-_. ]|$)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom with title (S01E05-06, S01E05-6)
                new Regex(@"^(?<title>.+?)(?:[-_\W](?<![()\[!]))+S(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))E(?<rom>\d{1,2}(?!\d+))(?:-(?<rom>\d{1,2}(?!\d+)))+(?:[-_. ]|$)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with a Chinese title and English title, Single roms (S01E05, 1x05, etc) & Multi-rom (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>[\u4E00-\u9FCC]+)[_. ](?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]){1,2}(?<rom>\d{2,3}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<rom>\d{2,3}(?!\d+)))*)(?:[-_. ]|$)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with a title, Single roms (S01E05, 1x05, etc) & Multi-rom (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[ex]|\W[ex]){1,2}(?<rom>\d{2,3}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<rom>\d{2,3}(?!\d+)))*)(?:[-_. ]|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with a title, 4 digit platform number, Single roms (S2016E05, etc) & Multi-rom (S2016E05E06, S2016E05-06, S2016E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+S(?<platform>(?<!\d+)(?:\d{4})(?!\d+))(?:e|\We|_){1,2}(?<rom>\d{2,4}(?!\d+))(?:(?:\-|e|\We|_){1,2}(?<rom>\d{2,3}(?!\d+)))*)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with a title, 4 digit platform number, Single roms (2016x05, etc) & Multi-rom (2016x05x06, 2016x05-06, 2016x05 x06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+(?<platform>(?<!\d+)(?:\d{4})(?!\d+))(?:x|\Wx){1,2}(?<rom>\d{2,4}(?!\d+))(?:(?:\-|x|\Wx|_){1,2}(?<rom>\d{2,3}(?!\d+)))*)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with a title, Single roms (s01.05)
                new Regex(@"^(?<title>.+?)(?:[-_\W](?<![()\[!]))+S(?<platform>(?<!\d+)(?:\d{2})(?!\d+))(?:\.)(?<rom>\d{2,3}(?!\d+))(?:[-_. ]|$)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-platform pack
                new Regex(@"^(?<title>.+?)(Complete Game)?[-_. ]+(?:S|(?:Platform|Saison|Game|Stagione)[_. ])(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))(?:[-_. ]{1}|[-_. ]{3})(?:S|(?:Platform|Saison|Game|Stagione)[_. ])?(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Partial platform pack
                new Regex(@"^(?<title>.+?)(?:\W+S(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))\W+(?:(?:(?:Part|Vol)\W?|(?<!\d+\W+)e|p)(?<seasonpart>\d{1,2}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - 4 digit absolute rom number in batch (1017-1088) or 1017-1088
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>.+?)[-_. ]+?\(?(?<absoluteepisode>\d{4}(\.\d{1,2})?(?!\d+))[-](?<absoluteepisode>\d{4}(\.\d{1,2})?(?!\d+))\)?",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - 4 digit absolute rom number
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>.+?)[-_. ]+?(?<absoluteepisode>\d{4}(\.\d{1,2})?(?!\d+))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title 4-digit Absolute Rom Number [SubGroup]
                new Regex(@"^(?<title>.+?)[-_. ]+(?<absoluteepisode>(?<!\d+)\d{4}(?!\d+))[-_. ]\[(?<subgroup>.+?)\]",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Mini-Game with year in title, treated as platform 1, roms are labelled as Part01, Part 01, Part.1
                new Regex(@"^(?<title>.+?\d{4})(?:\W+(?:(?:Part\W?|e)(?<rom>\d{1,2}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Mini-Game, treated as platform 1, multi roms are labelled as E1-E2
                new Regex(@"^(?<title>.+?)(?:[-._ ][e])(?<rom>\d{2,3}(?!\d+))(?:(?:\-?[e])(?<rom>\d{2,3}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with airdate and part (2018.04.28.Part.2)
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])(?![-_. ]+[0-3][0-9])[-_. ]+Part[-_. ]?(?<part>[1-9])",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // 3-digit platform number
                // Single rom: Title.S001E01
                // Multi rom: Title.S001E01-E02
                new Regex(@"^(?<title>.+?)[-_. ]+S(?<platform>(?<!\d+)\d{3}(?!\d+))E(?<rom>(?<!\d+)\d{2}(?!\d+))(?:-[ex](?<rom>(?<!\d+)\d{2}(?!\d+)))?([_. ]|$)(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Mini-Game, treated as platform 1, roms are labelled as XofY
                new Regex(@"^(?<title>.+?)(?:\W+(?:(?<rom>(?<!\d+)\d{1,2}(?!\d+))of\d+)+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Supports Platform 01 Rom 03
                new Regex(@"(?:.*(?:\""|^))(?<title>.*?)(?:[-_\W](?<![()\[]))+(?:\W?Platform\W?)(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:\W|_)+(?:Rom\W)(?:[-_. ]?(?<rom>(?<!\d+)\d{1,2}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom with roms in square brackets (Game Title [S01E11E12] or Game Title [S01E11-12])
                new Regex(@"(?:.*(?:^))(?<title>.*?)[-._ ]+\[S(?<platform>(?<!\d+)\d{2}(?!\d+))(?:[E-]{1,2}(?<rom>(?<!\d+)\d{2}(?!\d+)))+\]",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom with roms in brackets (Game Title (S01E11E12) or Game Title (S01E11-12) or Game Title (S01E1-3 of 12))
                new Regex(@"(?:.*(?:^))(?<title>.*?)[-._ ]+\(S(?<platform>(?<!\d+)\d{2}(?!\d+))(?:[E-]{1,2}(?<rom>(?<!\d+)\d{1,2}(?!\d+)))+([._ ]+of[._ ]+[\dx]+)?\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom release with no space between game title and platform (S01E11E12)
                new Regex(@"(?:.*(?:^))(?<title>.*?)S(?<platform>(?<!\d+)\d{2}(?!\d+))(?:E(?<rom>(?<!\d+)\d{2}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom with single rom numbers (S6.E1-E2, S6.E1E2, S6E1E2, etc)
                new Regex(@"^(?<title>.+?)[-_. ]S(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:[-_. ]?[ex]?(?<rom>(?<!\d+)\d{1,2}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Single or multi rom releases with multiple titles, each followed by platform and rom numbers in brackets
                new Regex(@"^(?<title>.*?)[ ._]\(S(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:\W|_)?E?[ ._]?(?<rom>(?<!\d+)\d{1,2}(?!\d+))(?:-(?<rom>(?<!\d+)\d{1,2}(?!\d+)))?\)(?:[ ._]\/[ ._])(?<title>.*?)[ ._]\(",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Multi-rom with roms in brackets (Game Title (S01E11E12) or Game Title (S1E11-12) or Game Title (S1E1-3 of 12))
                new Regex(@"(?:.*(?:^))(?<title>.*?)[-._ ]+\(S(?<platform>(?<!\d+)\d{1}(?!\d+))(?:[E-]{1,2}(?<rom>(?<!\d+)\d{1,2}(?!\d+)))+([._ ]+of[._ ]+[\dx]+)?\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Single rom platform or rom S1E1 or S1-E1 or S1.Ep1 or S01.Ep.01
                new Regex(@"(?:.*(?:\""|^))(?<title>.*?)(?:\W?|_)S(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:\W|_)?Ep?[ ._]?(?<rom>(?<!\d+)\d{1,2}(?!\d+))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // 3 digit platform S010E05
                new Regex(@"(?:.*(?:\""|^))(?<title>.*?)(?:\W?|_)S(?<platform>(?<!\d+)\d{3}(?!\d+))(?:\W|_)?E(?<rom>(?<!\d+)\d{1,2}(?!\d+))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // 5 digit rom number with a title
                new Regex(@"^(?:(?<title>.+?)(?:_|-|\s|\.)+)(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+)))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<rom>(?<!\d+)\d{5}(?!\d+)))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // 5 digit multi-rom with a title
                new Regex(@"^(?:(?<title>.+?)(?:_|-|\s|\.)+)(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+)))(?:(?:[-_. ]{1,3}ep){1,2}(?<rom>(?<!\d+)\d{5}(?!\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Separated platform and rom numbers S01 - E01
                new Regex(@"^(?<title>.+?)(?:_|-|\s|\.)+S(?<platform>\d{2}(?!\d+))(\W-\W)E(?<rom>(?<!\d+)\d{2}(?!\d+))(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Platform and rom numbers in square brackets (single and multi-rom)
                // Game Title - [02x01] - Rom 1
                // Game Title - [02x01x02] - Rom 1
                new Regex(@"^(?<title>.+?)?(?:[-_\W](?<![()\[!]))+\[(?:s)?(?<platform>(?<!\d+)\d{1,2})(?:(?:[ex])(?<rom>\d{2}))(?:(?:[-ex]){1,2}(?<rom>\d{2}))*\].+?(?:\.|$)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title with platform number - Absolute Rom Number (Title S01 - EP14)
                new Regex(@"^(?<title>.+?S\d{1,2})[-_. ]{3,}(?:EP)?(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+|[-]))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - French titles with single rom numbers, with or without leading sub group ([RlsGroup] Title - Rom 1)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)[-_. ]+?(?:Rom[-_. ]+?)(?<absoluteepisode>\d{1}(\.\d{1,2})?(?!\d+))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Absolute rom number in square brackets
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)(?<title>.+?)[-_. ]+?\[(?<absoluteepisode>\d{2,3}(\.\d{1,2})?(?!\d+))\]",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Japanese variety shows with leading date
                new Regex(@"^(?<airyear>\d{2})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9])(?![-_. ]+[0-3][0-9])[-_. ](?<title>.+?)[-_. ](?:Platform[-_. ]?(?<platform>\d{1,2})[-_. ])?(?:ep|#)(?<rom>\d{2,3})",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Platform only releases followed by year
                new Regex(@"^(?<title>.+?)[-_. ]+?(?:S|Platform|Saison|Game|Stagione)[-_. ]?(?<platform>\d{1,2}(?=[-_. ]\d{4}[-_. ]+))(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Platform only releases
                new Regex(@"^(?<title>.+?)[-_. ]+?(?:S|Platform|Saison|Game|Stagione)[-_. ]?(?<platform>\d{1,2}(?![-_. ]?\d+))(?:[-_. ]|$)+(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // 4 digit platform only releases
                new Regex(@"^(?<title>.+?)[-_. ]+?(?:S|Platform|Saison|Game|Stagione)[-_. ]?(?<platform>\d{4}(?![-_. ]?\d+))(\W+|_|$)(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Spanish tracker releases
                new Regex(@"^(?<title>.+?)(?:(?:[-_. ]+?Temporada.+?|\[.+?\])\[Cap)(?:[-_. ]+(?<platform>(?<!\d+)\d{1,2})(?<rom>(?<!e|x)(?:[1-9][0-9]|[0][1-9])))+(?:\])",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Supports 103/113 naming
                new Regex(@"^(?<title>.+?)?(?:(?:[_.-](?<![()\[!]))+(?<platform>(?<!\d+)[1-9])(?<rom>[1-9][0-9]|[0][1-9])(?![a-z]|\d+))+(?:[_.]|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // 4 digit rom number
                // Roms without a title, Single (S01E05, 1x05) AND Multi (S01E04E05, 1x04x05, etc)
                new Regex(@"^(?:S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<rom>\d{4}(?!\d+|i|p)))+)(\W+|_|$)(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // 4 digit rom number
                // Roms with a title, Single roms (S01E05, 1x05, etc) & Multi-rom (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(?:[-_\W](?<![()\[!]|\d{1,2}-))+S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<rom>\d{4}(?!\d+|i|p)))+)\W?(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with airdate (2018.04.28)
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})[-_. ]+(?<airmonth>[0-1][0-9])[-_. ]+(?<airday>[0-3][0-9])(?![-_. ]+[0-3][0-9])",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Turkish tracker releases (01 BLM, 3. Blm, 04.Bolum, etc)
                new Regex(@"^(?<title>.+?)[_. ](?<absoluteepisode>\d{1,4})(?:[_. ]+)(?:BLM|B[oö]l[uü]m)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with airdate (04.28.2018)
                new Regex(@"^(?<title>.+?)?\W*(?<ambiguousairmonth>[0-1][0-9])[-_. ]+(?<ambiguousairday>[0-3][0-9])[-_. ]+(?<airyear>\d{4})(?!\d+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with airdate (28.04.2018)
                new Regex(@"^(?<title>.+?)?\W*(?<ambiguousairday>[0-3][0-9])[-_. ]+(?<ambiguousairmonth>[0-1][0-9])[-_. ]+(?<airyear>\d{4})(?!\d+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with airdate (20180428)
                new Regex(@"^(?<title>.+?)?\W*(?<!\d+)(?<airyear>\d{4})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9])(?!\d+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Absolute Rom Number (E195 or E1206)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:(?:_|-|\s|\.)+(?:e|ep)(?<absoluteepisode>(\d{3}|\d{4})(\.\d{1,2})?))+[-_. ].*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Daily roms that use short month format instead of number
                new Regex(@"^(?<title>.+?)[-_. ]+(?<airday>[1-2]\d|3[01]|[1-9])(?:th|st|rd)[-_. ](?<shortairmonth>jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)[-_. ](?<airyear>19[4-9]\d|20\d\d)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Supports 1103/1113 naming
                new Regex(@"^(?<title>.+?)?(?:(?:[-_. ](?<![()\[!]))*(?<!\d{1,2}-)(?<platform>(?<!\d+|\(|\[|e|x)\d{2})(?<rom>(?<!e|x)(?:[1-9][0-9]|[0][1-9])(?!p|i|\d+|\)|\]|\W\d+|\W(?:e|ep|x)\d+)))+([-_. ]+|$)(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Dutch/Flemish release titles
                new Regex(@"^(?<title>.+?)[-_. ](?:Se\.(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[-_ ]?afl\.)(?<rom>\d{1,3}(?!\d+))(?:(?:[-]|[-_ ]en[-_ ])(?<rom>\d{1,3}(?!\d+)))*))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms with single digit rom number (S01E1, S01E5E6, etc)
                new Regex(@"^(?<title>.*?)(?:(?:[-_\W](?<![()\[!]))+S?(?<platform>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]){1,2}(?<rom>\d{1}))+)+(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // iTunes Platform 1\05 Title (Quality).ext
                new Regex(@"^(?:Platform(?:_|-|\s|\.)(?<platform>(?<!\d+)\d{1,2}(?!\d+)))(?:_|-|\s|\.)(?<rom>(?<!\d+)\d{1,2}(?!\d+))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // iTunes 1-05 Title (Quality).ext
                new Regex(@"^(?:(?<platform>(?<!\d+)(?:\d{1,2})(?!\d+))(?:-(?<rom>\d{2,3}(?!\d+))))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Mini-Game, treated as platform 1, roms are labelled as Part01, Part 01, Part.1
                new Regex(@"^(?<title>.+?)(?:\W+(?:(?:(?<!\()Part\W?|(?<!\d+\W+)e)(?<rom>\d{1,2}(?!\d+|\))))+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Mini-Game, treated as platform 1, roms are labelled as Part One/Two/Three/...Nine, Part.One, Part_One
                new Regex(@"^(?<title>.+?)(?:\W+(?:Part[-._ ](?<rom>One|Two|Three|Four|Five|Six|Seven|Eight|Nine)(?>[-._ ])))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime Range - Title Absolute Rom Number (ep01-12)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:_|\s|\.)+(?:e|ep)(?<absoluteepisode>\d{2,3}(\.\d{1,2})?)-(?<absoluteepisode>(?<!\d+)\d{1,2}(\.\d{1,2})?(?!\d+|-)).*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Absolute Rom Number (e66)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:(?:_|-|\s|\.)+(?:e|ep)(?<absoluteepisode>\d{2,4}(\.\d{1,2})?))+[-_. ].*?(?<hash>[(\[]\w{8}[)\]])?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Rom Absolute Rom Number (Game Title Rom 01)
                new Regex(@"^(?<title>.+?)[-_. ](?:Rom)(?:[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+)))+.*?(?<hash>[(\[]\w{8}[)\]])?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime Range - Title Absolute Rom Number (1 or 2 digit absolute rom numbers in a range, 1-10)
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)[_. ]+(?<absoluteepisode>(?<!\d+)\d{1,2}(\.\d{1,2})?(?!\d+))-(?<absoluteepisode>(?<!\d+)\d{1,2}(\.\d{1,2})?(?!\d+|-)).*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Rom/Episodio Absolute Rom Number
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)[-_. ]+(?:Rom|Episodio)(?:[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,4}(\.\d{1,2})?(?!\d+|[ip])))+.*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title [Absolute Rom Number] from AniLibriaTV
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:[-_. ]\[)(?:(?:-?)(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+|[ip])))+(?:\][-_. ]).*?(?<hash>[(\[]\w{8}[)\]])?$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Simple platform releases in the format 'Game (year) - Temporada platform'
                new Regex(@"^(?<title>.+?)[_. ]-[_. ](?:Temporada)[-_. ]?(?<platform>\d{1,2}(?![_. ]?\d+))(?:[_. ]|$)+(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title Absolute Rom Number
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:[-_. ]+(?<absoluteepisode>(?<!\d+)\d{2,4}(\.\d{1,2})?(?!\d+|[ip])))+.*?(?<hash>[(\[]\w{8}[)\]])?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime - Title {Absolute Rom Number}
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ]?)?(?<title>.+?)(?:(?:[-_\W](?<![()\[!]))+(?<absoluteepisode>(?<!\d+)\d{2,3}(\.\d{1,2})?(?!\d+|[ip])))+.*?(?<hash>[(\[]\w{8}[)\]])?$",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Extant, terrible multi-rom naming (extant.10708.hdtv-lol.mp4)
                new Regex(@"^(?<title>.+?)[-_. ](?<platform>[0]?\d?)(?:(?<rom>\d{2}){2}(?!\d+))[-_. ]",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Platform only releases for poorly named anime
                new Regex(@"^(?:\[(?<subgroup>.+?)\][-_. ])?(?<title>.+?)[-_. ]+?[\[(](?:S|Platform|Saison|Game|Stagione)[-_. ]?(?<platform>\d{1,2}(?![-_. ]?\d+))(?:[-_. )\]]|$)+(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Roms without a title, Single rom numbers (S1E1, 1x1)
                new Regex(@"^(?:S?(?<platform>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:[-_ ]?[ex])(?<rom>\d{1}(?!\d+))))",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                // Anime OVA special
                new Regex(@"^\[(?<subgroup>.+?)\][-_. ]?(?<title>.+?)(?:[-_. ]+(?<special>special|ova|ovd|ncop|nced)).*?(?<hash>[(\[]\w{8}[)\]])?(?:$|\.mkv)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

        private static readonly Regex[] SpecialRomTitleRegex = new Regex[]
            {
                new Regex(@"\.S\d+E00\.(?<episodetitle>.+?)(?:\.(?:720p|1080p|2160p|HDTV|WEB|WEBRip|WEB-DL)\.|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                new Regex(@"\.S\d+\.Special\.(?<episodetitle>.+?)(?:\.(?:720p|1080p|2160p|HDTV|WEB|WEBRip|WEB-DL)\.|$)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

        private static readonly Regex[] RejectHashedReleasesRegexes = new Regex[]
            {
                // Generic match for md5 and mixed-case hashes.
                new Regex(@"^[0-9a-zA-Z]{32}", RegexOptions.Compiled),

                // Generic match for shorter lower-case hashes.
                new Regex(@"^[a-z0-9]{24}$", RegexOptions.Compiled),

                // Format seen on some NZBGeek releases
                // Be very strict with these coz they are very close to the valid 101 ep numbering.
                new Regex(@"^[A-Z]{11}\d{3}$", RegexOptions.Compiled),
                new Regex(@"^[a-z]{12}\d{3}$", RegexOptions.Compiled),

                // Backup filename (Unknown origins)
                new Regex(@"^Backup_\d{5,}S\d{2}-\d{2}$", RegexOptions.Compiled),

                // 123 - Started appearing December 2014
                new Regex(@"^123$", RegexOptions.Compiled),

                // abc - Started appearing January 2015
                new Regex(@"^abc$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // abc - Started appearing 2020
                new Regex(@"^abc[-_. ]xyz", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // b00bs - Started appearing January 2015
                new Regex(@"^b00bs$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // 170424_26 - Started appearing August 2018
                new Regex(@"^\d{6}_\d{2}$"),

                // additional Generic match for mixed-case hashes. - Started appearing Dec 2020
                new Regex(@"^[0-9a-zA-Z]{30}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{26}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{39}", RegexOptions.Compiled),

                // additional Generic match for mixed-case hashes. - Started appearing Jan 2021
                new Regex(@"^[0-9a-zA-Z]{24}", RegexOptions.Compiled),
            };

        private static readonly Regex[] PlatformFolderRegexes = new Regex[]
            {
                new Regex(@"^(Platform[ ._-]*\d+|Specials)$", RegexOptions.Compiled)
            };

        // Regex to detect whether the title was reversed.
        private static readonly Regex ReversedTitleRegex = new Regex(@"(?:^|[-._ ])(p027|p0801|\d{2,3}E-?\d{2}S)[-._ ]", RegexOptions.Compiled);

        private static readonly RegexReplace NormalizeRegex = new RegexReplace(@"((?:\b|_)(?<!^)([aà](?!$)|an|the|and|or|of)(?!$)(?:\b|_))|\W|_",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PercentRegex = new Regex(@"(?<=\b\d+)%", RegexOptions.Compiled);

        private static readonly RegexReplace SimpleTitleRegex = new RegexReplace(@"(?:(480|540|576|720|1080|1440|2160)[ip]|[xh][\W_]?26[45]|DD\W?5\W1|[<>?*]|848x480|1280x720|1920x1080|3840x2160|4096x2160|(?<![a-f0-9])(8|10)[ -]?(b(?![a-z0-9])|bit))\s*?",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Valid TLDs http://data.iana.org/TLD/tlds-alpha-by-domain.txt

        private static readonly Regex SixDigitAirDateRegex = new Regex(@"(?<=[_.-])(?<airdate>(?<!\d)(?<airyear>[1-9]\d{1})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9]))(?=[_.-])",
                                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CleanQualityBracketsRegex = new Regex(@"\[[a-z0-9 ._-]+\]$",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YearInTitleRegex = new Regex(@"^(?<title>.+?)[-_. ]+?[\(\[]?(?<year>\d{4})[\]\)]?",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TitleComponentsRegex = new Regex(@"^(?:(?<title>.+?) \((?<title>.+?)\)|(?<title>.+?) \| (?<title>.+?)|(?<title>.+?) AKA (?<title>.+?))$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex PartRegex = new Regex(@"\(\d+\)$", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex ArticleWordRegex = new Regex(@"^(a|an|the)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);
        private static readonly Regex PlatformFolderRegex = new Regex(@"^(?:S|Platform|Saison|Game|Stagione)[-_. ]*(?<platform>(?<!\d+)\d{1,4}(?!\d+))(?:[_. ]+(?!\d+)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SimpleRomNumberRegex = new Regex(@"^[ex]?(?<rom>(?<!\d+)\d{1,3}(?!\d+))(?:[ex-](?<rom>(?<!\d+)\d{1,3}(?!\d+)))?(?:[_. ](?!\d+)(?<remaining>.+)|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RequestInfoRegex = new Regex(@"^(?:\[.+?\])+", RegexOptions.Compiled);

        private static readonly string[] Numbers = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

        private static readonly Regex MultiRegex = new(@"[-_. \[](?<multi>multi|multilang|multilanguage)[-_. \]]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Dictionary<string, int> ShortMonths = new()
        {
            { "jan", 1 },
            { "feb", 2 },
            { "mar", 3 },
            { "apr", 4 },
            { "may", 5 },
            { "jun", 6 },
            { "jul", 7 },
            { "aug", 8 },
            { "sep", 9 },
            { "oct", 10 },
            { "nov", 11 },
            { "dec", 12 },
        };

        public static ParsedRomInfo ParsePath(string path)
        {
            var fileInfo = new FileInfo(path);
            var result = ParseTitle(fileInfo.Name);

            // Parse using the folder and file separately, but combine if they both parse correctly.
            var romNumberMatch = SimpleRomNumberRegex.Match(fileInfo.Name);

            if (romNumberMatch.Success && fileInfo.Directory?.Name != null && (result == null || result.IsMiniSeries || result.AbsoluteRomNumbers.Any()))
            {
                var seasonMatch = PlatformFolderRegex.Match(fileInfo.Directory.Name);

                if (seasonMatch.Success && seasonMatch.Groups["platform"].Success)
                {
                    var episodeCaptures = romNumberMatch.Groups["rom"].Captures.Cast<Capture>().ToList();
                    var first = ParseNumber(episodeCaptures.First().Value);
                    var last = ParseNumber(episodeCaptures.Last().Value);
                    var pathTitle = $"S{seasonMatch.Groups["platform"].Value}E{first:00}";

                    if (first != last)
                    {
                        pathTitle += $"-E{last:00}";
                    }

                    if (romNumberMatch.Groups["remaining"].Success)
                    {
                        pathTitle += $" {romNumberMatch.Groups["remaining"].Value}";
                    }

                    var parsedFileInfo = ParseTitle(pathTitle);

                    Logger.Debug("Rom parsed from file and folder names. {0}", parsedFileInfo);

                    return parsedFileInfo;
                }
            }

            if (result == null && int.TryParse(Path.GetFileNameWithoutExtension(fileInfo.Name), out var number))
            {
                Logger.Debug("Attempting to parse rom info using directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseTitle(fileInfo.Directory.Name);

                if (result != null && result.AbsoluteRomNumbers.Contains(number))
                {
                    result.AbsoluteRomNumbers = new[] { number };
                }
                else if (result != null && result.RomNumbers.Contains(number))
                {
                    result.RomNumbers = new[] { number };
                }
                else
                {
                    result = null;
                }
            }

            // TODO: Skip for anime specials

            if (result == null)
            {
                Logger.Debug("Attempting to parse rom info using combined directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseTitle(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                Logger.Debug("Attempting to parse rom info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseTitle(fileInfo.Directory.Name + fileInfo.Extension);
            }

            return result;
        }

        public static string SimplifyTitle(string title)
        {
            if (!ValidateBeforeParsing(title))
            {
                return title;
            }

            Logger.Debug("Parsing string '{0}'", title);

            if (ReversedTitleRegex.IsMatch(title))
            {
                var titleWithoutExtension = FileExtensions.RemoveFileExtension(title).ToCharArray();
                Array.Reverse(titleWithoutExtension);

                title = string.Concat(new string(titleWithoutExtension), title.AsSpan(titleWithoutExtension.Length));

                Logger.Debug("Reversed name detected. Converted to '{0}'", title);
            }

            var simpleTitle = title;

            simpleTitle = ParserCommon.WebsitePrefixRegex.Replace(simpleTitle);
            simpleTitle = ParserCommon.WebsitePostfixRegex.Replace(simpleTitle);
            simpleTitle = ParserCommon.CleanTorrentSuffixRegex.Replace(simpleTitle);

            return simpleTitle;
        }

        public static ParsedRomInfo ParseTitle(string title)
        {
            try
            {
                if (!ValidateBeforeParsing(title))
                {
                    return null;
                }

                Logger.Debug("Parsing string '{0}'", title);

                if (ReversedTitleRegex.IsMatch(title))
                {
                    var titleWithoutExtension = FileExtensions.RemoveFileExtension(title).ToCharArray();
                    Array.Reverse(titleWithoutExtension);

                    title = string.Concat(new string(titleWithoutExtension), title.AsSpan(titleWithoutExtension.Length));

                    Logger.Debug("Reversed name detected. Converted to '{0}'", title);
                }

                var releaseTitle = FileExtensions.RemoveFileExtension(title);

                releaseTitle = releaseTitle.Replace("【", "[").Replace("】", "]");

                foreach (var replace in ParserCommon.PreSubstitutionRegex)
                {
                    if (replace.TryReplace(ref releaseTitle))
                    {
                        Logger.Trace($"Replace regex: {replace}");
                        Logger.Debug("Substituted with " + releaseTitle);
                    }
                }

                var simpleTitle = SimpleTitleRegex.Replace(releaseTitle);

                // TODO: Quick fix stripping [url] - prefixes and postfixes.
                simpleTitle = ParserCommon.WebsitePrefixRegex.Replace(simpleTitle);
                simpleTitle = ParserCommon.WebsitePostfixRegex.Replace(simpleTitle);
                simpleTitle = ParserCommon.CleanTorrentSuffixRegex.Replace(simpleTitle);

                simpleTitle = CleanQualityBracketsRegex.Replace(simpleTitle, m =>
                {
                    if (QualityParser.ParseQualityName(m.Value).Quality != Qualities.Quality.Unknown)
                    {
                        return string.Empty;
                    }

                    return m.Value;
                });

                var sixDigitAirDateMatch = SixDigitAirDateRegex.Match(simpleTitle);
                if (sixDigitAirDateMatch.Success)
                {
                    var airYear = sixDigitAirDateMatch.Groups["airyear"].Value;
                    var airMonth = sixDigitAirDateMatch.Groups["airmonth"].Value;
                    var airDay = sixDigitAirDateMatch.Groups["airday"].Value;

                    if (airMonth != "00" || airDay != "00")
                    {
                        var fixedDate = string.Format("20{0}.{1}.{2}", airYear, airMonth, airDay);

                        simpleTitle = simpleTitle.Replace(sixDigitAirDateMatch.Groups["airdate"].Value, fixedDate);
                    }
                }

                foreach (var regex in ReportTitleRegex)
                {
                    var match = regex.Matches(simpleTitle);

                    if (match.Count != 0)
                    {
                        Logger.Trace(regex);
                        try
                        {
                            var result = ParseMatchCollection(match, releaseTitle);

                            if (result != null)
                            {
                                if (result.FullSeason && result.ReleaseTokens.ContainsIgnoreCase("Special"))
                                {
                                    result.FullSeason = false;
                                    result.Special = true;
                                }

                                result.Languages = LanguageParser.ParseLanguages(result.ReleaseTokens);
                                Logger.Debug("Languages parsed: {0}", string.Join(", ", result.Languages));

                                result.Quality = QualityParser.ParseQuality(title);
                                Logger.Debug("Quality parsed: {0}", result.Quality);

                                result.ReleaseGroup = ReleaseGroupParser.ParseReleaseGroup(releaseTitle);

                                var subGroup = GetSubGroup(match);
                                if (!subGroup.IsNullOrWhiteSpace())
                                {
                                    result.ReleaseGroup = subGroup;
                                }

                                Logger.Debug("Release Group parsed: {0}", result.ReleaseGroup);

                                result.ReleaseHash = GetReleaseHash(match);
                                if (!result.ReleaseHash.IsNullOrWhiteSpace())
                                {
                                    Logger.Debug("Release Hash parsed: {0}", result.ReleaseHash);
                                }

                                return result;
                            }
                        }
                        catch (InvalidDateException ex)
                        {
                            Logger.Debug(ex, ex.Message);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!title.ToLower().Contains("password") && !title.ToLower().Contains("yenc"))
                {
                    Logger.Error(e, "An error has occurred while trying to parse {0}", title);
                }
            }

            Logger.Debug("Unable to parse {0}", title);
            return null;
        }

        public static string ParseSeriesName(string title)
        {
            Logger.Debug("Parsing string '{0}'", title);

            var parseResult = ParseTitle(title);

            if (parseResult == null)
            {
                return CleanGameTitle(title);
            }

            return parseResult.GameTitle;
        }

        public static string CleanGameTitle(this string title)
        {
            if (title.IsNullOrWhiteSpace())
            {
                return title;
            }

            // If Title only contains numbers return it as is.
            if (long.TryParse(title, out _))
            {
                return title;
            }

            // Replace `%` with `percent` to deal with the 3% case
            title = PercentRegex.Replace(title, "percent");

            return NormalizeRegex.Replace(title).ToLowerInvariant().RemoveDiacritics();
        }

        public static string NormalizeRomTitle(string title)
        {
            var match = SpecialRomTitleRegex
                        .Select(v => v.Match(title))
                        .FirstOrDefault(v => v.Success);

            if (match != null)
            {
                title = match.Groups["episodetitle"].Value;
            }

            // Disabled, Until we run into specific testcases for the removal of these words.
            // title = SpecialEpisodeWordRegex.Replace(title, string.Empty);

            title = PartRegex.Replace(title, "");
            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim().ToLower();
        }

        public static string NormalizeTitle(string title)
        {
            title = PunctuationRegex.Replace(title, string.Empty);
            title = ArticleWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim().ToLower();
        }

        public static string NormalizeImdbId(string imdbId)
        {
            var imdbRegex = new Regex(@"^(\d{1,10}|(tt)\d{1,10})$");

            if (!imdbRegex.IsMatch(imdbId))
            {
                return null;
            }

            if (imdbId.Length > 2)
            {
                imdbId = imdbId.Replace("tt", "").PadLeft(7, '0');
                return $"tt{imdbId}";
            }

            return null;
        }

        public static string RemoveFileExtension(string title)
        {
            return FileExtensions.RemoveFileExtension(title);
        }

        public static bool HasMultipleLanguages(string title)
        {
            return MultiRegex.IsMatch(title);
        }

        private static GameTitleInfo GetGameTitleInfo(string title, MatchCollection matchCollection)
        {
            var gameTitleInfo = new GameTitleInfo();
            gameTitleInfo.Title = title;

            var match = YearInTitleRegex.Match(title);

            if (!match.Success)
            {
                gameTitleInfo.TitleWithoutYear = title;
            }
            else
            {
                gameTitleInfo.TitleWithoutYear = match.Groups["title"].Value;
                gameTitleInfo.Year = Convert.ToInt32(match.Groups["year"].Value);
            }

            var matchComponents = TitleComponentsRegex.Match(gameTitleInfo.TitleWithoutYear);

            if (matchComponents.Success)
            {
                gameTitleInfo.AllTitles = matchComponents.Groups["title"].Captures.OfType<Capture>().Select(v => v.Value).ToArray();
            }
            else if (matchCollection[0].Groups["title"].Captures.Count > 1)
            {
                gameTitleInfo.AllTitles = matchCollection[0].Groups["title"].Captures.Select(c => c.Value.Replace('.', ' ').Replace('_', ' ')).ToArray();
            }

            return gameTitleInfo;
        }

        private static ParsedRomInfo ParseMatchCollection(MatchCollection matchCollection, string releaseTitle)
        {
            var seriesName = matchCollection[0].Groups["title"].Value.Replace('.', ' ').Replace('_', ' ');
            seriesName = RequestInfoRegex.Replace(seriesName, "").Trim(' ');

            int.TryParse(matchCollection[0].Groups["airyear"].Value, out var airYear);

            var lastSeasonEpisodeStringIndex = matchCollection[0].Groups["title"].EndIndex();

            ParsedRomInfo result;

            if (airYear < 1900)
            {
                result = new ParsedRomInfo
                {
                    ReleaseTitle = releaseTitle,
                    RomNumbers = Array.Empty<int>(),
                    AbsoluteRomNumbers = Array.Empty<int>()
                };

                foreach (Match matchGroup in matchCollection)
                {
                    var episodeCaptures = matchGroup.Groups["rom"].Captures.Cast<Capture>().ToList();
                    var absoluteEpisodeCaptures = matchGroup.Groups["absoluteepisode"].Captures.Cast<Capture>().ToList();

                    // Allows use to return a list of 0 roms (We can handle that as a full platform release)
                    if (episodeCaptures.Any())
                    {
                        var first = ParseNumber(episodeCaptures.First().Value);
                        var last = ParseNumber(episodeCaptures.Last().Value);

                        if (first > last)
                        {
                            return null;
                        }

                        var count = last - first + 1;
                        result.RomNumbers = Enumerable.Range(first, count).ToArray();

                        lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, episodeCaptures.Last().EndIndex());

                        if (matchGroup.Groups["special"].Success)
                        {
                            result.Special = true;
                        }

                        if (matchGroup.Groups["splitepisode"].Success)
                        {
                            result.IsSplitEpisode = true;
                        }
                    }

                    if (absoluteEpisodeCaptures.Any())
                    {
                        var first = ParseDecimal(absoluteEpisodeCaptures.First().Value);
                        var last = ParseDecimal(absoluteEpisodeCaptures.Last().Value);

                        if (first > last)
                        {
                            return null;
                        }

                        if ((first % 1) != 0 || (last % 1) != 0)
                        {
                            if (absoluteEpisodeCaptures.Count != 1)
                            {
                                return null; // Multiple matches not allowed for specials
                            }

                            result.SpecialAbsoluteRomNumbers = new decimal[] { first };
                            result.Special = true;

                            lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, absoluteEpisodeCaptures.First().EndIndex());
                        }
                        else
                        {
                            var count = last - first + 1;
                            result.AbsoluteRomNumbers = Enumerable.Range((int)first, (int)count).ToArray();

                            if (matchGroup.Groups["special"].Success)
                            {
                                result.Special = true;
                            }

                            lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, absoluteEpisodeCaptures.Last().EndIndex());
                        }
                    }

                    if (!episodeCaptures.Any() && !absoluteEpisodeCaptures.Any())
                    {
                        // Check to see if this is an "Extras" or "SUBPACK" release, if it is, set
                        // IsSeasonExtra so they can be filtered out
                        if (!matchCollection[0].Groups["extras"].Value.IsNullOrWhiteSpace())
                        {
                            result.IsSeasonExtra = true;
                        }

                        // Partial platform packs will have a seasonpart group so they can be differentiated
                        // from a full platform/single rom release
                        var seasonPart = matchCollection[0].Groups["seasonpart"].Value;

                        if (seasonPart.IsNotNullOrWhiteSpace())
                        {
                            result.SeasonPart = Convert.ToInt32(seasonPart);
                            result.IsPartialSeason = true;
                        }
                        else if (matchCollection[0].Groups["special"].Success)
                        {
                            result.Special = true;
                        }
                        else
                        {
                            result.FullSeason = true;
                        }
                    }

                    if (episodeCaptures.Count == 2 && matchCollection[0].Groups["episodecount"].Success && episodeCaptures.Last().Value == matchCollection[0].Groups["episodecount"].Value)
                    {
                        result.RomNumbers = Array.Empty<int>();
                        result.FullSeason = true;
                    }
                }

                var platforms = new List<int>();

                foreach (Capture seasonCapture in matchCollection[0].Groups["platform"].Captures)
                {
                    if (int.TryParse(seasonCapture.Value, out var parsedSeason))
                    {
                        platforms.Add(parsedSeason);

                        lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, seasonCapture.EndIndex());
                    }
                }

                // If more than 1 platform was parsed set IsMultiSeason to true so it can be rejected later
                if (platforms.Distinct().Count() > 1)
                {
                    result.IsMultiSeason = true;
                }

                if (platforms.Any())
                {
                    // If at least one platform was parsed use the first platform as the platform
                    result.SeasonNumber = platforms.First();
                }
                else if (!result.AbsoluteRomNumbers.Any() && result.RomNumbers.Any())
                {
                    // If no platform was found and it's not an absolute only release it should be treated as a mini game and platform 1
                    result.SeasonNumber = 1;
                    result.IsMiniSeries = true;
                }
            }
            else
            {
                // Try to Parse as a daily show

                var airmonth = 0;
                var airday = 0;

                if (matchCollection[0].Groups["ambiguousairmonth"].Success &&
                    matchCollection[0].Groups["ambiguousairday"].Success)
                {
                    var ambiguousAirMonth = Convert.ToInt32(matchCollection[0].Groups["ambiguousairmonth"].Value);
                    var ambiguousAirDay = Convert.ToInt32(matchCollection[0].Groups["ambiguousairday"].Value);

                    if (ambiguousAirDay <= 12 && ambiguousAirMonth <= 12)
                    {
                        throw new InvalidDateException("Ambiguous Date, cannot validate month and day with {0} and {1}", ambiguousAirMonth, ambiguousAirDay);
                    }

                    airmonth = ambiguousAirMonth;
                    airday = ambiguousAirDay;
                }
                else if (matchCollection[0].Groups["shortairmonth"].Success)
                {
                    var shortMonthValue = matchCollection[0].Groups["shortairmonth"].Value;

                    if (ShortMonths.TryGetValue(shortMonthValue.ToLowerInvariant(), out var shortMonth))
                    {
                        airmonth = shortMonth;
                        airday = Convert.ToInt32(matchCollection[0].Groups["airday"].Value);
                    }
                    else
                    {
                        throw new InvalidDateException("Unable to determine air month from month: {0}", shortMonthValue);
                    }
                }
                else
                {
                    airmonth = Convert.ToInt32(matchCollection[0].Groups["airmonth"].Value);
                    airday = Convert.ToInt32(matchCollection[0].Groups["airday"].Value);
                }

                // Swap day and month if month is bigger than 12 (scene fail)
                if (airmonth > 12)
                {
                    (airday, airmonth) = (airmonth, airday);
                }

                DateTime airDate;

                try
                {
                    airDate = new DateTime(airYear, airmonth, airday);
                }
                catch (Exception)
                {
                    throw new InvalidDateException("Invalid date found: {0}-{1}-{2}", airYear, airmonth, airday);
                }

                // Check if rom is in the future (most likely a parse error)
                if (airDate > DateTime.Now.AddDays(1).Date)
                {
                    throw new InvalidDateException("Invalid date found: {0}", airDate);
                }

                // If the parsed air date is before 1970 and the title year wasn't matched (not a match for the Plex DVR format) throw an error
                if (airDate < new DateTime(1970, 1, 1) && matchCollection[0].Groups["titleyear"].Value.IsNullOrWhiteSpace())
                {
                    throw new InvalidDateException("Invalid date found: {0}", airDate);
                }

                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airyear"].EndIndex());
                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airmonth"].EndIndex());
                lastSeasonEpisodeStringIndex = Math.Max(lastSeasonEpisodeStringIndex, matchCollection[0].Groups["airday"].EndIndex());

                result = new ParsedRomInfo
                {
                    ReleaseTitle = releaseTitle,
                    AirDate = airDate.ToString(Rom.AIR_DATE_FORMAT),
                };

                var partMatch = matchCollection[0].Groups["part"];

                if (partMatch.Success)
                {
                    result.DailyPart = Convert.ToInt32(partMatch.Value);
                }
            }

            if (lastSeasonEpisodeStringIndex < releaseTitle.Length)
            {
                result.ReleaseTokens = releaseTitle.Substring(lastSeasonEpisodeStringIndex);
            }
            else
            {
                result.ReleaseTokens = releaseTitle;
            }

            result.GameTitle = seriesName;
            result.GameTitleInfo = GetGameTitleInfo(result.GameTitle, matchCollection);

            Logger.Debug("Rom Parsed. {0}", result);

            return result;
        }

        private static bool ValidateBeforeParsing(string title)
        {
            if (title.ToLower().Contains("password") && title.ToLower().Contains("yenc"))
            {
                Logger.Debug("");
                return false;
            }

            if (!title.Any(char.IsLetterOrDigit))
            {
                return false;
            }

            var titleWithoutExtension = FileExtensions.RemoveFileExtension(title);

            if (RejectHashedReleasesRegexes.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Hashed Release Title: " + title);
                return false;
            }

            if (PlatformFolderRegexes.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Platform Folder Release Title: " + title);
                return false;
            }

            return true;
        }

        private static string GetSubGroup(MatchCollection matchCollection)
        {
            var subGroup = matchCollection[0].Groups["subgroup"];

            if (subGroup.Success)
            {
                return subGroup.Value;
            }

            return string.Empty;
        }

        private static string GetReleaseHash(MatchCollection matchCollection)
        {
            var hash = matchCollection[0].Groups["hash"];

            if (hash.Success)
            {
                var hashValue = hash.Value.Trim('[', ']', '(', ')');

                if (hashValue.Equals("1280x720"))
                {
                    return string.Empty;
                }

                return hashValue;
            }

            return string.Empty;
        }

        private static int ParseNumber(string value)
        {
            var normalized = ConvertToNumerals(value.Normalize(NormalizationForm.FormKC));

            if (int.TryParse(normalized, out var number))
            {
                return number;
            }

            number = Array.IndexOf(Numbers, value.ToLower());

            if (number != -1)
            {
                return number;
            }

            throw new FormatException(string.Format("{0} isn't a number", value));
        }

        private static decimal ParseDecimal(string value)
        {
            var normalized = ConvertToNumerals(value.Normalize(NormalizationForm.FormKC));

            if (decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return number;
            }

            throw new FormatException(string.Format("{0} isn't a number", value));
        }

        private static string ConvertToNumerals(string input)
        {
            var result = new StringBuilder(input.Length);

            foreach (var c in input.ToCharArray())
            {
                if (char.IsNumber(c))
                {
                    result.Append(char.GetNumericValue(c));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}
