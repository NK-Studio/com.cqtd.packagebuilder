using System;

namespace Fixed.UnityEditorInternal {
    class JSONParseException : Exception
    {
        public JSONParseException(string msg) : base(msg)
        {
        }
    }
}