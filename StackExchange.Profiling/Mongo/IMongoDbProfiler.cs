using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using StackExchange.Profiling.Data;

namespace StackExchange.Profiling.Mongo
{
    public interface IMongoDbProfiler
    {
        Guid ExecuteStart(string collectionName, object query, ExecuteType executeType);
        Guid ExecuteStart(string collectionName, IMongoQuery query, IMongoUpdate update, ExecuteType executeType);
        void ExecuteFinish(Guid id, MongoCursor reader);
        void ReaderFinish(MongoCursor reader);
    }
}
