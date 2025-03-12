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
    public class SemanticShiftRepository : RepositoryBase<SemanticShift, SemanticShiftId>, ISemanticShiftRepository
    {
        public SemanticShiftRepository(LinguisticsDbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<SemanticShift?> GetByIdAsync(SemanticShiftId id)
        {
            return await _dbContext.SemanticShifts
                .Include(ss => ss.AffectedCognate)
                .Include(ss => ss.RelatedEvents)
                .FirstOrDefaultAsync(ss => ss.Id == id);
        }

        public async Task<IEnumerable<SemanticShift>> GetByCognateSetAsync(CognateSetId cognateSetId)
        {
            return await _dbContext.SemanticShifts
                .Include(ss => ss.RelatedEvents)
                .Where(ss => (Guid)ss.AffectedCognate.Id == (Guid)cognateSetId)
                .ToListAsync();
        }
    }
}
