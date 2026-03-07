using System.Collections.Generic;
using System.Linq;
using Playarr.Core.Datastore;
using Playarr.Core.Messaging.Events;
using Playarr.Core.Games.Events;

namespace Playarr.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionService
    {
        ImportListExclusion Add(ImportListExclusion importListExclusion);
        List<ImportListExclusion> All();
        PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec);
        void Delete(int id);
        void Delete(List<int> ids);
        ImportListExclusion Get(int id);
        ImportListExclusion FindByIgdbId(int igdbId);
        ImportListExclusion Update(ImportListExclusion importListExclusion);
    }

    public class ImportListExclusionService : IImportListExclusionService, IHandleAsync<SeriesDeletedEvent>
    {
        private readonly IImportListExclusionRepository _repo;

        public ImportListExclusionService(IImportListExclusionRepository repo)
        {
            _repo = repo;
        }

        public ImportListExclusion Add(ImportListExclusion importListExclusion)
        {
            return _repo.Insert(importListExclusion);
        }

        public ImportListExclusion Update(ImportListExclusion importListExclusion)
        {
            return _repo.Update(importListExclusion);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public void Delete(List<int> ids)
        {
            _repo.DeleteMany(ids);
        }

        public ImportListExclusion Get(int id)
        {
            return _repo.Get(id);
        }

        public ImportListExclusion FindByIgdbId(int igdbId)
        {
            return _repo.FindByIgdbId(igdbId);
        }

        public List<ImportListExclusion> All()
        {
            return _repo.All().ToList();
        }

        public PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec)
        {
            return _repo.GetPaged(pagingSpec);
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            var exclusionsToAdd = new List<ImportListExclusion>();

            foreach (var game in message.Game.DistinctBy(s => s.TvdbId))
            {
                var existingExclusion = _repo.FindByIgdbId(game.TvdbId);

                if (existingExclusion != null)
                {
                    continue;
                }

                exclusionsToAdd.Add(new ImportListExclusion
                {
                    TvdbId = game.TvdbId,
                    Title = game.Title
                });
            }

            _repo.InsertMany(exclusionsToAdd);
        }
    }
}
