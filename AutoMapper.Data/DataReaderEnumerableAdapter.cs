using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AutoMapper.Data
{
    public class DataReaderEnumerableAdapter : IEnumerable<IDataRecord>
    {
        private IDataReader _reader;

        public DataReaderEnumerableAdapter(IDataReader reader)
        {
            _reader = reader;
        }

        public IEnumerator<IDataRecord> GetEnumerator()
        {
            return new DataReaderEnumerator(_reader);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class DataReaderEnumerator : IEnumerator<IDataRecord>
    {
        private IDataReader _reader;
        private bool disposedValue = false; // To detect redundant calls

        public DataReaderEnumerator(IDataReader reader)
        {
            _reader = reader;
        }

        public object Current => _reader;

        IDataRecord IEnumerator<IDataRecord>.Current => _reader;

        public bool MoveNext() => _reader?.Read() ?? false;

        public void Reset()
        {
            throw new NotSupportedException("IDataReader can only be used for forward-only operations.");
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _reader = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
