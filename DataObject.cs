using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using xUtilities.Attributes;

namespace GenericObjectsFramework
{
    [Serializable]
    public abstract class DataObject : INotifyPropertyChanged
    {
        public const string DATA_OBJECT_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        private string m_strSchemaName = string.Empty;
        private string m_strClassName = string.Empty;
        private Dictionary<string, ObjectKey> m_dictKeys = new Dictionary<string, ObjectKey>();
        private IDataStore m_iDataStore = null;

        [InternalAttribute(true)]
        public string SchemaName { get { return m_strSchemaName; } set { m_strSchemaName = (string)value; } }
        [InternalAttribute(true)]
        public string ClassName { get { return m_strClassName; } set { m_strClassName = (string)value; } }
        [InternalAttribute(true)]
        public Dictionary<string, ObjectKey> Keys { get { return m_dictKeys; } }
        [InternalAttribute(true)]
        public IDataStore DataStore { get { return m_iDataStore; } set { m_iDataStore = (IDataStore)value; } }
        [InternalAttribute(true)]
        public object ObjectID
        {
            get
            {
                object ret = null;
                if (this.Keys.ContainsKey(this.ClassName)) ret = this.Keys[this.ClassName].Value;
                return ret;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = null;

        #endregion

        public DataObject()
        {
        }

        public DataObject(XmlReader _reader)
        {
        }

        public DataObject(DataRow _dr)
        {
        }

        public void Load(XmlReader _reader)
        {
        }

        public void Load(DataRow _dr)
        {
        }

        public void Save(XmlWriter _writer)
        {
        }

        public void Save(DataRow _dr)
        {
        }

        public string BuildXml()
        {
            return string.Empty;
        }

        public Dictionary<string, string> BuildInserts()
        {
            Dictionary<string, string> listInserts = new Dictionary<string, string>();

            bool bFirst = true;
            string strFields = string.Empty;
            string strValues = string.Empty;
            string strInsert = string.Empty;
            object objValue = null;

            PropertyInfo[] pis = null;
            Type typeThis = GetType();
            Type typeStep = typeThis;
            ObjectKey key = null;

            while (typeStep != null && !typeStep.Equals(typeof(DataObject)) && !typeStep.IsInterface)
            {
                bFirst = true;
                strFields = string.Empty;
                strValues = string.Empty;

                pis = typeStep.GetProperties();
                key = Keys[typeStep.Name];

                foreach (PropertyInfo pi in pis)
                {
                    if (!pi.Name.Equals(key.KeyName) && !InternalAttribute.IsInternalProperty(pi))
                    {
                        objValue = pi.GetValue(this, null);

                        if (!bFirst)
                        {
                            strFields += ", ";
                            strValues += ", ";
                        }

                        if (objValue == null)
                        {
                            strValues += "NULL";
                        }
                        else
                        {
                            strFields += string.Format("[{0}]", pi.Name);
                            if (pi.PropertyType.Equals(typeof(string))) strValues += string.Format("'{0}'", Convert.ToString(objValue).Replace("'", "''"));
                            else if (pi.PropertyType.Equals(typeof(DateTime)))
                            {
                                string strFormat = DATA_OBJECT_DATE_FORMAT;
                                if (this.DataStore != null) strFormat = this.DataStore.DateFormat;
                                strValues += string.Format("'{0}'", Convert.ToDateTime(objValue).ToString(strFormat));
                            }
                            else if (pi.PropertyType.Equals(typeof(bool)))
                            {
                                strValues += Convert.ToInt32(Convert.ToBoolean(objValue)).ToString();
                            }
                            else strValues += objValue.ToString();
                            bFirst = false;
                        }

                    }
                }

                if (strFields.Length > 0 && strValues.Length > 0)
                {
                    if (this.SchemaName.Length > 0)
                        strInsert = string.Format("INSERT INTO [{0}].[{1}] ({2}) VALUES ({3})", this.SchemaName, typeStep.Name, strFields, strValues);
                    else
                        strInsert = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", typeStep.Name, strFields, strValues);

                    listInserts.Add(typeStep.Name, strInsert);
                }

                typeStep = typeStep.BaseType;
            }

            return listInserts;
        }

        public Dictionary<string, string> BuildUpdates()
        {
            Dictionary<string, string> ret = null;

            ObjectKey key = GetPrimaryKey();
            if (key != null)
            {
                string strWhere = string.Format("[{0}] = {1}", key.KeyName, (key.Value != null) ? key.Value.ToString() : "0");
                ret = BuildUpdates(strWhere);
            }

            return ret;
        }

        public Dictionary<string, string> BuildUpdates(string _strWhere)
        {
            Dictionary<string, string> listUpdates = new Dictionary<string, string>();

            string strUpdate = string.Empty;

            bool bFirst = true;
            string strValue = string.Empty;
            string strAssignments = string.Empty;
            string strWhere = string.Empty;
            ObjectKey key = null;
            object objValue = null;

            PropertyInfo[] pis = null;
            Type typeThis = GetType();
            Type typeStep = typeThis;

            while (typeStep != null && !typeStep.Equals(typeof(DataObject)) && !typeStep.IsInterface)
            {
                if (this.Keys.ContainsKey(typeStep.Name))
                {
                    key = this.Keys[typeStep.Name];

                    bFirst = true;

                    pis = typeStep.GetProperties();
                    key = Keys[typeStep.Name];

                    foreach (PropertyInfo pi in pis)
                    {
                        if (!pi.Name.Equals(key.KeyName) && !InternalAttribute.IsInternalProperty(pi))
                        {
                            objValue = pi.GetValue(this, null);

                            if (!bFirst) strAssignments += ", ";

                            if (objValue == null)
                            {
                                strValue = "NULL";
                            }
                            else 
                            {
                                switch (pi.PropertyType.FullName)
                                {
                                    case "System.Boolean":
                                        strValue = Convert.ToInt32(Convert.ToBoolean(objValue)).ToString();
                                        break;
                                    case "System.Byte":
                                    case "System.Char":
                                    case "System.Int16":
                                    case "System.Int32":
                                    case "System.Int64":
                                    case "System.UInt16":
                                    case "System.UInt32":
                                    case "System.UInt64":
                                    case "System.Single":
                                    case "System.Double":
                                    case "System.Decimal":
                                        strValue = objValue.ToString();
                                        break;
                                    case "System.String":
                                        strValue = string.Format("'{0}'", Convert.ToString(objValue).Replace("'", "''"));
                                        break;
                                    case "System.DateTime":
                                        {
                                            string strFormat = DATA_OBJECT_DATE_FORMAT;
                                            if (this.DataStore != null) strFormat = this.DataStore.DateFormat;
                                            strValue = string.Format("'{0}'", Convert.ToDateTime(objValue).ToString(strFormat));
                                            break;
                                        }
                                }
                            }
                            strAssignments += string.Format("[{0}] = {1}", pi.Name, strValue);
                            bFirst = false;
                        }
                    }

                    if (strAssignments.Length > 0)
                    {
                        if (typeThis.Name.Equals(key.ClassName)) strWhere = _strWhere;
                        else strWhere = string.Format("[{0}] = {1}", key.ClassName, key.Value);
                        
                        if (key.SchemaName.Length > 0)
                            strUpdate = string.Format("UPDATE [{0}].[{1}] SET {2} WHERE {3}", key.SchemaName, key.ClassName, strAssignments, strWhere);
                        else
                            strUpdate = string.Format("UPDATE [{0}] SET {1} WHERE {2}", key.ClassName, strAssignments, strWhere);
                    }

                    listUpdates.Add(typeStep.Name, strUpdate);
                }

                typeStep = typeStep.BaseType;
            }

            return listUpdates;
        }

        public Dictionary<string, string> BuildDeletes()
        {
            Dictionary<string, string> ret = null;

            ObjectKey key = GetPrimaryKey();
            if (key != null)
            {
                string strWhere = string.Format("[{0}] = {1}", key.KeyName, (key.Value != null) ? key.Value.ToString() : "0");
                ret = BuildDeletes(strWhere);
            }

            return ret;
        }

        public Dictionary<string, string> BuildDeletes(string _strWhere)
        {
            Dictionary<string, string> listDeletes = new Dictionary<string, string>();

            string strDelete = string.Empty;
            string strWhere = string.Empty;

            Type typeThis = GetType();
            Type typeStep = typeThis;
            ObjectKey key = null;

            while (typeStep != null && !typeStep.Equals(typeof(DataObject)) && !typeStep.IsInterface)
            {
                key = GetPrimaryKey(typeStep);
                if (key != null)
                {
                    if (key.KeyName.Equals(this.ClassName)) strWhere = _strWhere;
                    else strWhere = string.Format("[{0}] = {1}", key.KeyName, key.Value);

                    if (key.SchemaName.Length > 0)
                        strDelete = string.Format("DELETE FROM [{0}].[{1}] WHERE {2}", key.SchemaName, key.ClassName, strWhere);
                    else
                        strDelete = string.Format("DELETE FROM [{0}] WHERE {1}", key.ClassName, strWhere);

                    listDeletes.Add(key.ClassName, strDelete);
                }

                typeStep = typeStep.BaseType;
            }

            return listDeletes;
        }

        protected ObjectKey GetPrimaryKey()
        {
            return GetPrimaryKey(GetType());
        }

        protected ObjectKey GetPrimaryKey(Type type)
        {
            ObjectKey ret = null;

            string strTableName = TableNameAttribute.GetTableName(type);
            string strClassName = (strTableName.Length > 0) ? strTableName : type.Name;

            if (this.Keys.ContainsKey(strClassName))
            {
                ret = this.Keys[strClassName];
            }

            return ret;
        }

        protected void RaisePropertyChanged(string _strPropertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(_strPropertyName));
        }

        protected void OnPropertyChanged(string _strPropertyName)
        {
        }
    }
}
