using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xUtilities.Attributes;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Model layer base class used to specify an enumeration.
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public abstract class EnumObject : ModelObject
    {
        private string m_strTypeName = string.Empty;
        
        /// <summary>
        /// Gets or sets the enumeration's type ID.
        /// </summary>
        /// <value>The enumeration's ID.</value>
        /// <remarks>Type ID is directed to make use of the ModelObject ObjectID.</remarks>
        public int TypeID { get { return (int)ID; } set { ID = value; } }
        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        /// <remarks>Provides a base property to store the enumration's name.</remarks>
        public string TypeName { get { return m_strTypeName; } set { m_strTypeName = (string)value; } }

        [InternalAttribute(true)]
        public override string DisplayName { get { return this.TypeName; } }

        public EnumObject()
        {
        }

        public override string ToString()
        {
            return this.TypeName;
        }
    }
}
