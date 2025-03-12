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
    public class CognateSetRepository : RepositoryBase<CognateSet, CognateSetId>, ICognateSetRepository
    {
        public CognateSetRepository(LinguisticsDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<CognateSet?> GetByIdAsync(CognateSetId id)
        {
            return await _dbContext.CognateSets
                .Include(cs => cs.ReconstructedFrom)
                .Include(cs => cs.SemanticShifts)
                .FirstOrDefaultAsync(cs => cs.Id == id);
        }

        public async Task<IEnumerable<CognateSet>> GetBySemanticFieldAsync(SemanticField field)
        {
            return await _dbContext.CognateSets
                .Include(cs => cs.ReconstructedFrom)
                .Where(cs => cs.Field == field)
                .ToListAsync();
        }

        public async Task<IEnumerable<CognateSet>> GetByPhonemeInventoryAsync(PhonemeInventoryId inventoryId)
        {
            return await _dbContext.CognateSets
                .Include(cs => cs.SemanticShifts)
                .Where(cs => (Guid)cs.ReconstructedFrom.Id == (Guid)inventoryId)
                .ToListAsync();
        }
    }
}
