using System.Collections.Generic;

namespace Fixed.UnityEditorInternal {
    internal struct JSONValue
    {
        public JSONValue(object o)
        {
            data = o;
        }

        public bool IsString() { return data is string; }
        public bool IsFloat() { return data is float; }
        public bool IsList() { return data is List<JSONValue>; }
        public bool IsDict() { return data is Dictionary<string, JSONValue>; }
        public bool IsBool() { return data is bool; }
        public bool IsNull() { return data == null; }

        public static implicit operator JSONValue(string s)
        {
            return new JSONValue(s);
        }

        public static implicit operator JSONValue(float s)
        {
            return new JSONValue(s);
        }

        public static implicit operator JSONValue(bool s)
        {
            return new JSONValue(s);
        }

        public static implicit operator JSONValue(int s)
        {
            return new JSONValue((float)s);
        }

        public object AsObject()
        {
            return data;
        }

        public string AsString(bool nothrow)
        {
            if (data is string)
                return (string)data;
            if (!nothrow)
                throw new JSONTypeException("Tried to read non-string json value as string");
            return "";
        }

        public string AsString()
        {
            return AsString(false);
        }

        public float AsFloat(bool nothrow)
        {
            if (data is float)
                return (float)data;
            if (!nothrow)
                throw new JSONTypeException("Tried to read non-float json value as float");
            return 0.0f;
        }

        public float AsFloat()
        {
            return AsFloat(false);
        }

        public bool AsBool(bool nothrow)
        {
            if (data is bool)
                return (bool)data;
            if (!nothrow)
                throw new JSONTypeException("Tried to read non-bool json value as bool");
            return false;
        }

        public bool AsBool()
        {
            return AsBool(false);
        }

        public List<JSONValue> AsList(bool nothrow)
        {
            if (data is List<JSONValue>)
                return (List<JSONValue>)data;
            if (!nothrow)
                throw new JSONTypeException("Tried to read " + data.GetType().Name + " json value as list");
            return null;
        }

        public List<JSONValue> AsList()
        {
            return AsList(false);
        }

        public Dictionary<string, JSONValue> AsDict(bool nothrow)
        {
            if (data is Dictionary<string, JSONValue>)
                return (Dictionary<string, JSONValue>)data;
            if (!nothrow)
                throw new JSONTypeException("Tried to read non-dictionary json value as dictionary");
            return null;
        }

        public Dictionary<string, JSONValue> AsDict()
        {
            return AsDict(false);
        }

        public static JSONValue NewString(string val)
        {
            return new JSONValue(val);
        }

        public static JSONValue NewFloat(float val)
        {
            return new JSONValue(val);
        }

        public static JSONValue NewDict()
        {
            return new JSONValue(new Dictionary<string, JSONValue>());
        }

        public static JSONValue NewList()
        {
            return new JSONValue(new List<JSONValue>());
        }

        public static JSONValue NewBool(bool val)
        {
            return new JSONValue(val);
        }

        public static JSONValue NewNull()
        {
            return new JSONValue(null);
        }

        public JSONValue this[string index]
        {
            get
            {
                Dictionary<string, JSONValue> dict = AsDict();
                return dict[index];
            }
            set
            {
                if (data == null)
                    data = new Dictionary<string, JSONValue>();
                Dictionary<string, JSONValue> dict = AsDict();
                dict[index] = value;
            }
        }

        public bool ContainsKey(string index)
        {
            if (!IsDict())
                return false;
            return AsDict().ContainsKey(index);
        }

        // Get the specified field in a dict or null json value if
        // no such field exists. The key can point to a nested structure
        // e.g. key1.key2 in  { key1 : { key2 : 32 } }
        public JSONValue Get(string key)
        {
            if (!IsDict())
                return new JSONValue(null);
            JSONValue value = this;
            foreach (string part in key.Split('.'))
            {
                if (!value.ContainsKey(part))
                    return new JSONValue(null);
                value = value[part];
            }
            return value;
        }

        // Convenience dict value setting
        public void Set(string key, string value)
        {
            if (value == null)
            {
                this[key] = NewNull();
                return;
            }
            this[key] = NewString(value);
        }

        // Convenience dict value setting
        public void Set(string key, float value)
        {
            this[key] = NewFloat(value);
        }

        // Convenience dict value setting
        public void Set(string key, bool value)
        {
            this[key] = NewBool(value);
        }

        // Convenience list value add
        public void Add(string value)
        {
            List<JSONValue> list = AsList();
            if (value == null)
            {
                list.Add(NewNull());
                return;
            }
            list.Add(NewString(value));
        }

        // Convenience list value add
        public void Add(float value)
        {
            List<JSONValue> list = AsList();
            list.Add(NewFloat(value));
        }

        // Convenience list value add
        public void Add(bool value)
        {
            List<JSONValue> list = AsList();
            list.Add(NewBool(value));
        }

        /*
         * Serialize a JSON value to string.
         * This will recurse down through dicts and list type JSONValues.
         */
        public override string ToString()
        {
            if (IsString())
            {
                return "\"" + EncodeString(AsString()) + "\"";
            }
            else if (IsFloat())
            {
                return AsFloat().ToString();
            }
            else if (IsList())
            {
                string res = "[";
                string delim = "";
                foreach (JSONValue i in AsList())
                {
                    res += delim + i;
                    delim = ", ";
                }
                return res + "]";
            }
            else if (IsDict())
            {
                string res = "{";
                string delim = "";
                foreach (KeyValuePair<string, JSONValue> kv in AsDict())
                {
                    res += delim + '"' + EncodeString(kv.Key) + "\" : " + kv.Value;
                    delim = ", ";
                }
                return res + "}";
            }
            else if (IsBool())
            {
                return AsBool() ? "true" : "false";
            }
            else if (IsNull())
            {
                return "null";
            }
            else
            {
                throw new JSONTypeException("Cannot serialize json value of unknown type");
            }
        }

        // Encode a string into a json string
        private static string EncodeString(string str)
        {
            str = str.Replace("\"", "\\\"");
            str = str.Replace("\\", "\\\\");
            str = str.Replace("\b", "\\b");
            str = str.Replace("\f", "\\f");
            str = str.Replace("\n", "\\n");
            str = str.Replace("\r", "\\r");
            str = str.Replace("\t", "\\t");
            // We do not use \uXXXX specifier but direct unicode in the string.
            return str;
        }

        object data;
    }
}