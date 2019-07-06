using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace System.Json
{
    abstract public class JsonBasicValue
    {
        protected Regex m_regex;
        abstract public dynamic Get(string str, out int length);
    }

    public class JsonNumber : JsonBasicValue
    {
        private Regex m_regexFloatNormal;
        private Regex m_regexFloatZero;
        public JsonNumber()
        {
            m_regex = new Regex(@"-?[1-9]\d{0,}");
            m_regexFloatNormal = new Regex(@"-?[1-9]\d*\.\d{1,}(e[+\-]?\d{1,})?");
            m_regexFloatZero = new Regex(@"-?0\.\d{1,}(e[+\-]\d{1,})?");
        }
        public override dynamic Get(string str, out int length)
        {
            bool isParsed;
            Match ret;

            // floating point number?
            ret = m_regexFloatNormal.Match(str);
            if (ret.Success)
            {
                double result;
                isParsed = double.TryParse(ret.Value, out result);
                if (isParsed)
                {
                    length = ret.Length;
                    return result;
                }
            }
            ret = m_regexFloatZero.Match(str);
            if (ret.Success)
            {
                double result;
                isParsed = double.TryParse(ret.Value, out result);
                if (isParsed)
                {
                    length = ret.Length;
                    return result;
                }
            }

            // integer?
            ret = m_regex.Match(str);
            if (ret.Success)
            {
                int intResult;
                isParsed = int.TryParse(ret.Value, out intResult);
                if (isParsed)
                {
                    length = ret.Length;
                    return intResult;
                }

                long longResult;
                isParsed = long.TryParse(ret.Value, out longResult);
                if (isParsed)
                {
                    length = ret.Length;
                    return longResult;
                }
            }
            throw new InvalidCastException("Cannot cast " + ret.Value);
        }
    }

    public class JsonString : JsonBasicValue
    {
        public JsonString()
        {
            m_regex = new Regex(@"""([0-9a-zA-Z!@#$%^&=+_,<>^\t\n\r\b.\?\*\(\)\-\+]|\\""|\\u[0-9a-f]{4})+""");
        }
        public override dynamic Get(string str, out int length)
        {
            Match ret;

            // floating point number?
            ret = m_regex.Match(str);
            if (ret.Success)
            {
                length = ret.Length;
                return ret.Value;
            }
            throw new XmlException("Invalid json.");
        }
    }
    public class JsonBool : JsonBasicValue
    {
        public JsonBool()
        {
            m_regex = new Regex(@"(true|false)");
        }
        public override dynamic Get(string str, out int length)
        {
            Match ret;

            // floating point number?
            ret = m_regex.Match(str);
            if (ret.Success)
            {
                bool result;
                length = ret.Length;
                bool isParsed = bool.TryParse(ret.Value, out result);
                if (isParsed)
                {
                    return result;
                }
            }
            throw new XmlException("Invalid json.");
        }
    }
    public class JsonNull : JsonBasicValue
    {
        public JsonNull()
        {
            m_regex = new Regex(@"null");
        }

        public override dynamic Get(string str, out int length)
        {
            Match ret;

            // floating point number?
            ret = m_regex.Match(str);
            if (ret.Success)
            {
                length = ret.Length;
                return null;
            }
            throw new XmlException("Invalid json.");
        }
    }
}
