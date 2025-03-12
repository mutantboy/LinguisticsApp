using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(SemanticShiftIdValueConverter))]
    public readonly struct SemanticShiftId : IComparable<SemanticShiftId>, IEquatable<SemanticShiftId>
    {
        public Guid Value { get; }

        public SemanticShiftId() { }

        public SemanticShiftId(Guid value)
        {
            Value = value;
        }

        public static SemanticShiftId New() => new SemanticShiftId(Guid.NewGuid());
        public static explicit operator Guid(SemanticShiftId id) => id.Value;

        public bool Equals(SemanticShiftId other) => Value.Equals(other.Value);
        public int CompareTo(SemanticShiftId other) => Value.CompareTo(other.Value);
        public override bool Equals(object obj) => obj is SemanticShiftId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(SemanticShiftId a, SemanticShiftId b) => a.Equals(b);
        public static bool operator !=(SemanticShiftId a, SemanticShiftId b) => !a.Equals(b);
        public override string ToString() => Value.ToString();

        public class SemanticShiftIdValueConverter : ValueConverter<SemanticShiftId, Guid>
        {
            public SemanticShiftIdValueConverter()
                : base(id => id.Value, value => new SemanticShiftId(value)) { }
        }
    }
}
