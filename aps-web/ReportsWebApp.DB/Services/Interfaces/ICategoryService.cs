using ReportsWebApp.DB.Models;

public interface ICategoryService
{
    List<Category> GetCategories(int CompanyId);
    List<Category> GetCategoriesForNav(int CompanyId, List<Role> groups);
    List<Category> GetCategoriesForGroups(int CompanyId, List<Role> groups);
    Category GetCategory(int categoryId);
    Task<Category> SaveAsync(Category category);
    Task<bool> DeleteAsync(int categoryId);
}