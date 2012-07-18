using System;
using System.Linq;
using MongoDB.Driver;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace StackExchange.Profiling.Mongo
{
    public class ProfiledMongoCollection<TDefaultDocument> : MongoCollection<TDefaultDocument>
    {
        private readonly IMongoDbProfiler _profiler;

        public ProfiledMongoCollection(MongoDatabase database, MongoCollectionSettings<TDefaultDocument> settings, IMongoDbProfiler profiler)
            : base(database, settings)
        {
            _profiler = profiler;
        }

        public override MongoCursor<TDocument> FindAs<TDocument>(IMongoQuery query)
        {
            if (MiniProfiler.Current != null) return new ProfiledMongoCursor<TDocument>(this, query, _profiler);
            return base.FindAs<TDocument>(query);
        }

        public override System.Collections.Generic.IEnumerable<SafeModeResult> InsertBatch<TNominalType>(System.Collections.Generic.IEnumerable<TNominalType> documents, MongoInsertOptions options)
        {
            object insertObj = new object();
            _profiler.ExecuteStart(insertObj, ExecuteType.NonQuery);
            try
            {
                return base.InsertBatch<TNominalType>(documents, options);
            }
            finally
            {
                _profiler.ExecuteFinish(insertObj, ExecuteType.NonQuery, null);
            }
        }

        public override SafeModeResult Update(IMongoQuery query, IMongoUpdate update, MongoUpdateOptions options)
        {
            var command = String.Format("{0} Query {1} Update {2}", this.Name, query, update);
            _profiler.ExecuteStart(query, ExecuteType.NonQuery);
            try
            {
                return base.Update(query, update, options);
            }
            finally
            {
                _profiler.ExecuteFinish(query, ExecuteType.NonQuery, null);
            }
        }

        public override SafeModeResult Remove(IMongoQuery query, RemoveFlags flags, SafeMode safeMode)
        {
            var command = String.Format("{0} Remove {1}", this.Name, query);
            _profiler.ExecuteStart(query, ExecuteType.NonQuery);
            try
            {
                return base.Remove(query, flags, safeMode);
            }
            finally
            {
                _profiler.ExecuteFinish(query, ExecuteType.NonQuery, null);
            }
        }
    }
}