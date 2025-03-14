using System.Data;

namespace SolidLayer_Architecture.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(string id);
        void Insert(T entity);
        void Update(T entity);
        void Delete(string id);
        
        // Raw SQL operations
        IEnumerable<T> ExecuteQuery(string sql, params object[] parameters);
        int ExecuteNonQuery(string sql, params object[] parameters);
        T? ExecuteScalar(string sql, params object[] parameters);
    }
}
