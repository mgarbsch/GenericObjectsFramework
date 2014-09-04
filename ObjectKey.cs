using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericObjectsFramework
{
    public class ObjectKey
    {
        private string m_strKeyName = string.Empty;
        private string m_strClassName = string.Empty;
        private string m_strSchemaName = string.Empty;
        private object m_oValue = null;

        public string KeyName { get { return m_strKeyName; } set { m_strKeyName = (string)value; } }
        public string ClassName { get { return m_strClassName; } set { m_strClassName = (string)value; } }
        public string SchemaName { get { return m_strSchemaName; } set { m_strSchemaName = (string)value; } }
        public Type Type { get { return (m_oValue != null) ? m_oValue.GetType() : null; } }
        public object Value { get { return m_oValue; } set { m_oValue = value; } }

        public ObjectKey()
        {
        }

        public ObjectKey(string _strSchemaName, string _strClassName, string _strKeyName, object _oKeyValue)
        {
            m_strSchemaName = _strSchemaName;
            m_strClassName = _strClassName;
            m_strKeyName = _strKeyName;
            m_oValue = _oKeyValue;
        }

        public bool IsEqual(string _strClassName, string _strKeyName)
        {
            return (this.ClassName.Equals(_strClassName) && this.KeyName.Equals(_strKeyName));
        }

        public override bool Equals(object obj)
        {
            bool ret = false;
            if (obj is ObjectKey)
            {
                ObjectKey keyOther = obj as ObjectKey;
                ret = this.IsEqual(keyOther.ClassName, keyOther.KeyName);
            }
            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", this.ClassName, this.KeyName);
        }
    }
}
