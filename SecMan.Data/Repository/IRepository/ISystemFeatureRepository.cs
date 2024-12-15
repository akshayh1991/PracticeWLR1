using SecMan.Interfaces.DAL;

namespace SecMan.Data.Repository.IRepository
{
    public interface ISystemFeatureRepository : IGenericRepository<SQLCipher.SysFeat>
    {
        Task<SQLCipher.SysFeat> LoadLanguageWiseData(SQLCipher.SysFeat sysFeat);
    }
}
