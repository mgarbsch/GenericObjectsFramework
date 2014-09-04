using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Interface to create objects of both entity and model types.
    /// </summary>
    public interface IObjectFactory
    {
        string SchemaName { get; }
        string ClassName { get; }
        Type Type { get; }
        //IDataStore DataStore { get; set; }
        //ObjectContext DataContext { get; set; }
        EntityDataStoreCollection DataStores { get; set; }

        EntityObject CreateEntityObject();
        EntityObject GetEntityObject(object _objKeyValue, string strDataStoreName);
        DataObject CreateDataObject();
        DataObject GetDataObject(object _objKeyValue);
        ModelObject CreateModelObject();
        ModelObject CreateLiteObject();
    }
}
