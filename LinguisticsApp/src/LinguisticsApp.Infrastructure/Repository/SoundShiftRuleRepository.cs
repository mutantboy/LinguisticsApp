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
    public class SoundShiftRuleRepository : RepositoryBase<SoundShiftRule, SoundShiftRuleId>, ISoundShiftRuleRepository
    {
        public SoundShiftRuleRepository(LinguisticsDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<SoundShiftRule?> GetByIdAsync(SoundShiftRuleId id)
        {
            return await _dbContext.SoundShiftRules
                .Include(ssr => ssr.AppliesTo)
                .Include(ssr => ssr.Creator)
                .FirstOrDefaultAsync(ssr => ssr.Id == id);
        }

        public async Task<IEnumerable<SoundShiftRule>> GetByPhonemeInventoryAsync(PhonemeInventoryId inventoryId)
        {
            return await _dbContext.SoundShiftRules
                .Include(ssr => ssr.Creator)
                .Where(ssr => (Guid)ssr.AppliesTo.Id == (Guid)inventoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<SoundShiftRule>> GetByRuleTypeAsync(RuleType type)
        {
            return await _dbContext.SoundShiftRules
                .Include(ssr => ssr.AppliesTo)
                .Include(ssr => ssr.Creator)
                .Where(ssr => ssr.Type == type)
                .ToListAsync();
        }
    }
}
