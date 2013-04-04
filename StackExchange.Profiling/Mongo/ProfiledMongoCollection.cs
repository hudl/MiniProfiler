using System;
using System.Linq;
using MongoDB.Driver;
using StackExchange.Profiling.Data;

namespace StackExchange.Profiling.Mongo
{
    public class ProfiledMongoCollection<TDefaultDocument> : MongoCollection<TDefaultDocument>
    {
        public ProfiledMongoCollection(MongoDatabase database, MongoCollectionSettings<TDefaultDocument> settings)
            : base(database, settings)
        {
        }

        public override MongoCursor<TDocument> FindAs<TDocument>(IMongoQuery query)
        {
            if (MiniProfiler.Current != null) return new ProfiledMongoCursor<TDocument>(this, query, MiniProfiler.Current);
            return base.FindAs<TDocument>(query);
        }

        public override System.Collections.Generic.IEnumerable<SafeModeResult> InsertBatch<TNominalType>(System.Collections.Generic.IEnumerable<TNominalType> documents, MongoInsertOptions options)
        {
            IMongoDbProfiler profiler = MiniProfiler.Current;
            if (profiler == null) return base.InsertBatch<TNominalType>(documents, options);

            var dlist = documents.ToList();
            object insertObj = String.Join(",", dlist.Select(d => d.ToString()).ToArray());
            var id = profiler.ExecuteStart(this.Name + ".insert", insertObj, ExecuteType.NonQuery);
            try
            {
                return base.InsertBatch<TNominalType>(dlist, options);
            }
            finally
            {
                profiler.ExecuteFinish(id, null);
            }
        }

        public override SafeModeResult Update(IMongoQuery query, IMongoUpdate update, MongoUpdateOptions options)
        {
            IMongoDbProfiler profiler = MiniProfiler.Current;
            if (profiler == null) return base.Update(query, update, options);

            var id = profiler.ExecuteStart(this.Name + ".update", query, update, ExecuteType.NonQuery);
            try
            {
                return base.Update(query, update, options);
            }
            finally
            {
                profiler.ExecuteFinish(id, null);
            }
        }

        public override SafeModeResult Remove(IMongoQuery query, RemoveFlags flags, SafeMode safeMode)
        {
            IMongoDbProfiler profiler = MiniProfiler.Current;
            if (profiler == null) return base.Remove(query, flags, safeMode);

            var id = profiler.ExecuteStart(this.Name + ".remove", query, ExecuteType.NonQuery);
            try
            {
                return base.Remove(query, flags, safeMode);
            }
            finally
            {
                profiler.ExecuteFinish(id, null);
            }
        }
    }
}