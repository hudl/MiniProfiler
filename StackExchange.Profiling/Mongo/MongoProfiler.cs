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
    public class MongoProfiler
    {
        ConcurrentDictionary<Guid, MongoTiming> _inProgress = new ConcurrentDictionary<Guid, MongoTiming>();
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
        public Guid ExecuteStartImpl(string collectionName, object query, ExecuteType type)
        {
            var id = Guid.NewGuid();
            var sqlTiming = new MongoTiming(collectionName, query.ToString(), type, Profiler);

            _inProgress[id] = sqlTiming;
            return id;
        }

        public Guid ExecuteStartImpl(string collectionName, object query, IMongoUpdate update, ExecuteType type)
        {
            var id = Guid.NewGuid();
            var q = String.Format("{0}\n{1}", query, update);
            var sqlTiming = new MongoTiming(collectionName, q, type, Profiler);

            _inProgress[id] = sqlTiming;
            return id;
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
        public void ExecuteFinishImpl(Guid id, MongoCursor reader = null)
        {
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
        public void ReaderFinishImpl(MongoCursor reader)
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

    public static class MongoProfilerExtensions
    {
        /// <summary>
        /// Tracks when 'command' is started.
        /// </summary>
        public static Guid ExecuteStart(this MongoProfiler mongoProfiler, string collectionName, object query, ExecuteType type)
        {
            if (mongoProfiler == null) return Guid.Empty;
            return mongoProfiler.ExecuteStartImpl(collectionName, query, type);
        }

        public static Guid ExecuteStart(this MongoProfiler mongoProfiler, string collectionName, object query, IMongoUpdate update, ExecuteType type)
        {
            if (mongoProfiler == null) return Guid.Empty;
            return mongoProfiler.ExecuteStartImpl(collectionName, query, update, type);
        }

        /// <summary>
        /// Finishes profiling for 'command', recording durations.
        /// </summary>
        public static void ExecuteFinish(this MongoProfiler mongoProfiler, Guid id, MongoCursor reader = null)
        {
            if (mongoProfiler == null) return;
            mongoProfiler.ExecuteFinishImpl(id, reader);
        }

        /// <summary>
        /// Called when 'reader' finishes its iterations and is closed.
        /// </summary>
        public static void ReaderFinish(this MongoProfiler mongoProfiler, MongoCursor reader)
        {
            if (mongoProfiler == null) return;
            mongoProfiler.ReaderFinishImpl(reader);
        }

    }
}
