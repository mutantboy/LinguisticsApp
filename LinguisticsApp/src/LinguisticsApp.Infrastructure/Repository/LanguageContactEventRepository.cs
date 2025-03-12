using LinguisticsApp.Application.Common.Interfaces.Repository;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Infrastructure.Repository
{
    public class LanguageContactEventRepository : RepositoryBase<LanguageContactEvent, LanguageContactEventId>, ILanguageContactEventRepository
    {
        public LanguageContactEventRepository(LinguisticsDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<LanguageContactEvent?> GetByIdAsync(LanguageContactEventId id)
        {
            return await _dbContext.ContactEvents
                .Include(lce => lce.SourceLanguage)
                .Include(lce => lce.TargetLanguage)
                .Include(lce => lce.CausedShifts)
                .FirstOrDefaultAsync(lce => lce.Id == id);
        }

        public async Task<IEnumerable<LanguageContactEvent>> GetByLanguageAsync(PhonemeInventoryId languageId)
        {
            return await _dbContext.ContactEvents
                .Include(lce => lce.SourceLanguage)
                .Include(lce => lce.TargetLanguage)
                .Where(lce =>
                    (Guid)lce.SourceLanguage.Id == (Guid)languageId ||
                    (Guid)lce.TargetLanguage.Id == (Guid)languageId)
                .ToListAsync();
        }

        public async Task<IEnumerable<LanguageContactEvent>> GetByContactTypeAsync(ContactType type)
        {
            return await _dbContext.ContactEvents
                .Include(lce => lce.SourceLanguage)
                .Include(lce => lce.TargetLanguage)
                .Where(lce => lce.Type == type)
                .ToListAsync();
        }
    }
}
