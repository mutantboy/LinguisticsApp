using LinguisticsApp.Application.Common.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IPhonemeInventoryRepository PhonemeInventories { get; }
        ISoundShiftRuleRepository SoundShiftRules { get; }
        ICognateSetRepository CognateSets { get; }
        ISemanticShiftRepository SemanticShifts { get; }
        ILanguageContactEventRepository LanguageContactEvents { get; }

        Task<int> SaveChangesAsync();
    }
}
