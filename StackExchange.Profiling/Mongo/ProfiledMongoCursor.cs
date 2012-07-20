using System.Collections.Generic;
using MongoDB.Driver;

namespace StackExchange.Profiling.Mongo
{
    public class ProfiledMongoCursor<T> : MongoCursor<T>
    {
        private readonly IMongoDbProfiler _profiler;

        public ProfiledMongoCursor(MongoCollection collection, IMongoQuery query, IMongoDbProfiler profiler)
            : base(collection, query)
        {
            _profiler = profiler;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _profiler != null ? new ProfiledMongoCursorEnumerator<T>(this, _profiler) : base.GetEnumerator();
        }
    }
}