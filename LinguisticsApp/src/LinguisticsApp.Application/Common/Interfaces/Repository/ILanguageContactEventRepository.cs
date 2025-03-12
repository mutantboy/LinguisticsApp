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
    public interface ILanguageContactEventRepository : IRepository<LanguageContactEvent, LanguageContactEventId>
    {
        Task<IEnumerable<LanguageContactEvent>> GetByLanguageAsync(PhonemeInventoryId languageId);
        Task<IEnumerable<LanguageContactEvent>> GetByContactTypeAsync(ContactType type);
    }
}
