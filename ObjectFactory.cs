using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace GenericObjectsFramework
{
    public abstract class ObjectFactory : IObjectFactory
    {
        //private IDataStore m_iDataStore = null;
        //private ObjectContext m_contextData = null;
        private EntityDataStoreCollection m_collDataStores = new EntityDataStoreCollection();
        private Dictionary<string, ObjectKey> m_dictKeys = new Dictionary<string, ObjectKey>();

        protected Dictionary<string, ObjectKey> Keys { get { return m_dictKeys; } }

        #region IObjectFactory Members

        public Type Type { get { return GetClassType(); } }
        public string SchemaName { get { return GetSchemaName(); } }
        public string ClassName { get { return GetClassName(); } }

        //public IDataStore DataStore { get { return m_iDataStore; } set { m_iDataStore = (IDataStore)value; } }
        //public ObjectContext DataContext { get { return m_contextData; } set { m_contextData = (ObjectContext)value; } }
        public EntityDataStoreCollection DataStores { get { return m_collDataStores; } set { m_collDataStores = value; } }

        public virtual EntityObject CreateEntityObject()
        {
            throw new NotImplementedException();
        }

        public virtual EntityObject GetEntityObject(object _objKeyValue, string strDataStoreName)
        {
            throw new NotImplementedException();
        }

        public virtual DataObject CreateDataObject()
        {
            throw new NotImplementedException();
        }

        public virtual DataObject GetDataObject(object _objKeyValue)
        {
            throw new NotImplementedException();
        }

        public virtual ModelObject CreateModelObject()
        {
            throw new NotImplementedException();
        }

        public virtual ModelObject CreateLiteObject()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected virtual Type GetClassType()
        {
            throw new NotImplementedException();
        }

        protected virtual string GetSchemaName()
        {
            throw new NotImplementedException();
        }

        protected virtual string GetClassName()
        {
            throw new NotImplementedException();
        }

        public string BuildSelect()
        {
            return BuildSelect(null, null);
        }

        public string BuildSelect(string _strWhere)
        {
            return BuildSelect(_strWhere, null);
        }

        public string BuildSelect(string _strWhere, string _strOrderBy)
        {
            bool bFirstClass = true;
            string strFields = string.Empty;
            string strJoins = string.Empty;
            string strSelect = string.Empty;
            string strWhere = string.Empty;
            ObjectKey keyThis = null;
            ObjectKey keyPrev = null;

            Type typeStep = this.Type;
            while (typeStep != null && !typeStep.Equals(typeof(DataObject)))
            {
                if (this.Keys.Count == 1)
                {
                    strFields = "*";
                    break;
                }
                else if (this.Keys.Count > 1)
                {
                    if (this.Keys.ContainsKey(typeStep.Name))
                    {
                        keyPrev = keyThis;
                        keyThis = this.Keys[typeStep.Name];

                        if (!bFirstClass) strFields += ", ";

                        strFields = string.Format("[{0}].*", typeStep.Name);

                        if (!typeStep.Name.Equals(this.ClassName) && keyPrev != null)
                        {
                            if (!bFirstClass) strJoins += " ";
                            strJoins += string.Format("INNER JOIN [{0}].[{1}] ON [{0}].[{1}].[{2}] = [{3}].[{4}].[{5}]", keyThis.SchemaName, keyThis.ClassName, keyThis.KeyName, keyPrev.SchemaName, keyPrev.ClassName, keyPrev.KeyName);
                        }

                        bFirstClass = false;
                    }
                }
            }

            if (strFields.Length > 0)
            {
                if (this.SchemaName.Length > 0)
                    strSelect = string.Format("SELECT {0} FROM [{1}].[{2}]", strFields, this.SchemaName, this.ClassName);
                else
                    strSelect = string.Format("SELECT {0} FROM [{1}]", strFields, this.ClassName);
                if (strJoins.Length > 0) strSelect += string.Format(" {0}", strJoins);
                if (_strWhere != null && !_strWhere.Equals(string.Empty)) strSelect += string.Format(" WHERE {0}", _strWhere);
                if (_strOrderBy != null && !_strOrderBy.Equals(string.Empty)) strSelect += string.Format(" ORDER BY {0}", _strOrderBy);
            }

            return strSelect;
        }
    }
}
