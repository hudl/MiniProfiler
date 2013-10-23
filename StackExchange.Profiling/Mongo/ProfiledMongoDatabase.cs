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

        public ProfiledMongoDatabase(MongoServer server, string databaseName, MongoDatabaseSettings settings)
            : base(server, databaseName, settings)
        {
        }

        public override CommandResult RunCommandAs(Type commandResultType, IMongoCommand command)
        {
            IMongoDbProfiler profiler = MiniProfiler.Current;
            if (profiler == null) return base.RunCommandAs(commandResultType, command);

            var id = profiler.ExecuteStart(String.Empty, command, ExecuteType.NonQuery);
            try
            {
                return base.RunCommandAs(commandResultType, command);
            }
            finally
            {
                profiler.ExecuteFinish(id, null);
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
                        conn = (MongoCollection)new ProfiledMongoCollection<TDefaultDocument>(this, collectionSettings);
                        _collections.Add((MongoCollectionSettings)collectionSettings, conn);
                    }
                    return (MongoCollection<TDefaultDocument>)conn;
                }
            }
            return base.GetCollection<TDefaultDocument>(collectionSettings);
        }
    }
}
