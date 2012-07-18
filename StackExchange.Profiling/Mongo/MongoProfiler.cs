using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using StackExchange.Profiling.Data;

namespace StackExchange.Profiling.Mongo
{
    class MongoProfiler
    {
        ConcurrentDictionary<Tuple<object, ExecuteType>, MongoTiming> _inProgress = new ConcurrentDictionary<Tuple<object, ExecuteType>, MongoTiming>();
        ConcurrentDictionary<MongoCursor, MongoTiming> _inProgressCursors = new ConcurrentDictionary<MongoCursor, MongoTiming>();

        /// <summary>
        /// The profiling session this SqlProfiler is part of.
        /// </summary>
        public MiniProfiler Profiler { get; private set; }

        /// <summary>
        /// Returns a new SqlProfiler to be used in the 'profiler' session.
        /// </summary>
        public MongoProfiler(MiniProfiler profiler)
        {
            Profiler = profiler;
        }

        /// <summary>
        /// Tracks when 'command' is started.
        /// </summary>
        public void ExecuteStartImpl(string collectionName, object query, ExecuteType type)
        {
            var id = Tuple.Create(query, type);
            var sqlTiming = new MongoTiming(collectionName, query.ToString(), type, Profiler);

            _inProgress[id] = sqlTiming;
        }

        public void ExecuteStartImpl(string collectionName, object query, IMongoUpdate update, ExecuteType type)
        {
            var id = Tuple.Create(query, type);
            var q = String.Format("{0}\n{1}", query, update);
            var sqlTiming = new MongoTiming(collectionName, q, type, Profiler);

            _inProgress[id] = sqlTiming;
        }

        /// <summary>
        /// Returns all currently open commands on this connection
        /// </summary>
        public MongoTiming[] GetInProgressCommands()
        {
            return _inProgress.Values.OrderBy(x => x.StartMilliseconds).ToArray();
        }
        /// <summary>
        /// Finishes profiling for 'command', recording durations.
        /// </summary>
        public void ExecuteFinishImpl(object query, ExecuteType type, MongoCursor reader = null)
        {
            var id = Tuple.Create(query, type);
            var current = _inProgress[id];
            current.ExecutionComplete(isReader: reader != null);
            MongoTiming ignore;
            _inProgress.TryRemove(id, out ignore);
            if (reader != null)
            {
                _inProgressCursors[reader] = current;
            }
        }

        /// <summary>
        /// Called when 'reader' finishes its iterations and is closed.
        /// </summary>
        public void ReaderFinishedImpl(MongoCursor reader)
        {
            MongoTiming stat;
            // this reader may have been disposed/closed by reader code, not by our using()
            if (_inProgressCursors.TryGetValue(reader, out stat))
            {
                stat.ReaderFetchComplete();
                MongoTiming ignore;
                _inProgressCursors.TryRemove(reader, out ignore);
            }
        }
    }
}
