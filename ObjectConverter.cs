using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using xUtilities.Attributes;

namespace GenericObjectsFramework
{
    /// <summary>
    /// Class which uses Reflection to convert from EntityObject to ModelObject and vice versa.
    /// </summary>
    /// <remarks></remarks>
    public class ObjectConverter
    {
        private ObjectManager m_manager = null;

        /// <summary>
        /// A reference to the application's ObjectManager
        /// </summary>
        public ObjectManager Manager { get { return m_manager; } protected set { m_manager = (ObjectManager)value; } }

        public ObjectConverter(ObjectManager _manager)
        {
            Manager = _manager;
        }

        /// <summary>
        /// Converts the specified EntityObject.
        /// </summary>
        /// <param name="objEntity">The EntityObject to convert.</param>
        /// <returns>Returns a newly created ModelObject, or <c>null</c> is unsuccessful.</returns>
        /// <remarks></remarks>
        public ModelObject ConvertToModel(EntityObject objEntity)
        {
            ModelObject objModel = null;

            try
            {
                if (objEntity != null)
                {
                    Type typeEntity = objEntity.GetType();
                    string strDataStoreName = Manager.GetDataStoreName(typeEntity);
                    objModel = Manager.CreateModelObject(typeEntity.Name);

                    if (objModel != null)
                    {
                        Type typeModel = objModel.GetType();
                        PropertyInfo[] pisModel = typeModel.GetProperties();

                        // cycle through the model object's properties to assign new values
                        foreach (PropertyInfo piModel in pisModel)
                        {
                            AssignPropertyValue(objModel, typeModel, piModel, objEntity, typeEntity, true, strDataStoreName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objModel = null;
            }

            return objModel;
        }

        /// <summary>
        /// Converts the specified ModelObject.
        /// </summary>
        /// <param name="objEntity">The ModelObject to convert.</param>
        /// <returns>Returns an EntityObject either fetched from the application's data store or created if no coinciding is found, or <c>null</c> is unsuccessful.</returns>
        /// <remarks></remarks>
        public EntityObject ConvertToEntity(ModelObject objModel)
        {
            return ConvertToEntity(objModel, this.Manager.DataStores.PrimaryStore.Name);
        }
        public EntityObject ConvertToEntity(ModelObject objModel, string strDataStoreName)
        {
            EntityObject objEntity = null;

            try
            {
                if (objModel != null)
                {
                    Type typeModel = objModel.GetType();

                    string strClassName = GetClassName(typeModel);

                    objEntity = Manager.GetEntityObject(strClassName, objModel.ID, strDataStoreName);
                    if (objEntity == null) objEntity = Manager.CreateEntityObject(strClassName);

                    if (objEntity != null)
                    {
                        Type typeEntity = objEntity.GetType();
                        PropertyInfo[] pisEntity = typeEntity.GetProperties();

                        // cycle through the entity object's properties to assign new values
                        foreach (PropertyInfo piEntity in pisEntity)
                        {
                            AssignPropertyValue(objModel, typeModel, piEntity, objEntity, typeEntity, false, strDataStoreName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objEntity = null;
            }


            return objEntity;
        }

        /// <summary>
        /// Converts the specified DataObject.
        /// </summary>
        /// <param name="objEntity">The DataObject to convert.</param>
        /// <returns>Returns a newly created ModelObject, or <c>null</c> is unsuccessful.</returns>
        /// <remarks></remarks>
        public ModelObject ConvertToModel(DataObject objData)
        {
            ModelObject objModel = null;

            try
            {
                if (objData != null)
                {
                    Type typeData = objData.GetType();
                    objModel = Manager.CreateModelObject(typeData.Name);

                    if (objModel != null)
                    {
                        Type typeModel = objModel.GetType();
                        PropertyInfo[] pisModel = typeModel.GetProperties();

                        // cycle through the model object's properties to assign new values
                        foreach (PropertyInfo piModel in pisModel)
                        {
                            AssignPropertyValue(objModel, typeModel, piModel, objData, typeData, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objModel = null;
            }

            return objModel;
        }

        /// <summary>
        /// Converts the specified ModelObject.
        /// </summary>
        /// <param name="objEntity">The ModelObject to convert.</param>
        /// <returns>Returns an DataObject either fetched from the application's data store or created if no coinciding is found, or <c>null</c> is unsuccessful.</returns>
        /// <remarks></remarks>
        public DataObject ConvertToData(ModelObject objModel)
        {
            DataObject objData = null;

            try
            {
                if (objModel != null)
                {
                    Type typeModel = objModel.GetType();

                    string strClassName = GetClassName(typeModel);

                    objData = Manager.GetDataObject(strClassName, objModel.ID);
                    if (objData == null) objData = Manager.CreateDataObject(strClassName);

                    if (objData != null)
                    {
                        Type typeData = objData.GetType();
                        PropertyInfo[] pisEntity = typeData.GetProperties();

                        // cycle through the entity object's properties to assign new values
                        foreach (PropertyInfo piEntity in pisEntity)
                        {
                            AssignPropertyValue(objModel, typeModel, piEntity, objData, typeData, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                objData = null;
            }


            return objData;
        }

        /// <summary>
        /// Assigns the property value.
        /// </summary>
        /// <param name="objModel">The model object used in the conversion.</param>
        /// <param name="typeModel">The model object's Type information.</param>
        /// <param name="pi">The PropertyInfo describing the property in effect.</param>
        /// <param name="objEntity">The entity object used in the conversion.</param>
        /// <param name="typeEntity">The entity object's Type information.</param>
        /// <param name="bCopyToModel">When <c>true</c> copy direction from entity to model. When <c>false</c> copy direction from model to entity.</param>
        /// <remarks></remarks>
        private void AssignPropertyValue(ModelObject objModel, Type typeModel, PropertyInfo pi, EntityObject objEntity, Type typeEntity, bool bCopyToModel, string strDataStoreName)
        {
            try
            {
                string strPropertyName = string.Empty;

                PropertyInfo piModel = null;
                PropertyInfo piEntity = null;

                // assign PropertyInfo references based on CopyToModel flag
                if (bCopyToModel)
                {
                    piModel = pi;
                    piEntity = FindEntityProperty(piModel, typeEntity);
                }
                else
                {
                    piEntity = pi;
                    piModel = FindModelProperty(piEntity, typeModel);
                }

                if (piModel != null && piEntity != null)
                {
                    // get values for both entity and model properties
                    object objEntityProperty = piEntity.GetValue(objEntity, null);
                    object objModelProperty = piModel.GetValue(objModel, null);

                    // check if properties are both collections of model-entity instances
                    bool bIsEntityCollection = IsSubclassOfRawGeneric(typeof(EntityCollection<>), piEntity.PropertyType);
                    bool bIsModelCollection = piModel.PropertyType.IsSubclassOf(typeof(ModelObjectCollection));

                    if (bIsEntityCollection && bIsModelCollection)
                    {
                        // both property values must be valid for collection conversion
                        if (objEntityProperty != null && objModelProperty != null)
                        {
                            if (bCopyToModel)
                            {
                                ModelObject oModel = null;
                                ModelObjectCollection enumModelCollection = objModelProperty as ModelObjectCollection;
                                enumModelCollection.Clear();

                                IEnumerable<EntityObject> enumEntityCollection = objEntityProperty as IEnumerable<EntityObject>;
                                foreach (EntityObject oEntity in enumEntityCollection)
                                {
                                    oModel = ConvertToModel(oEntity);
                                    enumModelCollection.Add(oModel);
                                }
                            }
                            else
                            {
                                throw new Exception("Cannot convert ModelCollection to EntityCollection (yet)!");
                                // the casting to an EntityCollection<EntityObject> is not allowed, 
                                //  this type is needed in order to clear and then add converted entity objects

                                //EntityObject oEntity = null;
                                //IEnumerable<EntityObject> enumEntityCollection = objEntityProperty as IEnumerable<EntityObject>;
                                ////enumEntityCollection.Clear();
                                                                
                                //ModelObjectCollection enumModelCollection = objModelProperty as ModelObjectCollection;
                                //foreach (ModelObject oModel in enumModelCollection)
                                //{
                                //    oEntity = Convert(oModel);
                                //    //enumEntityCollection.Add(oEntity);
                                //}
                            }
                        }
                    }
                    else 
                    {
                        // if properties are not collections, check if equal tpyes
                        if (piModel.PropertyType.Equals(piEntity.PropertyType))
                        {
                            if (bCopyToModel)
                                piModel.SetValue(objModel, piEntity.GetValue(objEntity, null), null);
                            else
                                piEntity.SetValue(objEntity, piModel.GetValue(objModel, null), null);
                        }                        
                        else
                        {
                            // property types are not the same, check for model vs. entity
                            if ((bCopyToModel && objEntityProperty != null) || (!bCopyToModel && objModelProperty != null))
                            {
                                // check if properties have model-entity instance realtionship
                                bool bIsInstance = false;
                                if (bCopyToModel) bIsInstance = (typeof(EntityObject).IsInstanceOfType(objEntityProperty) && (objModelProperty == null || typeof(ModelObject).IsInstanceOfType(objModelProperty)));
                                else bIsInstance = ((objEntityProperty == null || typeof(EntityObject).IsInstanceOfType(objEntityProperty)) && typeof(ModelObject).IsInstanceOfType(objModelProperty));

                                if (bIsInstance)
                                {
                                    if (bCopyToModel) piModel.SetValue(objModel, ConvertToModel((EntityObject)objEntityProperty), null);
                                    else piEntity.SetValue(objEntity, ConvertToEntity((ModelObject)objModelProperty, strDataStoreName), null);
                                }                            
                            }
                            else
                            {
                                // set target property value to null if the source value is null
                                if (bCopyToModel && !piModel.PropertyType.IsValueType) piModel.SetValue(objModel, null, null);
                                else if (!bCopyToModel && !piEntity.PropertyType.IsValueType) piEntity.SetValue(objEntity, null, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { 
            }
        }

        /// <summary>
        /// Assigns the property value.
        /// </summary>
        /// <param name="objModel">The model object used in the conversion.</param>
        /// <param name="typeModel">The model object's Type information.</param>
        /// <param name="pi">The PropertyInfo describing the property in effect.</param>
        /// <param name="objEntity">The data object used in the conversion.</param>
        /// <param name="typeEntity">The data object's Type information.</param>
        /// <param name="bCopyToModel">When <c>true</c> copy direction from entity to model. When <c>false</c> copy direction from model to entity.</param>
        /// <remarks></remarks>
        private void AssignPropertyValue(ModelObject objModel, Type typeModel, PropertyInfo pi, DataObject objData, Type typeData, bool bCopyToModel)
        {
            try
            {
                string strPropertyName = string.Empty;

                PropertyInfo piModel = null;
                PropertyInfo piData = null;

                // assign PropertyInfo references based on CopyToModel flag
                if (bCopyToModel)
                {
                    piModel = pi;
                    piData = FindDataProperty(piModel, typeData);
                }
                else
                {
                    piData = pi;
                    piModel = FindModelProperty(piData, typeModel);
                }

                if (piModel != null && piData != null)
                {
                    // get values for both entity and model properties
                    object objDataProperty = piData.GetValue(objData, null);
                    object objModelProperty = piModel.GetValue(objModel, null);

                    // check if properties are both collections of model-entity instances
                    bool bIsDataCollection = IsSubclassOfRawGeneric(typeof(DataObjectCollection), piData.PropertyType);
                    bool bIsModelCollection = piModel.PropertyType.IsSubclassOf(typeof(ModelObjectCollection));

                    if (bIsDataCollection && bIsModelCollection)
                    {
                        // both property values must be valid for collection conversion
                        if (objDataProperty != null && objModelProperty != null)
                        {
                            if (bCopyToModel)
                            {
                                ModelObject oModel = null;
                                ModelObjectCollection enumModelCollection = objModelProperty as ModelObjectCollection;
                                enumModelCollection.Clear();

                                DataObjectCollection enumDataCollection = objDataProperty as DataObjectCollection;
                                foreach (DataObject oData in enumDataCollection)
                                {
                                    oModel = ConvertToModel(oData);
                                    enumModelCollection.Add(oModel);
                                }
                            }
                            else
                            {
                                DataObject oData = null;
                                DataObjectCollection enumDataCollection = objDataProperty as DataObjectCollection;
                                enumDataCollection.Clear();

                                ModelObjectCollection enumModelCollection = objModelProperty as ModelObjectCollection;
                                foreach (ModelObject oModel in enumModelCollection)
                                {
                                    oData = ConvertToData(oModel);
                                    enumDataCollection.Add(oData);
                                }
                            }
                        }
                    }
                    else
                    {
                        // if properties are not collections, check if equal tpyes
                        if (piModel.PropertyType.Equals(piData.PropertyType))
                        {
                            if (bCopyToModel)
                                piModel.SetValue(objModel, piData.GetValue(objData, null), null);
                            else
                                piData.SetValue(objData, piModel.GetValue(objModel, null), null);
                        }
                        else
                        {
                            // property types are not the same, check for model vs. entity
                            if ((bCopyToModel && objDataProperty != null) || (!bCopyToModel && objModelProperty != null))
                            {
                                // check if properties have model-entity instance realtionship
                                bool bIsInstance = false;
                                if (bCopyToModel) bIsInstance = (typeof(DataObject).IsInstanceOfType(objDataProperty) && (objModelProperty == null || typeof(ModelObject).IsInstanceOfType(objModelProperty)));
                                else bIsInstance = ((objDataProperty == null || typeof(DataObject).IsInstanceOfType(objDataProperty)) && typeof(ModelObject).IsInstanceOfType(objModelProperty));

                                if (bIsInstance)
                                {
                                    if (bCopyToModel) piModel.SetValue(objModel, ConvertToModel((DataObject)objDataProperty), null);
                                    else piData.SetValue(objData, ConvertToData((ModelObject)objModelProperty), null);
                                }
                            }
                            else
                            {
                                // set target property value to null if the source value is null
                                if (bCopyToModel && !piModel.PropertyType.IsValueType) piModel.SetValue(objModel, null, null);
                                else if (!bCopyToModel && !piData.PropertyType.IsValueType) piData.SetValue(objData, null, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Method to determine if Type is a raw generic (eg. <c>IEnumerable<></c>)
        /// </summary>
        /// <param name="typeGeneric">The generic type to look for.</param>
        /// <param name="typeCheck">The type to check.</param>
        /// <returns>Returns <c>true</c> if type is a raw generic, return <c>false</c> if not.</returns>
        private bool IsSubclassOfRawGeneric(Type typeGeneric, Type typeCheck)
        {
            bool ret = false;
            while (typeCheck != typeof(object) && !typeCheck.IsInterface)
            {
                var cur = typeCheck.IsGenericType ? typeCheck.GetGenericTypeDefinition() : typeCheck;
                if (typeGeneric == cur)
                {
                    ret = true;
                    break;
                }
                typeCheck = typeCheck.BaseType;
            }
            return ret;
        }

        /// <summary>
        /// Find the coinsiding property with the provided model type.
        /// </summary>
        /// <param name="piEntity">PropertyInfo for the entity property to look for.</param>
        /// <param name="typeModel">The model Type to search for the property.</param>
        /// <returns>Returns the ProprtyInfo for the model property if found, otherwise <c>null</c>.</returns>
        private PropertyInfo FindModelProperty(PropertyInfo piEntity, Type typeModel)
        {
            PropertyInfo piModel = null;

            bool bIsInternal = false;
            string strPropertyName = string.Empty;
            PropertyInfo[] pisModel = typeModel.GetProperties();

            foreach (PropertyInfo pi in pisModel)
            {
                // check if property is meant for internal use, if so ignore
                bIsInternal = InternalAttribute.IsInternalProperty(pi);

                if (!bIsInternal)
                {
                    // use DataObjectName, if specified, for property name comparison
                    strPropertyName = pi.Name;
                    string name = DataObjectNameAttribute.GetDataObjectName(pi);
                    if (name.Length > 0) strPropertyName = name;

                    if (strPropertyName.Equals(piEntity.Name))
                    {
                        piModel = pi;
                        break;
                    }
                }
            }

            return piModel;
        }

        /// <summary>
        /// Find the coinsiding property with the provided entity type.
        /// </summary>
        /// <param name="piEntity">PropertyInfo for the model property to look for.</param>
        /// <param name="typeModel">The entity Type to search for the property.</param>
        /// <returns>Returns the ProprtyInfo for the entity property if found, otherwise <c>null</c>.</returns>
        private PropertyInfo FindEntityProperty(PropertyInfo piModel, Type typeEntity)
        {
            PropertyInfo piEntity = null;

            // check if property is meant for internal use, if so ignore
            bool bIsInternal = InternalAttribute.IsInternalProperty(piModel);

            if (!bIsInternal)
            {
                string strPropertyName = piModel.Name;
                PropertyInfo[] pisEntity = typeEntity.GetProperties();

                // use DataObjectName, if specified, for property name comparison
                string name = DataObjectNameAttribute.GetDataObjectName(piModel);
                if (name.Length > 0) strPropertyName = name;

                foreach (PropertyInfo pi in pisEntity)
                {
                    if (strPropertyName.Equals(pi.Name))
                    {
                        piEntity = pi;
                        break;
                    }
                }
            }

            return piEntity;
        }

        private PropertyInfo FindDataProperty(PropertyInfo piModel, Type typeData)
        {
            PropertyInfo piData = null;

            // check if property is meant for internal use, if so ignore
            bool bIsInternal = InternalAttribute.IsInternalProperty(piModel);

            if (!bIsInternal)
            {
                string strPropertyName = piModel.Name;
                PropertyInfo[] pisData = typeData.GetProperties();

                // use DataObjectName, if specified, for property name comparison
                string name = DataObjectNameAttribute.GetDataObjectName(piModel);
                if (name.Length > 0) strPropertyName = name;

                foreach (PropertyInfo pi in pisData)
                {
                    if (strPropertyName.Equals(pi.Name))
                    {
                        piData = pi;
                        break;
                    }
                }
            }

            return piData;
        }

        /// <summary>
        /// Gets the name of the class from the input Type.
        /// </summary>
        /// <param name="type">The Type to search.</param>
        /// <returns>The name of the class used in the data layer.</returns>
        /// <remarks>Uses the DataObjectNameAttribute of the class to determine its name on the data layer.</remarks>
        public string GetClassName(Type type)
        {
            string strClassName = type.Name;
            string name = DataObjectNameAttribute.GetDataObjectName(type);
            if (name.Length > 0) strClassName = name;
            return strClassName;
        }

        /// <summary>
        /// Gets the name of the class's primary key from the input Type.
        /// </summary>
        /// <param name="type">The Type to search.</param>
        /// <returns>The name of the class's primary key used in the data layer.</returns>
        /// <remarks>Uses the PrimaryKeyAttribute and DataObjectNameAttribute of the class to determine its key name on the data layer.</remarks>
        public string GetKeyName(Type type)
        {
            string strKeyName = PrimaryKeyAttribute.GetKeyName(type);

            if (!strKeyName.Equals(string.Empty))
            {
                PropertyInfo piKey = type.GetProperty(strKeyName);
                if (piKey != null)
                {
                    string name = DataObjectNameAttribute.GetDataObjectName(piKey);
                    if (name.Length > 0) strKeyName = name;
                }
            }
            return strKeyName;
        }

        /// <summary>
        /// Gets the name of the class's owner key from the input Type.
        /// </summary>
        /// <param name="type">The Type to search.</param>
        /// <returns>The name of the class's owner object key used in the data layer.</returns>
        /// <remarks>Uses the ForeignKeyAttribute and DataObjectNameAttribute of the class to determine its owner key name on the data layer.</remarks>
        public string GetOwnerKeyName(Type type)
        {
            string strKeyName = ForeignKeyAttribute.GetKeyName(type, 0);

            if (!strKeyName.Equals(string.Empty))
            {
                PropertyInfo piKey = type.GetProperty(strKeyName);
                if (piKey != null)
                {
                    string name = DataObjectNameAttribute.GetDataObjectName(piKey);
                    if (name.Length > 0) strKeyName = name;
                }
            }
            return strKeyName;
        }
    }
}
