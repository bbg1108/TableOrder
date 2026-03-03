using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk
{
    public static class DBConnectionFactory
    {
        private static string _connectionString;

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static SqlConnection Create()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("ConnectionString이 초기화되지 않았습니다.");

            return new SqlConnection(_connectionString);
        }
    }
}
