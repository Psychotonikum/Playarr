using System.Collections.Generic;
using System.Linq;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.MediaInfo;

namespace Playarr.Core.Notifications.Webhook
{
    public class WebhookRomFileMediaInfo
    {
        public WebhookRomFileMediaInfo()
        {
        }

        public WebhookRomFileMediaInfo(RomFile romFile)
        {
            AudioChannels = MediaInfoFormatter.FormatAudioChannels(romFile.MediaInfo.PrimaryAudioStream);
            AudioCodec = MediaInfoFormatter.FormatAudioCodec(romFile.MediaInfo.PrimaryAudioStream, romFile.SceneName);
            AudioLanguages = romFile.MediaInfo.AudioStreams?.Select(l => l.Language).Distinct().ToList();
            Height = romFile.MediaInfo.Height;
            Width = romFile.MediaInfo.Width;
            Subtitles = romFile.MediaInfo.SubtitleStreams?.Select(l => l.Language).Distinct().ToList();
            VideoCodec = MediaInfoFormatter.FormatVideoCodec(romFile.MediaInfo, romFile.SceneName);
            VideoDynamicRange = MediaInfoFormatter.FormatVideoDynamicRange(romFile.MediaInfo);
            VideoDynamicRangeType = MediaInfoFormatter.FormatVideoDynamicRangeType(romFile.MediaInfo);
        }

        public decimal AudioChannels { get; set; }
        public string AudioCodec { get; set; }
        public List<string> AudioLanguages { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public List<string> Subtitles { get; set; }
        public string VideoCodec { get; set; }
        public string VideoDynamicRange { get; set; }
        public string VideoDynamicRangeType { get; set; }
    }
}
