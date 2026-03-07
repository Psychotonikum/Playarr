using System;
using System.Collections.Generic;
using Playarr.Core.Indexers;
using Playarr.Core.Validation;

namespace Playarr.Core.Test.IndexerTests
{
    public class TestIndexerSettings : IIndexerSettings
    {
        public PlayarrValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public string BaseUrl { get; set; }

        public IEnumerable<int> MultiLanguages { get; set; }
        public IEnumerable<int> FailDownloads { get; set; }
    }
}
