using System;
using System.Collections.Generic;
using System.Linq;
using Playarr.Common.Cache;
using Playarr.Common.Extensions;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications
{
    public class MediaServerUpdateQueue<TQueueHost, TItemInfo>
        where TQueueHost : class
    {
        private class UpdateQueue
        {
            public Dictionary<int, UpdateQueueItem<TItemInfo>> Pending { get; } = new Dictionary<int, UpdateQueueItem<TItemInfo>>();
            public bool Refreshing { get; set; }
        }

        private readonly ICached<UpdateQueue> _pendingSeriesCache;

        public MediaServerUpdateQueue(ICacheManager cacheManager)
        {
            _pendingSeriesCache = cacheManager.GetRollingCache<UpdateQueue>(typeof(TQueueHost), "pendingSeries", TimeSpan.FromDays(1));
        }

        public void Add(string identifier, Game game, TItemInfo info)
        {
            var queue = _pendingSeriesCache.Get(identifier, () => new UpdateQueue());

            lock (queue)
            {
                var item = queue.Pending.TryGetValue(game.Id, out var value)
                    ? value
                    : new UpdateQueueItem<TItemInfo>(game);

                item.Info.Add(info);

                queue.Pending[game.Id] = item;
            }
        }

        public void ProcessQueue(string identifier, Action<List<UpdateQueueItem<TItemInfo>>> update)
        {
            var queue = _pendingSeriesCache.Find(identifier);

            if (queue == null)
            {
                return;
            }

            lock (queue)
            {
                if (queue.Refreshing)
                {
                    return;
                }

                queue.Refreshing = true;
            }

            try
            {
                while (true)
                {
                    List<UpdateQueueItem<TItemInfo>> items;

                    lock (queue)
                    {
                        if (queue.Pending.Empty())
                        {
                            queue.Refreshing = false;
                            return;
                        }

                        items = queue.Pending.Values.ToList();
                        queue.Pending.Clear();
                    }

                    update(items);
                }
            }
            catch
            {
                lock (queue)
                {
                    queue.Refreshing = false;
                }

                throw;
            }
        }
    }

    public class UpdateQueueItem<TItemInfo>
    {
        public Game Game { get; set; }
        public HashSet<TItemInfo> Info { get; set; }

        public UpdateQueueItem(Game game)
        {
            Game = game;
            Info = new HashSet<TItemInfo>();
        }
    }
}
