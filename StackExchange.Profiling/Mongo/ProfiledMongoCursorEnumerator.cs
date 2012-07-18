using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace StackExchange.Profiling.Mongo
{
    public class ProfiledMongoCursorEnumerator<T> : MongoCursorEnumerator<T>, IEnumerator, IDisposable
    {
        private bool _started;
        private readonly MongoCursor<T> _cursor;
        private IMongoDbProfiler _profiler;

        public ProfiledMongoCursorEnumerator(MongoCursor<T> cursor, IMongoDbProfiler profiler) 
            : base(cursor)
        {
            _cursor = cursor;
            _profiler = profiler;
        }

        bool IEnumerator.MoveNext()
        {
            if (_profiler == null) return base.MoveNext();

            if (!_started) _profiler.ExecuteStart(_cursor.Collection.Name + ".find", _cursor.Query, ExecuteType.Reader);
            var result = base.MoveNext();
            if (!_started) _profiler.ExecuteFinish(_cursor.Query, ExecuteType.Reader, _cursor);
            
            if (!result)
            {
                _profiler.ReaderFinish(_cursor);
                _profiler = null;
            }
            _started = true;
            return result;
        }
        
        void IDisposable.Dispose()
        {
            if (_profiler != null) _profiler.ReaderFinish(_cursor);
            base.Dispose();
        }
    }
}
