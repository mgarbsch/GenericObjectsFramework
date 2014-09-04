using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Interface to provide ModelObjects the method to load child collections. 
    /// </summary>
    public interface IModelObjectProvider
    {
        ModelObject GetByID(object id);
        IEnumerable<ModelObject> GetAll();
        IEnumerable<ModelObject> GetByOwnerID(object id);
        IEnumerable<ModelObject> GetByCommand(string _strCommand, object id);
    }
}
