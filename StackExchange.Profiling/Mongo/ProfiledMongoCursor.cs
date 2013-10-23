using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace StackExchange.Profiling.Mongo
{
    public class ProfiledMongoCursor<T> : MongoCursor<T>
    {
        private readonly IMongoDbProfiler _profiler;

        public ProfiledMongoCursor(MongoCollection collection, IMongoQuery query, ReadPreference readPreference,
                                   IBsonSerializer serializer, IBsonSerializationOptions serializationOptions,
                                   IMongoDbProfiler profiler)
            : base(collection, query, readPreference, serializer, serializationOptions)
        {
            _profiler = profiler;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _profiler != null ? new ProfiledMongoCursorEnumerator<T>(this, _profiler) : base.GetEnumerator();
        }
    }
}