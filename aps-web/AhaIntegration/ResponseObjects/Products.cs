using System;

namespace AhaIntegration.ResponseObjects
{

    public class Products
    {
        public Product[] products { get; set; }
        public Pagination pagination { get; set; }
    }
    
    public class Product
    {
        public string id { get; set; }
        public string reference_prefix { get; set; }
        public string name { get; set; }
        public bool product_line { get; set; }
        public DateTime created_at { get; set; }
    }

}
