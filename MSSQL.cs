using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projetoControlarVendas
{
    public class MSSQL
    {
        private static string server = @"JOEL-PC\DESENVOLVIMENTO";
        private static string database = "aprendizagem";
        private static string user = "sa";
        private static string password = "Dv@CG#2025";


        public static string StringConnection
        {
            get { return $"Data Source={server}; Integrated Security=false; Initial Catalog={database}; User ID={user}; Password={password}"; }
        }
    }
}
