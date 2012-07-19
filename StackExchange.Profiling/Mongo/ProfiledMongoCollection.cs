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
            var dlist = documents.ToList();
            object insertObj = String.Join(",", dlist.Select(d => d.ToString()).ToArray());
            if (_profiler != null) _profiler.ExecuteStart(this.Name + ".insert", insertObj, ExecuteType.NonQuery);
            try
            {
                return base.InsertBatch<TNominalType>(dlist, options);
            }
            finally
            {
                if (_profiler != null) _profiler.ExecuteFinish(insertObj, ExecuteType.NonQuery, null);
            }
        }

        public override SafeModeResult Update(IMongoQuery query, IMongoUpdate update, MongoUpdateOptions options)
        {
            if (_profiler != null) _profiler.ExecuteStart(this.Name + ".update", query, update, ExecuteType.NonQuery);
            try
            {
                return base.Update(query, update, options);
            }
            finally
            {
                if (_profiler != null) _profiler.ExecuteFinish(query, ExecuteType.NonQuery, null);
            }
        }

        public override SafeModeResult Remove(IMongoQuery query, RemoveFlags flags, SafeMode safeMode)
        {
            if (_profiler != null) _profiler.ExecuteStart(this.Name + ".remove", query, ExecuteType.NonQuery);
            try
            {
                return base.Remove(query, flags, safeMode);
            }
            finally
            {
                if (_profiler != null) _profiler.ExecuteFinish(query, ExecuteType.NonQuery, null);
            }
        }
    }
}