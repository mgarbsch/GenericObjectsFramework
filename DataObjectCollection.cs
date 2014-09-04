using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Collection base class for storing DataObjects
    /// <remarks>Necessary for ModelObjectConverter to convert child collections.</remarks>
    /// </summary>
    public abstract class DataObjectCollection : IEnumerable<DataObject>
    {
        /// <summary>
        /// Reference to host the member IEnumerable declared in the inheriting class.
        /// </summary>
        private Dictionary<object, DataObject> m_dictObjects = new Dictionary<object, DataObject>();
        private Dictionary<int, object> m_dictKeys = new Dictionary<int, object>();

        /// <summary>
        /// Gets or sets the hosted enumerable reference used within the collection class.
        /// </summary>
        /// <value>The enumerable.</value>
        /// <remarks>Used by the ModelObjectConverter to list the collection's contents.</remarks>
        public int Count { get { return m_dictObjects.Count; } }

        public DataObjectCollection()
        {
        }

        public DataObjectCollection(IEnumerable<DataObject> _enum)
        {
            foreach (DataObject obj in _enum)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Overload to add an object to the hosted collection.
        /// </summary>
        /// <param name="obj">A generic model object.</param>
        public void Add(DataObject obj)
        {
            m_dictKeys.Add(m_dictObjects.Count, obj.ObjectID);
            m_dictObjects.Add(obj.ObjectID, obj);
        }

        /// <summary>
        /// Overload to remove an object from the hosted collection.
        /// </summary>
        /// <param name="obj">A generic model object.</param>
        public void Remove(DataObject obj)
        {
            if (m_dictObjects.ContainsKey(obj.ObjectID))
            {
                if (m_dictKeys.ContainsValue(obj.ObjectID))
                {
                    foreach (KeyValuePair<int, object> pair in m_dictKeys)
                    {
                        if (pair.Value.Equals(obj.ObjectID))
                        {
                            m_dictKeys.Remove(pair.Key);
                            break;
                        }
                    }
                }
                m_dictObjects.Remove(obj.ObjectID);
            }
            
            AdjustIndices(0);
        }

        /// <summary>
        /// Overload to clear contents fro mthe hosted collection.
        /// </summary>
        public void Clear()
        {
            m_dictKeys.Clear();
            m_dictObjects.Clear();
        }

        private void AdjustIndices(int _iStart)
        {
            for (int i = _iStart; i < m_dictObjects.Count; i++)
            {
            }
        }

        public DataObject this[int index]
        {
            get
            {
                DataObject ret = null;
                if (m_dictKeys.ContainsKey(index))
                {
                    object key = m_dictKeys[index];
                    if (m_dictObjects.ContainsKey(key))
                    {
                        ret = m_dictObjects[key];
                    }
                }
                return ret;
            }
            set
            {
                if (m_dictKeys.ContainsKey(index))
                {
                    object keyOld = m_dictKeys[index];
                    if (m_dictObjects.ContainsKey(keyOld))
                    {
                        m_dictObjects.Remove(keyOld);
                    }

                    DataObject obj = (DataObject)value;
                    m_dictObjects.Add(obj.ObjectID, obj);

                    m_dictKeys[index] = obj.ObjectID;
                }
            }
        }

        #region IEnumerable<DataObject> Members

        public IEnumerator<DataObject> GetEnumerator()
        {
            return m_dictObjects.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_dictObjects.Values.GetEnumerator();
        }

        #endregion
    }
}
