using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.DomainModel
{
    public class LanguageContactEvent : Entity<LanguageContactEventId>
    {
        public ContactType Type { get; private set; }
        public HashSet<string> LoanwordsAdopted { get; private set; } = new HashSet<string>();
        public string GrammaticalInfluence { get; private set; }
        public PhonemeInventory SourceLanguage { get; private set; }
        public PhonemeInventory TargetLanguage { get; private set; }

        private readonly List<SemanticShift> _causedShifts = new List<SemanticShift>();
        public IReadOnlyCollection<SemanticShift> CausedShifts => _causedShifts.AsReadOnly();

        private LanguageContactEvent() { }

        public LanguageContactEvent(
            LanguageContactEventId id,
            ContactType type,
            string grammaticalInfluence,
            PhonemeInventory sourceLanguage,
            PhonemeInventory targetLanguage)
            : base(id)
        {
            Type = type;
            GrammaticalInfluence = grammaticalInfluence ?? throw new ArgumentNullException(nameof(grammaticalInfluence));
            SourceLanguage = sourceLanguage ?? throw new ArgumentNullException(nameof(sourceLanguage));
            TargetLanguage = targetLanguage ?? throw new ArgumentNullException(nameof(targetLanguage));
        }

        public void UpdateType(ContactType type)
        {
            Type = type;
        }

        public void UpdateGrammaticalInfluence(string grammaticalInfluence)
        {
            GrammaticalInfluence = grammaticalInfluence ?? throw new ArgumentNullException(nameof(grammaticalInfluence));
        }

        public void AddLoanword(string loanword)
        {
            if (string.IsNullOrEmpty(loanword))
                throw new ArgumentException("Loanword cannot be empty", nameof(loanword));

            LoanwordsAdopted.Add(loanword);
        }

        public void AddCausedShift(SemanticShift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));

            _causedShifts.Add(shift);
        }
    }
}
