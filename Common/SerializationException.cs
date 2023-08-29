using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stalker_Studio.Common
{
    public class SerializationException : Exception
    {
        int _lineIndex = -1;
        ISerializable _object = null;
        bool _isCritical = true;

        public SerializationException(int lineIndex, ISerializable Object, bool isCritical = true) : base()
        {
            _lineIndex = lineIndex;
            _object = Object;
            _isCritical = isCritical;
        }
        public SerializationException(string message, int lineIndex = -1, ISerializable Object = null, bool isCritical = true) : base(message)
        {
            _lineIndex = lineIndex;
            _object = Object;
            _isCritical = isCritical;
        }
        public SerializationException(string message, Exception innerException, int lineIndex = -1, ISerializable Object = null, bool isCritical = true) : base(message, innerException)
        {
            _lineIndex = lineIndex;
            _object = Object;
            _isCritical = isCritical;
        }

        public int LineIndex { get => _lineIndex; }
        public ISerializable Object { get => _object; }
        public bool IsCritical { get => _isCritical; }
    }
}
