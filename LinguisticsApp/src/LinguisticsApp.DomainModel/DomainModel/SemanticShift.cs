using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.DomainModel
{
    public class SemanticShift : Entity<SemanticShiftId>
    {
        public string OriginalMeaning { get; private set; }
        public string ModernMeaning { get; private set; }
        public ShiftTrigger Trigger { get; private set; }
        public CognateSet AffectedCognate { get; private set; }

        private readonly List<LanguageContactEvent> _relatedEvents = new List<LanguageContactEvent>();
        public IReadOnlyCollection<LanguageContactEvent> RelatedEvents => _relatedEvents.AsReadOnly();

        private SemanticShift() { }

        public SemanticShift(
            SemanticShiftId id,
            string originalMeaning,
            string modernMeaning,
            ShiftTrigger trigger,
            CognateSet affectedCognate)
            : base(id)
        {
            OriginalMeaning = originalMeaning ?? throw new ArgumentNullException(nameof(originalMeaning));
            ModernMeaning = modernMeaning ?? throw new ArgumentNullException(nameof(modernMeaning));
            Trigger = trigger;
            AffectedCognate = affectedCognate ?? throw new ArgumentNullException(nameof(affectedCognate));
        }

        public void UpdateMeanings(string originalMeaning, string modernMeaning)
        {
            OriginalMeaning = originalMeaning ?? throw new ArgumentNullException(nameof(originalMeaning));
            ModernMeaning = modernMeaning ?? throw new ArgumentNullException(nameof(modernMeaning));
        }

        public void UpdateTrigger(ShiftTrigger trigger)
        {
            Trigger = trigger;
        }

        public void AddRelatedEvent(LanguageContactEvent contactEvent)
        {
            if (contactEvent == null)
                throw new ArgumentNullException(nameof(contactEvent));

            _relatedEvents.Add(contactEvent);
        }
    }
}
