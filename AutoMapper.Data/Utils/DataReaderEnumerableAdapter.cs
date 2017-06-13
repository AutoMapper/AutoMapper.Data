using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace AutoMapper.Data.Utils
{
    public class DataReaderEnumerableAdapter : IEnumerable<IDataRecord>
    {
        private readonly IDataReader _reader;

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
        private bool _disposedValue; // To detect redundant calls

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
            if (!_disposedValue)
            {
                _reader = null;

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
