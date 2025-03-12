using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Interfaces.Repository
{
    public interface IPhonemeInventoryRepository : IRepository<PhonemeInventory, PhonemeInventoryId>
    {
        Task<IEnumerable<PhonemeInventory>> GetByCreatorAsync(UserId creatorId);
        Task<IEnumerable<PhonemeInventory>> GetByLanguageNameAsync(LanguageName name);
    }
}
