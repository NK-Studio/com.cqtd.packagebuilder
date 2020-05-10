using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Fixed.UnityEditorInternal {
    class JSONParser
    {
        private string json;
        private int line;
        private int linechar;
        private int len;
        private int idx;
        private int pctParsed;
        private char cur;

        public static JSONValue SimpleParse(string jsondata)
        {
            var parser = new JSONParser(jsondata);
            try
            {
                return parser.Parse();
            }
            catch (JSONParseException ex)
            {
                Debug.LogError(ex.Message);
            }
            return new JSONValue(null);
        }

        /*
         * Setup a parse to be ready for parsing the given string
         */
        public JSONParser(string jsondata)
        {
            // TODO: fix that parser needs trailing spaces;
            json = jsondata + "    ";
            line = 1;
            linechar = 1;
            len = json.Length;
            idx = 0;
            pctParsed = 0;
        }

        /*
         * Parse the entire json data string into a JSONValue structure hierarchy
         */
        public JSONValue Parse()
        {
            cur = json[idx];
            return ParseValue();
        }

        private char Next()
        {
            if (cur == '\n')
            {
                line++;
                linechar = 0;
            }
            idx++;
            if (idx >= len)
                throw new JSONParseException("End of json while parsing at " + PosMsg());

            linechar++;

            int newPct = (int)((float)idx * 100f / (float)len);
            if (newPct != pctParsed)
            {
                pctParsed = newPct;
            }
            cur = json[idx];
            return cur;
        }

        private void SkipWs()
        {
            string ws = " \n\t\r";
            while (ws.IndexOf(cur) != -1) Next();
        }

        private string PosMsg()
        {
            return "line " + line + ", column " + linechar;
        }

        private JSONValue ParseValue()
        {
            // Skip spaces
            SkipWs();

            switch (cur)
            {
                case '[':
                    return ParseArray();
                case '{':
                    return ParseDict();
                case '"':
                    return ParseString();
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ParseNumber();
                case 't':
                case 'f':
                case 'n':
                    return ParseConstant();
                default:
                    throw new JSONParseException("Cannot parse json value starting with '" + json.Substring(idx, 5) + "' at " + PosMsg());
            }
        }

        private JSONValue ParseArray()
        {
            Next();
            SkipWs();
            List<JSONValue> arr = new List<JSONValue>();
            while (cur != ']')
            {
                arr.Add(ParseValue());
                SkipWs();
                if (cur == ',')
                {
                    Next();
                    SkipWs();
                }
            }
            Next();
            return new JSONValue(arr);
        }

        private JSONValue ParseDict()
        {
            Next();
            SkipWs();
            Dictionary<string, JSONValue> dict = new Dictionary<string, JSONValue>();
            while (cur != '}')
            {
                JSONValue key = ParseValue();
                if (!key.IsString())
                    throw new JSONParseException("Key not string type at " + PosMsg());
                SkipWs();
                if (cur != ':')
                    throw new JSONParseException("Missing dict entry delimiter ':' at " + PosMsg());
                Next();
                dict.Add(key.AsString(), ParseValue());
                SkipWs();
                if (cur == ',')
                {
                    Next();
                    SkipWs();
                }
            }
            Next();
            return new JSONValue(dict);
        }

        static char[] endcodes = new char[] { '\\', '"' };

        private JSONValue ParseString()
        {
            string res = "";

            Next();

            while (idx < len)
            {
                int endidx = json.IndexOfAny(endcodes, idx);
                if (endidx < 0)
                    throw new JSONParseException("missing '\"' to end string at " + PosMsg());

                res += json.Substring(idx, endidx - idx);

                if (json[endidx] == '"')
                {
                    cur = json[endidx];
                    idx = endidx;
                    break;
                }

                endidx++; // get escape code
                if (endidx >= len)
                    throw new JSONParseException("End of json while parsing while parsing string at " + PosMsg());

                // char at endidx is \
                char ncur = json[endidx];
                switch (ncur)
                {
                    case '"':
                        goto case '/';
                    case '\\':
                        goto case '/';
                    case '/':
                        res += ncur;
                        break;
                    case 'b':
                        res += '\b';
                        break;
                    case 'f':
                        res += '\f';
                        break;
                    case 'n':
                        res += '\n';
                        break;
                    case 'r':
                        res += '\r';
                        break;
                    case 't':
                        res += '\t';
                        break;
                    case 'u':
                        // Unicode char specified by 4 hex digits
                        string digit = "";
                        if (endidx + 4 >= len)
                            throw new JSONParseException("End of json while parsing while parsing unicode char near " + PosMsg());
                        digit += json[endidx + 1];
                        digit += json[endidx + 2];
                        digit += json[endidx + 3];
                        digit += json[endidx + 4];
                        try
                        {
                            int d = System.Int32.Parse(digit, System.Globalization.NumberStyles.AllowHexSpecifier);
                            res += (char)d;
                        }
                        catch (FormatException)
                        {
                            throw new JSONParseException("Invalid unicode escape char near " + PosMsg());
                        }
                        endidx += 4;
                        break;
                    default:
                        throw new JSONParseException("Invalid escape char '" + ncur + "' near " + PosMsg());
                }
                idx = endidx + 1;
            }
            if (idx >= len)
                throw new JSONParseException("End of json while parsing while parsing string near " + PosMsg());

            cur = json[idx];

            Next();
            return new JSONValue(res);
        }

        private JSONValue ParseNumber()
        {
            string resstr = "";

            if (cur == '-')
            {
                resstr = "-";
                Next();
            }

            while (cur >= '0' && cur <= '9')
            {
                resstr += cur;
                Next();
            }
            if (cur == '.')
            {
                Next();
                resstr += '.';
                while (cur >= '0' && cur <= '9')
                {
                    resstr += cur;
                    Next();
                }
            }

            if (cur == 'e' || cur == 'E')
            {
                resstr += "e";
                Next();
                if (cur != '-' && cur != '+')
                {
                    // throw new JSONParseException("Missing - or + in 'e' potent specifier at " + PosMsg());
                    resstr += cur;
                    Next();
                }
                while (cur >= '0' && cur <= '9')
                {
                    resstr += cur;
                    Next();
                }
            }

            try
            {
                float f = System.Convert.ToSingle(resstr, CultureInfo.InvariantCulture);
                return new JSONValue(f);
            }
            catch (Exception)
            {
                throw new JSONParseException("Cannot convert string to float : '" + resstr + "' at " + PosMsg());
            }
        }

        private JSONValue ParseConstant()
        {
            string c = "";
            c = "" + cur + Next() + Next() + Next();
            Next();
            if (c == "true")
            {
                return new JSONValue(true);
            }
            else if (c == "fals")
            {
                if (cur == 'e')
                {
                    Next();
                    return new JSONValue(false);
                }
            }
            else if (c == "null")
            {
                return new JSONValue(null);
            }
            throw new JSONParseException("Invalid token at " + PosMsg());
        }
    }
}