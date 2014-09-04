using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Interface providing access to repository's base functionality.
    /// </summary>
    /// <typeparam name="T">The type handled </typeparam>
    public interface IRepository<T> where T : class
    {
        T GetByID(object id);
        IQueryable<T> GetAll();
        IQueryable<T> GetByOwnerID(object idOwner);

        void InsertObject(T entity);
        void UpdateObject(T entity);
        void DeleteObject(T entity);
        T RefreshObject(T entity);

        [Obsolete("Units of Work should be managed externally to the Repository.")]
        void SaveChanges();
    }
}
