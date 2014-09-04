using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.IO;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using xUtilities.Commands;
using DatabaseDefinitionLibrary;

namespace GenericObjectsFramework
{
    public enum DataStoreLocation
    {
        None = 0,
        Local = 1,
        Remote = 2
    }

    public class EntityDataStore
    {
        private string m_strName = string.Empty;
        private DataStoreLocation m_eLocation = DataStoreLocation.None;
        private bool m_bIsPrimary = false;
        private DateTime m_dtTimestamp = DateTime.MinValue;
        private string m_strConnectionString = string.Empty;
        private string m_strDefinitionFileName = string.Empty;

        private ObjectContext m_refObjectContext = null;
        private DbCommandScript m_CommandScript = new DbCommandScript();
        private DbDefinition m_DbDefinition = new DbDefinition();

        public string Name { get { return m_strName; } set { m_strName = value; } }
        public DataStoreLocation Location { get { return m_eLocation; } set { m_eLocation = value; } }
        public bool IsPrimary { get { return m_bIsPrimary; } set { m_bIsPrimary = value; } }
        public DateTime Timestamp { get { return m_dtTimestamp; } set { m_dtTimestamp = value; } }
        public string ConnectionString { get { return m_strConnectionString; } set { m_strConnectionString = value; } }
        public string DefinitionFileName { get { return m_strDefinitionFileName; } set { m_strDefinitionFileName = value; } }

        public ObjectContext ObjectContext { get { return m_refObjectContext; } set { m_refObjectContext = value; } }
        public DbCommandScript Commands { get { return m_CommandScript; } }
        public DbDefinition DatabaseDefinition { get { return m_DbDefinition; } }

        public EntityDataStore()
        {
            InitDataStore();
        }

        public EntityDataStore(string strName)
        {
            m_strName = strName;

            InitDataStore();
        }

        public EntityDataStore(string strName, ObjectContext context, DataStoreLocation eLocation, bool bIsPrimary)
        {
            m_strName = strName;
            m_refObjectContext = context;
            m_eLocation = eLocation;
            m_bIsPrimary = bIsPrimary;

            InitDataStore();
        }

        private void InitDataStore()
        {
            m_CommandScript.NeedCommandPermission += new CommandPermissionEventHandler(CommandScript_NeedCommandPermission);
            m_CommandScript.CommandDispatched += new CommandEventHandler(CommandScript_CommandDispatched);
            m_CommandScript.CommandProgressUpdated += new CommandProgressEventHandler(CommandScript_CommandProgressUpdated);
        }

        private void CommandScript_NeedCommandPermission(CommandPermissionEventArgs args)
        {
        }

        private void CommandScript_CommandDispatched(CommandEventArgs args)
        {
        }

        private void CommandScript_CommandProgressUpdated(CommandProgressEventArgs args)
        {
        }

        public void Read(XmlReader reader)
        {
            if (reader.Name.Equals("EntityDataStore"))
            {
                if (reader.HasAttributes)
                {
                    this.Name = reader.GetAttribute("Name");
                    this.Location = (DataStoreLocation)Enum.Parse(typeof(DataStoreLocation), reader.GetAttribute("Location"));
                    this.DefinitionFileName = reader.GetAttribute("DefinitionFileName");
                    this.ConnectionString = reader.GetAttribute("ConnectionString");
                }

                if (!reader.IsEmptyElement)
                {
                    bool bBreak = false;
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name.Equals("CommandScript"))
                                {
                                    this.Commands.Read(reader);
                                }
                                break;
                            case XmlNodeType.EndElement:
                                if (reader.Name.Equals("EntityDataStore")) bBreak = true;
                                break;
                        }

                        if (bBreak) break;
                    }
                }
            }
        }

        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement("EntityDataStore");
            writer.WriteAttributeString("Name", this.Name);
            writer.WriteAttributeString("Location", this.Location.ToString());
            writer.WriteAttributeString("DefinitionFileName", this.DefinitionFileName);
            writer.WriteAttributeString("ConnectionString", this.ConnectionString);

            this.Commands.Write(writer);

            writer.WriteEndElement();
        }
    }

    public class EntityDataStoreCollection : IEnumerable<EntityDataStore>
    {
        private string m_strFileName = string.Empty;
        private Dictionary<string, EntityDataStore> m_dictStores = new Dictionary<string, EntityDataStore>();
        private EntityDataStore m_refPrimaryStore = null;

        public string FileName { get { return m_strFileName; } set { m_strFileName = value; } }
        public int Count { get { return m_dictStores.Count; } }

        public EntityDataStore this[string strName]
        {
            get { return (m_dictStores.ContainsKey(strName)) ? m_dictStores[strName] : null; }
        }

        public EntityDataStore PrimaryStore
        {
            get
            {
                if (m_refPrimaryStore == null)
                {
                    if (m_dictStores.Count > 0)
                    {
                        EntityDataStore storeFirst = null;
                        foreach (EntityDataStore store in m_dictStores.Values)
                        {
                            if (storeFirst == null) storeFirst = store;
                            if (store.IsPrimary)
                            {
                                m_refPrimaryStore = store;
                                break;
                            }
                        }
                        if (m_refPrimaryStore == null) m_refPrimaryStore = storeFirst;
                    }
                    
                }
                return m_refPrimaryStore;
            }
        }

        public EntityDataStoreCollection()
        {
        }

        public bool Contains(string strName)
        {
            return m_dictStores.ContainsKey(strName);
        }

        public void Attach(string strName, ObjectContext context)
        {
            Attach(strName, context, false);
        }
        public void Attach(string strName, ObjectContext context, bool bIsPrimary)
        {
            EntityDataStore store = null;
            if (m_dictStores.ContainsKey(strName))
            {
                store = m_dictStores[strName];
            }
            else 
            {
                store = new EntityDataStore(strName);
                m_dictStores.Add(strName, store);
            }
            store.ObjectContext = context;
            store.IsPrimary = bIsPrimary;
        }

        #region IEnumerable<DataStore> Members

        public IEnumerator<EntityDataStore> GetEnumerator()
        {
            return m_dictStores.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_dictStores.Values.GetEnumerator();
        }

        #endregion

        public void Load(string strFileName)
        {
            XmlReader reader = null;

            try
            {
                if (System.IO.File.Exists(strFileName))
                {
                    reader = XmlReader.Create(strFileName);
                    Read(reader);
                    m_strFileName = strFileName;
                }
            }
            catch
            {
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        public void Read(XmlReader reader)
        {
            if (reader.Name.Equals("EntityDataStores") && !reader.IsEmptyElement)
            {
                EntityDataStore store = null;
                EntityDataStore storeFirst = null;

                bool bBreak = false;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("EntityDataStore"))
                            {
                                store = new EntityDataStore();
                                store.Read(reader);
                                m_dictStores.Add(store.Name, store);

                                if (storeFirst == null) storeFirst = store;
                                if (m_refPrimaryStore == null && store.IsPrimary)
                                    m_refPrimaryStore = store;
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name.Equals("EntityDataStores")) bBreak = true;
                            break;
                    }

                    if (bBreak) break;
                }

                if (m_refPrimaryStore == null)
                    m_refPrimaryStore = storeFirst;
            }
        }

        public void Save()
        {
            Save(this.FileName);
        }

        public void Save(string strFileName)
        {
            XmlWriter writer = null;

            try
            {
                if (Path.IsPathRooted(strFileName))
                {
                    writer = XmlWriter.Create(strFileName);
                    Write(writer);
                    writer.Flush();
                }
            }
            catch
            {
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }

        public void Write(XmlWriter writer)
        {
            writer.WriteStartElement("EntityDataStores");

            foreach (EntityDataStore store in this)
            {
                store.Write(writer);
            }

            writer.WriteEndElement();
        }
    }
}
