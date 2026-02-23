using System.Collections.Generic;

namespace AhaIntegration.ResponseObjects
{
    public class Users
    {
        public User[] users { get; set; }
        public Pagination pagination { get; set; }
    }
    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string product_id { get; set; }
        public string product_name { get; set; }
        public List<Product_Roles> product_roles { get; set; }

    }
}
