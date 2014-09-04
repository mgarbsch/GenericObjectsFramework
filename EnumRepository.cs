using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Interface to provide access methods to the types stored within a repository.
    /// </summary>
    /// <typeparam name="T">The type to be stored within the repository.</typeparam>
    /// <remarks></remarks>
    public interface IEnumRepository<T> where T : class
    {
        Dictionary<int, T> ValuesByID { get; }
        Dictionary<string, T> ValuesByName { get; }
        IEnumerable<string> Names { get; }
        IQueryable<T> GetAll();
        T GetObject(int _nID);

        int GetEnumID(string _strName);
        string GetEnumName(int _nID);
    }

    /// <summary>
    /// Class to provide a repository structure for enumerations.
    /// </summary>
    /// <remarks>Provides loading behaviour and access methods for an enumeration type class.</remarks>
    public abstract class EnumRepository : Repository, IEnumRepository<EnumObject>
    {
        /// <summary>
        /// Declaration of dictionary to store enumeration objects by their IDs.
        /// </summary>
        protected Dictionary<int, EnumObject> m_dictIDs = new Dictionary<int, EnumObject>();
        /// <summary>
        /// Declaration of dictionary to store enumeration objects by their names.
        /// </summary>
        protected Dictionary<string, EnumObject> m_dictNames = new Dictionary<string, EnumObject>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumRepository"/> class.
        /// </summary>
        /// <param name="_manager">The application's ObjectManager.</param>
        /// <remarks>The constructor immediately accesses the data store to load all of its enumeration values.</remarks>
        protected EnumRepository(ObjectManager _manager) : base(_manager)
        {
            IQueryable<EnumObject> objs = GetAll();
            if (objs != null)
            {
                IEnumerable<EnumObject> enumObjs = objs.AsEnumerable();

                if (enumObjs != null)
                {
                    foreach (EnumObject obj in enumObjs)
                    {
                        try
                        {
                            ValuesByID.Add(obj.TypeID, obj);
                            ValuesByName.Add(obj.TypeName, obj);
                            OnAddNewEnumObject(obj);
                        }
                        catch (Exception ex)
                        {
                            int i = 0;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Internal virtual class method called to query enumeration value from an application specific data store.
        /// </summary>
        /// <returns>A queryable collection of EnumObjects.</returns>
        /// <remarks></remarks>
        protected virtual IQueryable<EnumObject> GetAllInternal()
        {
            return null;
        }

        /// <summary>
        /// Called when a new EnumObject is added upon loading the repository.
        /// </summary>
        /// <param name="obj">The enumeration object.</param>
        /// <remarks>Can be used to provide additional functionality upon the object's add event.</remarks>
        protected virtual void OnAddNewEnumObject(EnumObject obj)
        {
            return;
        }

        #region IEnumRepository<Status> Members

        public Dictionary<int, EnumObject> ValuesByID { get { return m_dictIDs; } }
        public Dictionary<string, EnumObject> ValuesByName { get { return m_dictNames; } }
        public IEnumerable<string> Names { get { return m_dictNames.Keys; } }

        public IQueryable<EnumObject> GetAll()
        {
            return GetAllInternal();
        }

        public EnumObject GetObject(int _nID)
        {
            EnumObject ret = null;
            if (m_dictIDs.ContainsKey(_nID))
            {
                ret = m_dictIDs[_nID];
            }
            return ret;
        }

        public int GetEnumID(string _strName)
        {
            int ret = 0;
            if (m_dictNames.ContainsKey(_strName))
            {
                EnumObject data = m_dictNames[_strName];
                ret = data.TypeID;
            }
            return ret;
        }

        public string GetEnumName(int _nID)
        {
            string ret = string.Empty;
            if (m_dictIDs.ContainsKey(_nID))
            {
                EnumObject data = m_dictIDs[_nID];
                ret = data.TypeName;
            }
            return ret;
        }

        #endregion
    }
}
