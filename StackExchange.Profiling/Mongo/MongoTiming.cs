using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using StackExchange.Profiling.Data;
using StackExchange.Profiling.Helpers;

namespace StackExchange.Profiling.Mongo
{
    [DataContract]
    public class MongoTiming
    {
        /// <summary>
        /// Unique identifier for this SqlTiming.
        /// </summary>
        [ScriptIgnore]
        public Guid Id { get; set; }

        /// <summary>
        /// Category of statement executed.
        /// </summary>
        [DataMember(Order = 1)]
        public ExecuteType ExecuteType { get; set; }

        /// <summary>
        /// The query that was executed.
        /// </summary>
        [ScriptIgnore]
        [DataMember(Order = 2)]
        public string CommandString { get; set; }

        /// <summary>
        /// The command string with special formatting applied based on MiniProfiler.Settings.SqlFormatter
        /// </summary>
        public string FormattedCommandString
        {
            get
            {
                return CollectionName + "\n" + ((MiniProfiler.Settings.MongoFormatter == null) ? 
                    CommandString :
                    MiniProfiler.Settings.MongoFormatter.FormatMongo(this));
            }
        }

        /// <summary>
        /// Roughly where in the calling code that this sql was executed.
        /// </summary>
        [DataMember(Order = 3)]
        public string StackTraceSnippet { get; set; }

        /// <summary>
        /// Offset from main MiniProfiler start that this sql began.
        /// </summary>
        [DataMember(Order = 4)]
        public decimal StartMilliseconds { get; set; }

        /// <summary>
        /// How long this sql statement took to execute.
        /// </summary>
        [DataMember(Order = 5)]
        public decimal DurationMilliseconds { get; set; }

        /// <summary>
        /// When executing readers, how long it took to come back initially from the database, 
        /// before all records are fetched and reader is closed.
        /// </summary>
        [DataMember(Order = 6)]
        public decimal FirstFetchDurationMilliseconds { get; set; }

        /// <summary>
        /// Id of the Timing this statement was executed in.
        /// </summary>
        /// <remarks>
        /// Needed for database deserialization.
        /// </remarks>
        public Guid? ParentTimingId { get; set; }

        private Timing _parentTiming;
        /// <summary>
        /// The Timing step that this sql execution occurred in.
        /// </summary>
        [ScriptIgnore]
        public Timing ParentTiming
        {
            get { return _parentTiming; }
            set
            {
                _parentTiming = value;

                if (value != null && ParentTimingId != value.Id)
                    ParentTimingId = value.Id;
            }
        }

        /// <summary>
        /// True when other identical sql statements have been executed during this MiniProfiler session.
        /// </summary>
        [DataMember(Order = 7)]
        public bool IsDuplicate { get; set; }

        [DataMember(Order = 8)]
        public string CollectionName { get; set; }

        private readonly long _startTicks;
        private readonly MiniProfiler _profiler;

        /// <summary>
        /// Creates a new SqlTiming to profile 'command'.
        /// </summary>
        public MongoTiming(string collectionName, string command, ExecuteType type, MiniProfiler profiler)
        {
            Id = Guid.NewGuid();

            CollectionName = collectionName;
            CommandString = command;
            ExecuteType = type;

            if (!MiniProfiler.Settings.ExcludeStackTraceSnippetFromSqlTimings)
                StackTraceSnippet = Helpers.StackTraceSnippet.Get();

            _profiler = profiler;
            if (_profiler != null)
            {
                _profiler.AddMongoTiming(this);
                _startTicks = _profiler.ElapsedTicks;
                StartMilliseconds = _profiler.GetRoundedMilliseconds(_startTicks);
            }
        }

        /// <summary>
        /// Obsolete - used for serialization.
        /// </summary>
        [Obsolete("Used for serialization")]
        public MongoTiming()
        {
        }

        /// <summary>
        /// Returns a snippet of the sql command and the duration.
        /// </summary>
        public override string ToString()
        {
            return CommandString.Truncate(30) + " (" + DurationMilliseconds + " ms)";
        }

        /// <summary>
        /// Returns true if Ids match.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj != null && obj is MongoTiming && Id.Equals(((MongoTiming)obj).Id);
        }

        /// <summary>
        /// Returns hashcode of Id.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Called when command execution is finished to determine this SqlTiming's duration.
        /// </summary>
        public void ExecutionComplete(bool isReader)
        {
            if (isReader)
            {
                FirstFetchDurationMilliseconds = GetDurationMilliseconds();
            }
            else
            {
                DurationMilliseconds = GetDurationMilliseconds();
            }
        }

        /// <summary>
        /// Called when database reader is closed, ending profiling for <see cref="StackExchange.Profiling.Data.ExecuteType.Reader"/> SqlTimings.
        /// </summary>
        public void ReaderFetchComplete()
        {
            DurationMilliseconds = GetDurationMilliseconds();
        }

        private decimal GetDurationMilliseconds()
        {
            return _profiler.GetRoundedMilliseconds(_profiler.ElapsedTicks - _startTicks);
        }
    }
}
