using LinguisticsApp.Application.Common.Interfaces.Repository;
using LinguisticsApp.DomainModel.DomainModel;
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
    public class PhonemeInventoryRepository : RepositoryBase<PhonemeInventory, PhonemeInventoryId>, IPhonemeInventoryRepository
    {
        public PhonemeInventoryRepository(LinguisticsDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<PhonemeInventory?> GetByIdAsync(PhonemeInventoryId id)
        {
            return await _dbContext.PhonemeInventories
                .Include(pi => pi.Consonants)
                .Include(pi => pi.Vowels)
                .Include(pi => pi.Creator)
                .FirstOrDefaultAsync(pi => pi.Id == id);
        }

        public async Task<IEnumerable<PhonemeInventory>> GetByCreatorAsync(UserId creatorId)
        {
            return await _dbContext.PhonemeInventories
                .Include(pi => pi.Consonants)
                .Include(pi => pi.Vowels)
                .Where(pi => (Guid)pi.Creator.Id == (Guid)creatorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PhonemeInventory>> GetByLanguageNameAsync(LanguageName name)
        {
            return await _dbContext.PhonemeInventories
                .Include(pi => pi.Consonants)
                .Include(pi => pi.Vowels)
                .Include(pi => pi.Creator)
                .Where(pi => pi.Name.Value.Contains(name.Value))
                .ToListAsync();
        }
    }
}
