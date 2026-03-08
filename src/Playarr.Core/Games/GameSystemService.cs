using System.Collections.Generic;
using System.Linq;
using NLog;
using Playarr.Core.Games.Events;
using Playarr.Core.Messaging.Events;

namespace Playarr.Core.Games
{
    public interface IGameSystemService
    {
        GameSystem Get(int id);
        List<GameSystem> All();
        GameSystem FindByName(string name);
        GameSystem FindByFolderName(string folderName);
        List<GameSystem> GetBySystemType(GameSystemType systemType);
        GameSystem Add(GameSystem system);
        GameSystem Update(GameSystem system);
        void Delete(int id);
    }

    public class GameSystemService : IGameSystemService
    {
        private readonly IGameSystemRepository _repository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public GameSystemService(IGameSystemRepository repository,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _repository = repository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public GameSystem Get(int id)
        {
            return _repository.Get(id);
        }

        public List<GameSystem> All()
        {
            return _repository.All().OrderBy(s => s.Name).ToList();
        }

        public GameSystem FindByName(string name)
        {
            return _repository.FindByName(name);
        }

        public GameSystem FindByFolderName(string folderName)
        {
            return _repository.FindByFolderName(folderName);
        }

        public List<GameSystem> GetBySystemType(GameSystemType systemType)
        {
            return _repository.GetBySystemType(systemType);
        }

        public GameSystem Add(GameSystem system)
        {
            _logger.Info("Adding game system: {0} ({1})", system.Name, system.FolderName);

            _repository.Insert(system);
            _eventAggregator.PublishEvent(new GameSystemUpdatedEvent());

            return system;
        }

        public GameSystem Update(GameSystem system)
        {
            _logger.Info("Updating game system: {0}", system.Name);

            _repository.Update(system);
            _eventAggregator.PublishEvent(new GameSystemUpdatedEvent());

            return system;
        }

        public void Delete(int id)
        {
            _logger.Info("Deleting game system: {0}", id);

            _repository.Delete(id);
            _eventAggregator.PublishEvent(new GameSystemUpdatedEvent());
        }
    }
}
