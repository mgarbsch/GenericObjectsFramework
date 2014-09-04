using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Collection base class for storing ModelObjects
    /// <remarks>Necessary for ModelObjectConverter to convert child collections.</remarks>
    /// </summary>
    public abstract class ModelObjectCollection : IEnumerable<ModelObject>
    {
        /// <summary>
        /// Reference to host the member IEnumerable declared in the inheriting class.
        /// </summary>
        private IEnumerable<ModelObject> m_iEnumInternal = null;

        /// <summary>
        /// Gets or sets the hosted enumerable reference used within the collection class.
        /// </summary>
        /// <value>The enumerable.</value>
        /// <remarks>Used by the ModelObjectConverter to list the collection's contents.</remarks>
        public IEnumerable<ModelObject> Enumerable { get { return m_iEnumInternal; } protected set { m_iEnumInternal = (IEnumerable<ModelObject>)value; } }

        public int Count { get { return GetCount(); } }

        public ModelObjectCollection()
        {
        }

        public ModelObjectCollection(IEnumerable<ModelObject> _enum)
        {
            m_iEnumInternal = _enum;
        }

        public void AddProvider(string strName, IModelObjectProvider provider)
        {
            foreach (ModelObject obj in this)
            {
                obj.AddProvider(strName, provider);
            }
        } 

        /// <summary>
        /// Overload to return the count of the hosted collection.
        /// </summary>
        /// <returns>The count of the hosted collection.</returns>
        protected virtual int GetCount()
        {
            return 0;
        }

        /// <summary>
        /// Overload to add an object to the hosted collection.
        /// </summary>
        /// <param name="obj">A generic model object.</param>
        public virtual void Add(ModelObject obj)
        {
        }

        /// <summary>
        /// Overload to remove an object from the hosted collection.
        /// </summary>
        /// <param name="obj">A generic model object.</param>
        public virtual void Remove(ModelObject obj)
        {
        }

        /// <summary>
        /// Overload to clear contents fro mthe hosted collection.
        /// </summary>
        public virtual void Clear()
        {
        }

        public virtual bool ContainsID(object idObject)
        {
            bool ret = false;
            foreach (ModelObject obj in this.Enumerable)
            {
                if (obj.ID.Equals(idObject))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        #region IEnumerable<ModelObject> Members

        public IEnumerator<ModelObject> GetEnumerator()
        {
            IEnumerator<ModelObject> ret = null;
            if (m_iEnumInternal != null) ret = m_iEnumInternal.GetEnumerator();
            return ret;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator ret = null;
            if (m_iEnumInternal != null) ret = m_iEnumInternal.GetEnumerator();
            return ret;
        }

        #endregion
    }
}
