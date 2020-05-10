// Decompiled with JetBrains decompiler
// Type: UnityEditor.Json
// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B74787E-5869-4CDD-A2C2-41162159B3B7
// Assembly location: C:\Program Files\Unity\Hub\Editor\2019.3.0f1\Editor\Data\Managed\UnityEditor.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQ.PackageManager
{
  internal static class Json
  {
    public static object Deserialize(string json)
    {
      if (json == null)
        return (object) null;
      return Json.Parser.Parse(json);
    }

    public static string Serialize(object obj, bool pretty = false, string indentText = "  ")
    {
      return Json.Serializer.Serialize(obj, pretty, indentText);
    }

    private sealed class Parser : IDisposable
    {
      private const string WORD_BREAK = "{}[],:\"";
      private StringReader json;

      public static bool IsWordBreak(char c)
      {
        return char.IsWhiteSpace(c) || "{}[],:\"".IndexOf(c) != -1;
      }

      private Parser(string jsonString)
      {
        this.json = new StringReader(jsonString);
      }

      public static object Parse(string jsonString)
      {
        using (Json.Parser parser = new Json.Parser(jsonString))
          return parser.ParseValue();
      }

      public void Dispose()
      {
        this.json.Dispose();
        this.json = (StringReader) null;
      }

      private Dictionary<string, object> ParseObject()
      {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        this.json.Read();
        while (true)
        {
          Json.Parser.TOKEN nextToken;
          do
          {
            nextToken = this.NextToken;
            if (nextToken != Json.Parser.TOKEN.NONE)
            {
              if (nextToken == Json.Parser.TOKEN.CURLY_CLOSE)
                goto label_4;
            }
            else
              goto label_3;
          }
          while (nextToken == Json.Parser.TOKEN.COMMA);
          string index = this.ParseString();
          if (index != null && this.NextToken == Json.Parser.TOKEN.COLON)
          {
            this.json.Read();
            dictionary[index] = this.ParseValue();
          }
          else
            goto label_6;
        }
label_3:
        return (Dictionary<string, object>) null;
label_4:
        return dictionary;
label_6:
        return (Dictionary<string, object>) null;
      }

      private List<object> ParseArray()
      {
        List<object> objectList = new List<object>();
        this.json.Read();
        bool flag = true;
        while (flag)
        {
          Json.Parser.TOKEN nextToken = this.NextToken;
          switch (nextToken)
          {
            case Json.Parser.TOKEN.NONE:
              return (List<object>) null;
            case Json.Parser.TOKEN.SQUARED_CLOSE:
              flag = false;
              break;
            case Json.Parser.TOKEN.COMMA:
              continue;
            default:
              object byToken = this.ParseByToken(nextToken);
              objectList.Add(byToken);
              break;
          }
        }
        return objectList;
      }

      private object ParseValue()
      {
        return this.ParseByToken(this.NextToken);
      }

      private object ParseByToken(Json.Parser.TOKEN token)
      {
        switch (token)
        {
          case Json.Parser.TOKEN.CURLY_OPEN:
            return (object) this.ParseObject();
          case Json.Parser.TOKEN.SQUARED_OPEN:
            return (object) this.ParseArray();
          case Json.Parser.TOKEN.STRING:
            return (object) this.ParseString();
          case Json.Parser.TOKEN.NUMBER:
            return this.ParseNumber();
          case Json.Parser.TOKEN.TRUE:
            return (object) true;
          case Json.Parser.TOKEN.FALSE:
            return (object) false;
          case Json.Parser.TOKEN.NULL:
            return (object) null;
          default:
            return (object) null;
        }
      }

      private string ParseString()
      {
        StringBuilder stringBuilder = new StringBuilder();
        this.json.Read();
        bool flag = true;
        while (flag)
        {
          if (this.json.Peek() == -1)
            break;
          char nextChar1 = this.NextChar;
          switch (nextChar1)
          {
            case '"':
              flag = false;
              break;
            case '\\':
              if (this.json.Peek() == -1)
              {
                flag = false;
                break;
              }
              char nextChar2 = this.NextChar;
              switch (nextChar2)
              {
                case '"':
                case '/':
                case '\\':
                  stringBuilder.Append(nextChar2);
                  break;
                case 'b':
                  stringBuilder.Append('\b');
                  break;
                case 'f':
                  stringBuilder.Append('\f');
                  break;
                case 'n':
                  stringBuilder.Append('\n');
                  break;
                case 'r':
                  stringBuilder.Append('\r');
                  break;
                case 't':
                  stringBuilder.Append('\t');
                  break;
                case 'u':
                  char[] chArray = new char[4];
                  for (int index = 0; index < 4; ++index)
                    chArray[index] = this.NextChar;
                  stringBuilder.Append((char) Convert.ToInt32(new string(chArray), 16));
                  break;
              }
              stringBuilder.Append(nextChar1);
              break;
            default:
              stringBuilder.Append(nextChar1);
              break;
          }
        }
        return stringBuilder.ToString();
      }

      private object ParseNumber()
      {
        string nextWord = this.NextWord;
        if (nextWord.IndexOf('.') == -1)
        {
          long result;
          long.TryParse(nextWord, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result);
          return (object) result;
        }
        double result1;
        double.TryParse(nextWord, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result1);
        return (object) result1;
      }

      private void EatWhitespace()
      {
        while (char.IsWhiteSpace(this.PeekChar))
        {
          this.json.Read();
          if (this.json.Peek() == -1)
            break;
        }
      }

      private char PeekChar
      {
        get
        {
          return Convert.ToChar(this.json.Peek());
        }
      }

      private char NextChar
      {
        get
        {
          return Convert.ToChar(this.json.Read());
        }
      }

      private string NextWord
      {
        get
        {
          StringBuilder stringBuilder = new StringBuilder();
          while (!Json.Parser.IsWordBreak(this.PeekChar))
          {
            stringBuilder.Append(this.NextChar);
            if (this.json.Peek() == -1)
              break;
          }
          return stringBuilder.ToString();
        }
      }

      private Json.Parser.TOKEN NextToken
      {
        get
        {
          this.EatWhitespace();
          if (this.json.Peek() == -1)
            return Json.Parser.TOKEN.NONE;
          switch (this.PeekChar)
          {
            case '"':
              return Json.Parser.TOKEN.STRING;
            case ',':
              this.json.Read();
              return Json.Parser.TOKEN.COMMA;
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
              return Json.Parser.TOKEN.NUMBER;
            case ':':
              return Json.Parser.TOKEN.COLON;
            case '[':
              return Json.Parser.TOKEN.SQUARED_OPEN;
            case ']':
              this.json.Read();
              return Json.Parser.TOKEN.SQUARED_CLOSE;
            case '{':
              return Json.Parser.TOKEN.CURLY_OPEN;
            case '}':
              this.json.Read();
              return Json.Parser.TOKEN.CURLY_CLOSE;
            default:
              string nextWord = this.NextWord;
              if (nextWord == "false")
                return Json.Parser.TOKEN.FALSE;
              if (nextWord == "true")
                return Json.Parser.TOKEN.TRUE;
              return nextWord == "null" ? Json.Parser.TOKEN.NULL : Json.Parser.TOKEN.NONE;
          }
        }
      }

      private enum TOKEN
      {
        NONE,
        CURLY_OPEN,
        CURLY_CLOSE,
        SQUARED_OPEN,
        SQUARED_CLOSE,
        COLON,
        COMMA,
        STRING,
        NUMBER,
        TRUE,
        FALSE,
        NULL,
      }
    }

    private sealed class Serializer
    {
      private readonly string indentText;
      private bool pretty;
      private StringBuilder builder;

      private Serializer(bool pretty, string indentText)
      {
        this.builder = new StringBuilder();
        this.pretty = pretty;
        this.indentText = indentText;
      }

      public static string Serialize(object obj, bool pretty, string indentText)
      {
        Json.Serializer serializer = new Json.Serializer(pretty, indentText);
        serializer.SerializeValue(obj, 0);
        return serializer.builder.ToString();
      }

      private void SerializeValue(object value, int indent)
      {
        if (value == null || value is Delegate)
          this.builder.Append("null");
        else if (value is string str)
          this.SerializeString(str);
        else if (value is bool)
          this.builder.Append((bool) value ? "true" : "false");
        else if (value is IList anArray)
          this.SerializeArray(anArray, indent);
        else if (value is IDictionary dictionary)
          this.SerializeObject(dictionary, indent);
        else if (value is char)
          this.SerializeString(new string((char) value, 1));
        else
          this.SerializeOther(value, indent);
      }

      private void SerializeObject(IDictionary obj, int indent)
      {
        if (this.pretty && obj.Keys.Count == 0)
        {
          this.builder.Append("{}");
        }
        else
        {
          bool flag = true;
          string str = (string) null;
          this.builder.Append('{');
          if (this.pretty)
          {
            this.builder.Append('\n');
            str = string.Concat(Enumerable.Repeat<string>(this.indentText, indent).ToArray<string>());
          }
          foreach (object key in (IEnumerable) obj.Keys)
          {
            if (!flag)
            {
              this.builder.Append(',');
              if (this.pretty)
                this.builder.Append('\n');
            }
            if (this.pretty)
            {
              this.builder.Append(str);
              this.builder.Append(this.indentText);
            }
            this.SerializeString(key.ToString());
            this.builder.Append(':');
            if (this.pretty)
              this.builder.Append(' ');
            this.SerializeValue(obj[key], indent + 1);
            flag = false;
          }
          if (this.pretty)
          {
            this.builder.Append('\n');
            this.builder.Append(str);
          }
          this.builder.Append('}');
        }
      }

      private void SerializeArray(IList anArray, int indent)
      {
        if (this.pretty && anArray.Count == 0)
        {
          this.builder.Append("[]");
        }
        else
        {
          bool flag = true;
          string str = (string) null;
          this.builder.Append('[');
          if (this.pretty)
          {
            this.builder.Append('\n');
            str = string.Concat(Enumerable.Repeat<string>(this.indentText, indent).ToArray<string>());
          }
          foreach (object an in (IEnumerable) anArray)
          {
            if (!flag)
            {
              this.builder.Append(',');
              if (this.pretty)
                this.builder.Append('\n');
            }
            if (this.pretty)
            {
              this.builder.Append(str);
              this.builder.Append(this.indentText);
            }
            this.SerializeValue(an, indent + 1);
            flag = false;
          }
          if (this.pretty)
          {
            this.builder.Append('\n');
            this.builder.Append(str);
          }
          this.builder.Append(']');
        }
      }

      private void SerializeString(string str)
      {
        this.builder.Append('"');
        foreach (char ch in str.ToCharArray())
        {
          switch (ch)
          {
            case '\b':
              this.builder.Append("\\b");
              break;
            case '\t':
              this.builder.Append("\\t");
              break;
            case '\n':
              this.builder.Append("\\n");
              break;
            case '\f':
              this.builder.Append("\\f");
              break;
            case '\r':
              this.builder.Append("\\r");
              break;
            case '"':
              this.builder.Append("\\\"");
              break;
            case '\\':
              this.builder.Append("\\\\");
              break;
            default:
              int int32 = Convert.ToInt32(ch);
              if (int32 >= 32 && int32 <= 126)
              {
                this.builder.Append(ch);
                break;
              }
              this.builder.Append("\\u");
              this.builder.Append(int32.ToString("x4"));
              break;
          }
        }
        this.builder.Append('"');
      }

      private void SerializeOther(object value, int indent)
      {
        if (value is float)
          this.builder.Append(((float) value).ToString("R", (IFormatProvider) CultureInfo.InvariantCulture));
        else if (value is int || value is uint || (value is long || value is sbyte) || (value is byte || value is short || value is ushort) || value is ulong)
          this.builder.Append(value);
        else if (value is double || value is Decimal)
        {
          this.builder.Append(Convert.ToDouble(value).ToString("R", (IFormatProvider) CultureInfo.InvariantCulture));
        }
        else
        {
          Dictionary<string, object> dictionary = new Dictionary<string, object>();
          foreach (System.Reflection.FieldInfo fieldInfo in ((IEnumerable<System.Reflection.FieldInfo>) value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)).ToList<System.Reflection.FieldInfo>())
            dictionary.Add(fieldInfo.Name, fieldInfo.GetValue(value));
          foreach (PropertyInfo propertyInfo in ((IEnumerable<PropertyInfo>) value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)).ToList<PropertyInfo>())
            dictionary.Add(propertyInfo.Name, propertyInfo.GetValue(value, (object[]) null));
          this.SerializeObject((IDictionary) dictionary, indent);
        }
      }
    }
  }
}
