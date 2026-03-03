using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk
{
    public static class DBHelper
    {
        /// <summary>
        /// 조회용 (SELECT)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<DataTable> QueryAsync(string sql, params SqlParameter[] parameters)
        {
            using (var conn = DBConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }

        /// <summary>
        /// 데이터 변경용 (INSERT / UPDATE / DELETE)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAsync(string sql, params SqlParameter[] parameters)
        {
            using (var conn = DBConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// 단일 값 조회
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<T> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            using (var conn = DBConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                await conn.OpenAsync();
                object result = await cmd.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                    return default(T);

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
    }
}
