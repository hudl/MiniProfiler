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
        void ExecuteStart(object query, ExecuteType executeType);
        void ExecuteFinish(object query, ExecuteType executeType, MongoCursor reader);
        void ReaderFinish(MongoCursor reader);
    }
}
