using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class RoleCategory
    {
        public int RolesId { get; set; }
        public Role Role { get; set; }
        public int CategoriesId { get; set; }
        public Category Category { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is RoleCategory gc)
            {
                return gc.RolesId == RolesId && gc.CategoriesId == CategoriesId;
            }
            return base.Equals(obj);
        }
    }
}
