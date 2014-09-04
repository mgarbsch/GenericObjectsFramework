using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xUtilities.Attributes;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Model layer base class for application object definitions.
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public abstract class ModelObject
    {
        /// <summary>
        /// The object's underlying ID.
        /// </summary>
        private object m_objID = null;
        /// <summary>
        /// The object's underlying owner ID.
        /// </summary>
        private object m_objOwnerID = null;
        /// <summary>
        /// Declaration of the object's collection of supported ModelObjectProviders.
        /// </summary>
        private Dictionary<string, IModelObjectProvider> m_dictProviders = new Dictionary<string, IModelObjectProvider>();

        /// <summary>
        /// Gets or sets the object ID.
        /// </summary>
        /// <value>The object ID.</value>
        /// <remarks>Provides access to the object's underlying ID. Used in object conversion.</remarks>
        [InternalAttribute(true)]
        public object ID { get { return m_objID; } set { m_objID = value; } }
        /// <summary>
        /// Gets or sets the object's owner ID.
        /// </summary>
        /// <value>The owner object ID.</value>
        /// <remarks>Provides access to the object's underlying owner ID. Used in object conversion. Can be <c>null</c> if object has no logical owner.</remarks>
        [InternalAttribute(true)]
        public object OwnerID { get { return m_objOwnerID; } set { m_objOwnerID = value; } }
        /// <summary>
        /// Gets the object's supported ModelObjectProvider dictionary.
        /// </summary>
        /// <remarks>The supported providers allow ModelObjects access to other model objects outside their scope. Used for filling child collections.</remarks>
        [InternalAttribute(true)]
        protected Dictionary<string, IModelObjectProvider> Providers { get { return m_dictProviders; } }

        [InternalAttribute(true)]
        public virtual string ClassName { get { return string.Empty; } }
        [InternalAttribute(true)]
        public virtual string DisplayName { get { return string.Empty; } }
        [InternalAttribute(true)]
        public bool IsSelected { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <remarks></remarks>
        public ModelObject()
        {
        }

        /// <summary>
        /// Adds a ModelObjectProvider for use in lazy-loaded collections.
        /// </summary>
        /// <param name="_strName">Name of the provider.</param>
        /// <param name="_provider">The provider interface (usually a service).</param>
        /// <remarks></remarks>
        public void AddProvider(string _strName, IModelObjectProvider _provider)
        {
            m_dictProviders.Add(_strName, _provider);
        }

        /// <summary>
        /// Determines whether the named ModelObjectProvider is supported in this object.
        /// </summary>
        /// <param name="_strName">Name of the provider.</param>
        /// <returns><c>true</c> if the specified provider name is supported; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public bool ContainsProvider(string _strName)
        {
            return m_dictProviders.ContainsKey(_strName);
        }
    }
}
