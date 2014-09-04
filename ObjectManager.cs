using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace GenericObjectsFramework
{
    public abstract class ObjectManager
    {
        private EntityDataStoreCollection m_collDataStores = new EntityDataStoreCollection();
        private Dictionary<string, IObjectFactory> m_dictFactories = new Dictionary<string, IObjectFactory>();

        public EntityDataStoreCollection DataStores { get { return m_collDataStores; } }
        public Dictionary<string, IObjectFactory> Factories { get { return m_dictFactories; } }

        public ObjectManager()
        {
        }

        public string GetDataStoreName(Type typeEntity)
        {
            string ret = string.Empty;

            foreach (EntityDataStore store in this.DataStores)
            {
                if (typeEntity.FullName.Contains(store.Name))
                {
                    ret = store.Name;
                    break;
                }
            }

            return ret;
        }

        public virtual ModelObject CreateModelObject(string _strClassName)
        {
            ModelObject ret = null;
            IObjectFactory factory = GetFactory(_strClassName);
            if (factory != null) ret = factory.CreateModelObject();
            return ret;
        }

        public virtual DataObject CreateDataObject(string _strClassName)
        {
            return CreateDataObject(_strClassName, false);
        }
        public virtual DataObject CreateDataObject(string _strClassName, bool _bAssignKey)
        {
            DataObject ret = null;
            IObjectFactory factory = GetFactory(_strClassName);
            if (factory != null)
            {
                ret = factory.CreateDataObject();
                if (_bAssignKey && ret != null)
                {
                    AssignKeyValue(ret);
                }
            }
            return ret;
        }

        public virtual DataObject GetDataObject(string _strClassName, object _objKeyValue)
        {
            DataObject ret = null;
            IObjectFactory factory = GetFactory(_strClassName);
            if (factory != null) ret = factory.GetDataObject(_objKeyValue);
            return ret;
        }

        public virtual EntityObject CreateEntityObject(string _strClassName)
        {
            return CreateEntityObject(_strClassName, false);
        }
        public virtual EntityObject CreateEntityObject(string _strClassName, bool _bAssignKey)
        {
            EntityObject ret = null;
            IObjectFactory factory = GetFactory(_strClassName);
            if (factory != null)
            {
                ret = factory.CreateEntityObject();
                if (_bAssignKey && ret != null)
                {
                    AssignKeyValue(ret);
                }
            }
            return ret;
        }

        public virtual EntityObject GetEntityObject(string _strClassName, object _objKeyValue, string strDataStoreName)
        {
            EntityObject ret = null;
            IObjectFactory factory = GetFactory(_strClassName);
            if (factory != null) ret = factory.GetEntityObject(_objKeyValue, strDataStoreName);
            return ret;
        }

        protected IObjectFactory GetFactory(string _strClassName)
        {
            IObjectFactory factory = null;
            if (Factories.ContainsKey(_strClassName))
            {
                factory = Factories[_strClassName];
            }
            else
            {
                factory = CreateFactory(_strClassName);
                Factories.Add(_strClassName, factory);
            }
            return factory;
        }

        protected virtual IObjectFactory CreateFactory(string _strClassName)
        {
            return null;
        }

        protected virtual void AssignKeyValue(object obj)
        {
        }

        protected virtual object GetNextKeyValue(string _strClassName)
        {
            return null;
        }

        protected virtual object UseNextKeyValue(string _strClassName)
        {
            return null;
        }

        public void SynchStores()
        {
            // TODO
        }
    }
}
