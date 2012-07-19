using System;
using System.Collections.Generic;
using MongoDB.Driver;
using StackExchange.Profiling.Data;

namespace StackExchange.Profiling.Mongo
{
    public class ProfiledMongoDatabase : MongoDatabase
    {
        private readonly object _databaseLock = new object();
        private readonly Dictionary<MongoCollectionSettings, MongoCollection> _collections = new Dictionary<MongoCollectionSettings, MongoCollection>();
        private readonly IMongoDbProfiler _profiler;

        public ProfiledMongoDatabase(MongoServer server, MongoDatabaseSettings settings, IMongoDbProfiler profiler)
            : base(server, settings)
        {
            _profiler = profiler;
        }

        public override CommandResult RunCommandAs(Type commandResultType, IMongoCommand command)
        {
            if (_profiler != null) _profiler.ExecuteStart(String.Empty, command, ExecuteType.NonQuery);
            try
            {
                return base.RunCommandAs(commandResultType, command);
            }
            finally
            {
                if (_profiler != null) _profiler.ExecuteFinish(command, ExecuteType.NonQuery, null);
            }
        }

        public override MongoCollection<TDefaultDocument> GetCollection<TDefaultDocument>(MongoCollectionSettings<TDefaultDocument> collectionSettings)
        {
            if (MiniProfiler.Current != null)
            {
                lock (this._databaseLock)
                {
                    MongoCollection conn;
                    if (!_collections.TryGetValue((MongoCollectionSettings)collectionSettings, out conn))
                    {
                        conn = (MongoCollection)new ProfiledMongoCollection<TDefaultDocument>(this, collectionSettings, _profiler);
                        _collections.Add((MongoCollectionSettings)collectionSettings, conn);
                    }
                    return (MongoCollection<TDefaultDocument>)conn;
                }
            }
            return base.GetCollection<TDefaultDocument>(collectionSettings);
        }
    }
}
