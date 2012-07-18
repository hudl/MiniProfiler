using System.Collections.Generic;
using MongoDB.Driver;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace StackExchange.Profiling.Mongo
{
    public class ProfiledMongoCursor<T> : MongoCursor<T>
    {
        private IMongoDbProfiler _profiler;

        public ProfiledMongoCursor(MongoCollection collection, IMongoQuery query, IMongoDbProfiler profiler)
            : base(collection, query)
        {
            _profiler = profiler;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            if (MiniProfiler.Current != null) return new ProfiledMongoCursorEnumerator<T>(this, _profiler);
            return base.GetEnumerator();
        }
    }
}