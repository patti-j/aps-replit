//using Microsoft.EntityFrameworkCore;

//namespace PTIntegrationAPI.DAL
//{
//    public static class DbContextFactory
//    {
//        //public static Dictionary<string, string> ConnectionStrings { get; set; }

//        //public static void SetConnectionString(Dictionary<string, string> connectionStrings)
//        //{
//        //    ConnectionStrings = connectionStrings;
//        //}

//        public static InstanceDBContext Create(string connectionString)
//        {
//            //var connStr = ConnectionStrings[connectionString];
//            var optionBuilder = new DbContextOptionsBuilder<InstanceDBContext>();
//            optionBuilder.UseSqlServer(connectionString);
//            return new InstanceDBContext(optionBuilder.Options);

//        }
//    }
//}
