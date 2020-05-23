using DIS2020.INFRA.Factories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DIS2020.BLL
{
    public class BaseService
    {
        private IConfiguration config;
        public BaseService(IConfiguration _config)
        {
            config = _config;
        }

        public dynamic ExecuteProc(string name, Dictionary<string, object> param)
        {
            var conn = new ConnFactory(config);
            var result = conn.ExecuteScalar(name, CommandType.StoredProcedure, param);
            return result;
        }

        public IEnumerable<T> ExecuteProcGet<T>(string name, Dictionary<string, object> param)
        {
            var conn = new ConnFactory(config);
            DataTable dt = new DataTable();
            var result = conn.GetReader(name, CommandType.StoredProcedure, param);
            dt.Load(result);

            var res = TranslateDatatable<T>(dt);
            return res;
        }

        public IEnumerable<T> GetStuff<T>(string query)
        {
            var conn = new ConnFactory(config);
            DataTable dt = new DataTable();
            var result = conn.GetReader(query);
            dt.Load(result);

            var res = TranslateDatatable<T>(dt);
            return res;
        }

        public IEnumerable<T> TranslateDatatable<T>(dynamic db)
        {
            var dt = (DataTable)db;

            var colunas = dt.Columns.Cast<DataColumn>().Select(d => new { d.DataType, d.ColumnName }).ToList();
            var res = new List<T>();
            foreach (DataRow item in dt.Rows)
            {
                var entidade = Activator.CreateInstance(typeof(T));

                foreach (var coluna in colunas)
                {
                    var tipo = Type.GetTypeCode(coluna.DataType);
                    var val = item[coluna.ColumnName];
                    if (val == null || DBNull.Value.Equals(val))
                    {
                        val = ValidateNullValue(coluna.DataType.Name);
                    }
                    entidade?.GetType()?.GetProperty(coluna.ColumnName).SetValue(entidade, val);
                }
                res.Add((T)entidade);
            }

            return res;
        }

        private dynamic ValidateNullValue(string tipo)
        {
            switch (tipo)
            {
                case "Int32":
                    return int.MinValue;
                   // break;

                case "String":
                    return "";
                   // break;

                case "DateTime":
                    return DateTime.MinValue;
                    //break;

                case "Boolean":
                    return false;
                    //break;
                default:
                    return null;
            }
        }
    }
}
