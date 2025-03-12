using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Interfaces.Repository
{
    public interface ICognateSetRepository : IRepository<CognateSet, CognateSetId>
    {
        Task<IEnumerable<CognateSet>> GetBySemanticFieldAsync(SemanticField field);
        Task<IEnumerable<CognateSet>> GetByPhonemeInventoryAsync(PhonemeInventoryId inventoryId);
    }
}
