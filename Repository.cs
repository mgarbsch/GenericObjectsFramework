using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace GenericObjectsFramework
{
    public abstract class Repository
    {
        private ObjectManager m_manager = null;

        public ObjectManager Manager { get { return m_manager; } }

        public Repository(ObjectManager _manager)
        {
            m_manager = _manager;
        }

        public virtual void InsertObject(ModelObject entity)
        {
            if (this.Manager != null && this.Manager.DataStores.Count > 0)
            {
                ObjectConverter converter = new ObjectConverter(this.Manager);
                string strClassName = converter.GetClassName(entity.GetType());

                foreach (EntityDataStore store in this.Manager.DataStores)
                {
                    if (store.ObjectContext != null)
                    {
                        if (store.ObjectContext.Connection.State == ConnectionState.Open)
                        {
                            EntityObject objEntity = converter.ConvertToEntity(entity, store.Name);
                            store.ObjectContext.AddObject(strClassName, objEntity);
                        }
                        else
                        {
                            store.Commands.AddInsert(strClassName, store.Name, entity);
                        }
                    }
                }
            }
        }

        public virtual void UpdateObject(ModelObject entity)
        {
            ObjectConverter converter = new ObjectConverter(this.Manager);

            foreach (EntityDataStore store in this.Manager.DataStores)
            {
                if (store.ObjectContext != null)
                {
                    if (store.ObjectContext.Connection.State == ConnectionState.Open)
                    {
                        EntityObject objEntity = converter.ConvertToEntity(entity, store.Name);
                    }
                    else
                    {
                        store.Commands.AddUpdate(entity.ClassName, store.Name, entity);
                    }
                }
            }
        }

        public virtual void DeleteObject(ModelObject entity)
        {
            if (this.Manager != null && this.Manager.DataStores.Count > 0)
            {
                ObjectConverter converter = new ObjectConverter(this.Manager);
                string strClassName = converter.GetClassName(entity.GetType());
                bool bDeleted = false;

                foreach (EntityDataStore store in this.Manager.DataStores)
                {
                    if (store.ObjectContext != null)
                    {
                        bDeleted = false;
                        try
                        {
                            EntityObject objEntity = this.Manager.GetEntityObject(strClassName, entity.ID, store.Name);
                            store.ObjectContext.DeleteObject(objEntity);
                            bDeleted = true;
                        }
                        catch { bDeleted = false; }
                        if (!bDeleted)
                        {
                            store.Commands.AddDelete(entity.ClassName, store.Name, entity.ID);
                        }
                    }
                }
            }
        }

        public virtual ModelObject RefreshObject(ModelObject entity)
        {
            ModelObject ret = null;
            if (this.Manager != null)
            {
                ObjectConverter converter = new ObjectConverter(this.Manager);
                string strClassName = converter.GetClassName(entity.GetType());

                if (this.Manager.DataStores.PrimaryStore != null && this.Manager.DataStores.PrimaryStore.ObjectContext != null)
                {
                    //if (this.Manager.DataStores.PrimaryStore.ObjectContext.Connection.State == ConnectionState.Open)
                    //{
                        EntityObject objEntity = this.Manager.GetEntityObject(strClassName, entity.ID, this.Manager.DataStores.PrimaryStore.Name);
                        ret = converter.ConvertToModel(objEntity);
                    //}
                }
            }
            return ret;
        }

        public virtual void SaveChanges()
        {
            if (this.Manager != null && this.Manager.DataStores.Count > 0)
            {
                foreach (EntityDataStore store in this.Manager.DataStores)
                {
                    if (store.ObjectContext != null) store.ObjectContext.SaveChanges();
                }
            }
        }
    }
}
