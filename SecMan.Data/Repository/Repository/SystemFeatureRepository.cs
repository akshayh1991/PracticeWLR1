using Microsoft.EntityFrameworkCore;
using SecMan.Data.DAL;
using SecMan.Data.Repository.IRepository;
using SecMan.Data.SQLCipher;

namespace SecMan.Data.Repository.Repository
{
    public class SystemFeatureRepository : GenericRepository<SQLCipher.SysFeat>, ISystemFeatureRepository
    {
        private readonly Db _context;

        public SystemFeatureRepository(Db context) : base(context)
        {
            _context = context;
        }


        public async Task<SQLCipher.SysFeat> LoadLanguageWiseData(SQLCipher.SysFeat sysFeat)
        {
            await _context.Entry(sysFeat)
                .Collection(x => x.SysFeatProps)
                .Query()
                .Include(x => x.Langs)
                .LoadAsync();
            return sysFeat;
        }

    }
}
