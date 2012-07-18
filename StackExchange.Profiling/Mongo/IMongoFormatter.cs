using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackExchange.Profiling.Mongo
{
    public interface IMongoFormatter
    {
        string FormatMongo(MongoTiming mongoTiming);
    }
}
