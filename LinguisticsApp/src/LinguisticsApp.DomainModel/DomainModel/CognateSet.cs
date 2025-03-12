using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.DomainModel
{
    public class CognateSet : Entity<CognateSetId>
    {
        public ProtoForm ProtoForm { get; private set; }
        public SemanticField Field { get; private set; }
        public Dictionary<string, string> AttestedExamples { get; private set; } = new Dictionary<string, string>();
        public PhonemeInventory ReconstructedFrom { get; private set; }

        private readonly List<SemanticShift> _semanticShifts = new List<SemanticShift>();
        public IReadOnlyCollection<SemanticShift> SemanticShifts => _semanticShifts.AsReadOnly();

        private CognateSet() { }

        public CognateSet(
            CognateSetId id,
            ProtoForm protoForm,
            SemanticField field,
            PhonemeInventory reconstructedFrom)
            : base(id)
        {
            ProtoForm = protoForm ?? throw new ArgumentNullException(nameof(protoForm));
            Field = field;
            ReconstructedFrom = reconstructedFrom ?? throw new ArgumentNullException(nameof(reconstructedFrom));
        }

        public void UpdateProtoForm(ProtoForm protoForm)
        {
            ProtoForm = protoForm ?? throw new ArgumentNullException(nameof(protoForm));
        }

        public void UpdateField(SemanticField field)
        {
            Field = field;
        }

        public void AddAttestedExample(string language, string form)
        {
            if (string.IsNullOrEmpty(language))
                throw new ArgumentException("Language cannot be empty", nameof(language));

            if (string.IsNullOrEmpty(form))
                throw new ArgumentException("Form cannot be empty", nameof(form));

            AttestedExamples[language] = form;
        }

        public void AddSemanticShift(SemanticShift shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));

            _semanticShifts.Add(shift);
        }
    }
}
