using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace System.Json
{
    class Program
    {
        static void Main(string[] args)
        {
            dynamic ret;

            try
            {
                ret = JsonParser.Parse("2.99");
                Console.WriteLine(JsonParser.ToJsonString(ret));

                ret = JsonParser.Parse("{ \"hoge\": true}");
                Console.WriteLine(JsonParser.ToJsonString(ret));

                ret = JsonParser.Parse("1");
                Console.WriteLine(JsonParser.ToJsonString(ret));
            }
            catch (XmlException e)
            {
                Console.WriteLine(e);
            }
        }
    }
    public static class JsonParser
    {
        static char[] spaces = new char[] { ' ', '\t', '\r' };
        static char[] boolCandidates = new char[] { 't', 'f' };

        static public dynamic Parse(string jsonStr)
        {
            char[] jsonChar = jsonStr.ToCharArray();
            int left = 0;
            int right = jsonChar.Length;
            dynamic ret = ParseJsonBasicValue(jsonChar, ref left, right);
            if (left != right)
            {
                throw new XmlException(DumpInvalidJson(jsonChar, left, right));
            }
            return ret;
        }

        static private void SkipWhites(char[] jsonStr, ref int left, int right)
        {
            while (left < right)
            {
                if (!spaces.Contains(jsonStr[left]))
                {
                    break;
                }
                left++;
            }
        }

        static private dynamic ParseJsonBasicValue(char[] jsonChar, ref int left, int right)
        {
            JsonBasicValue node = null;
            dynamic ret;
            SkipWhites(jsonChar, ref left, right);

            char target = jsonChar[left];

            if (target == 'n')
            {
                node = new JsonNull();
            }
            else if (boolCandidates.Contains(target))
            {
                node = new JsonBool();
            }
            else if (target == '"')
            {
                node = new JsonString();
            }
            else if (target == '[')
            {
                int tleft = left + 1;
                ret = ParseJsonList(jsonChar, ref tleft, right);
                if (jsonChar[tleft] != ']')
                {
                    throw new XmlException("Invalid json. (List)");
                }
                left = tleft + 1;

                SkipWhites(jsonChar, ref left, right);
                return ret;
            }
            else if (target == '{')
            {
                int tleft = left + 1;
                ret = ParseJsonObject(jsonChar, ref tleft, right);
                if (jsonChar[tleft] != '}')
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, tleft, right));
                }
                left = tleft + 1;
                return ret;
            }
            else
            {
                node = new JsonNumber();
            }
            char[] remainChars = new char[right - left];
            Array.Copy(jsonChar, left, remainChars, 0, right - left);
            int length;
            try
            {
                ret = node.Get(new String(remainChars), out length);
            }
            catch (XmlException)
            {
                throw new XmlException(DumpInvalidJson(jsonChar, left, right));
            }
            catch (InvalidCastException)
            {
                throw new XmlException(DumpInvalidJson(jsonChar, left, right));
            }
            left += length;
            SkipWhites(jsonChar, ref left, right);

            return ret;
        }

        static private List<dynamic> ParseJsonList(char[] jsonChar, ref int left, int right)
        {
            List<dynamic> ret = new List<dynamic>();
            SkipWhites(jsonChar, ref left, right);
            if (left >= right)
            {
                throw new XmlException(DumpInvalidJson(jsonChar, left, right));
            }
            if (jsonChar[left] == ']')
            {
                SkipWhites(jsonChar, ref left, right);
                return ret;
            }

            while (left < right)
            {
                char target = jsonChar[left];

                dynamic val = ParseJsonBasicValue(jsonChar, ref left, right);
                ret.Add(val);

                if (left >= right)
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, right, right));
                }

                target = jsonChar[left];
                if (target == ']')
                {
                    break;
                }
                if (target != ',')
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, left, right));
                }
                left++;
            }
            return ret;
        }

        static private Dictionary<string, dynamic> ParseJsonObject(char[] jsonChar, ref int left, int right)
        {
            JsonString node;
            Dictionary<string, dynamic> ret = new Dictionary<string, dynamic>();

            SkipWhites(jsonChar, ref left, right);

            if (left >= right)
            {
                throw new XmlException(DumpInvalidJson(jsonChar, right, right));
            }

            if (jsonChar[left] == '}')
            {
                return ret;
            }

            while (left < right)
            {
                char target = jsonChar[left];

                node = new JsonString();
                char[] remainChars = new char[right - left];
                Array.Copy(jsonChar, left, remainChars, 0, right - left);
                int length;
                string str;
                try
                {
                    str = node.Get(new String(remainChars), out length);
                }
                catch (XmlException)
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, left, right));
                }
                left += length;
                SkipWhites(jsonChar, ref left, right);

                if (left >= right)
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, right, right));
                }

                if (jsonChar[left] != ':')
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, left, right));
                }
                left++;
                SkipWhites(jsonChar, ref left, right);

                if (left >= right)
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, right, right));
                }

                dynamic val = ParseJsonBasicValue(jsonChar, ref left, right);
                ret.Add(str, val);

                SkipWhites(jsonChar, ref left, right);

                if (left >= right)
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, right, right));
                }

                target = jsonChar[left];
                if (target == '}')
                {
                    break;
                }
                if (target != ',')
                {
                    throw new XmlException(DumpInvalidJson(jsonChar, left, right));
                }
                left++;
            }
            return ret;
        }

        static private string DumpInvalidJson(char[] jsonChar, int position, int right)
        {
            int printPosition = (position > 10) ? 10 : position;
            int printStart = (position > 10) ? position - 10 : 0;
            int printEnd = (printStart + 20 > right) ? right : printStart + 20;
            char[] buf = new char[printEnd - printStart];
            Array.Copy(jsonChar, printStart, buf, 0, printEnd - printStart);

            string dumpedJson = new String(buf);
            string positionStr = new String(' ', printPosition) + "^ here";

            return "Invalid Json.\n" + dumpedJson + "\n" + positionStr;
        }

        /// <summary>
        /// Convert dynamic object into JSON String.
        /// dynamic object must be one of JsonValues below:
        /// JsonValues:
        ///     Null: null,
        ///     Boolean: true or false
        ///     Number: 123, -10, 5.8, 2.997e-8
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        static public string ToJsonString(dynamic val)
        {
            if (val == null)
            {
                return "null";
            }
            else if (typeof(bool) == val.GetType())
            {
                return val ? "true" : "false";
            }
            else if (typeof(string) == val.GetType())
            {
                return val;
            }
            else if (typeof(int) == val.GetType() || typeof(long) == val.GetType())
            {
                return "" + val;
            }
            else if (typeof(double) == val.GetType())
            {
                string str = "" + val;
                return str.Replace('E', 'e');
            }
            else if (typeof(List<dynamic>) == val.GetType())
            {
                return "[" + String.Join(", ", Enumerable.Cast<object>(val)) + "]";
            }
            else if (typeof(Dictionary<string, dynamic>) == val.GetType())
            {
                List<String> strs = new List<String>();
                foreach (KeyValuePair<string, dynamic> kvp in val)
                {
                    strs.Add(kvp.Key + ": " + ToJsonString(kvp.Value));
                }
                return "{" + String.Join(", ", strs) + "}";
            }
            else
            {
                return val.ToString();
            }
        }
    }
}
