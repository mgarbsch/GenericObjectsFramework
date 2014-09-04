using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xUtilities.Collections;

namespace GenericObjectsFramework
{
    public class QueryPagingParams
    {
        private string m_strQueryName = string.Empty;
        private int m_nQueryPosition = 0;
        private int m_nQueryPageSize = 20;

        public string Name { get { return m_strQueryName; } }
        public int Position { get { return m_nQueryPosition; } set { m_nQueryPosition = (int)value; } }
        public int PageSize { get { return m_nQueryPageSize; } set { m_nQueryPageSize = (int)value; } }

        public QueryPagingParams(string _strName)
        {
            m_strQueryName = _strName;
        }

        public QueryPagingParams(string _strName, int _nPageSize)
        {
            m_strQueryName = _strName;
            m_nQueryPageSize = _nPageSize;
        }
    }

    public class Service
    {
        public delegate object ServiceRequestHandler(string strServiceName, string strMethodName, ParameterCollection listParams);
        public event ServiceRequestHandler ServiceRequested = null;

        protected object RaiseServiceRequest(string strServiceName, string strMethodName, ParameterCollection listParams)
        {
            object ret = null;
            if (ServiceRequested != null) ret = ServiceRequested(strServiceName, strMethodName, listParams);
            return ret;
        }

        private Dictionary<string, QueryPagingParams> m_dictQueryPagingParams = new Dictionary<string, QueryPagingParams>();

        public Dictionary<string, QueryPagingParams> PagingParams { get { return m_dictQueryPagingParams; } }

        public Service()
        {
        }

        public QueryPagingParams this[string _strQueryName]
        {
            get
            {
                QueryPagingParams ret = null;
                if (m_dictQueryPagingParams.ContainsKey(_strQueryName)) ret = m_dictQueryPagingParams[_strQueryName];
                return ret;
            }
        }

        public void AddPagingParams(string _strQueryName)
        {
            QueryPagingParams param = new QueryPagingParams(_strQueryName);
            PagingParams.Add(_strQueryName, param);
        }

        public void AddPagingParams(string _strQueryName, int _nPageSize)
        {
            QueryPagingParams param = new QueryPagingParams(_strQueryName, _nPageSize);
            PagingParams.Add(_strQueryName, param);
        }


    }
}
