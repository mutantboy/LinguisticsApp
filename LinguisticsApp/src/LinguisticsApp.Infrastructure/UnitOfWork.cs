using LinguisticsApp.Application.Common.Interfaces.Repository;
using LinguisticsApp.Infrastructure.Persistence;
using LinguisticsApp.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinguisticsApp.Application.Common.Interfaces;

namespace LinguisticsApp.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LinguisticsDbContext _dbContext;
        private IUserRepository _userRepository;
        private IPhonemeInventoryRepository _phonemeInventoryRepository;
        private ISoundShiftRuleRepository _soundShiftRuleRepository;
        private ICognateSetRepository _cognateSetRepository;
        private ISemanticShiftRepository _semanticShiftRepository;
        private ILanguageContactEventRepository _languageContactEventRepository;
        private bool _disposed = false;

        public UnitOfWork(LinguisticsDbContext dbContext)
        {
            _dbContext = dbContext;
            _userRepository = new UserRepository(dbContext);
            _phonemeInventoryRepository = new PhonemeInventoryRepository(dbContext);
            _soundShiftRuleRepository = new SoundShiftRuleRepository(dbContext);
            _cognateSetRepository = new CognateSetRepository(dbContext);
            _semanticShiftRepository = new SemanticShiftRepository(dbContext);
            _languageContactEventRepository = new LanguageContactEventRepository(dbContext);
        }

        public IUserRepository Users => _userRepository;
        public IPhonemeInventoryRepository PhonemeInventories => _phonemeInventoryRepository;
        public ISoundShiftRuleRepository SoundShiftRules => _soundShiftRuleRepository;
        public ICognateSetRepository CognateSets => _cognateSetRepository;
        public ISemanticShiftRepository SemanticShifts => _semanticShiftRepository;
        public ILanguageContactEventRepository LanguageContactEvents => _languageContactEventRepository;

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
