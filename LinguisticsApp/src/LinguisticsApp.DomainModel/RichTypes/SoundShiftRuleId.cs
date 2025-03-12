using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(SoundShiftRuleIdValueConverter))]
    public readonly struct SoundShiftRuleId : IComparable<SoundShiftRuleId>, IEquatable<SoundShiftRuleId>
    {
        public Guid Value { get; }

        public SoundShiftRuleId() { }

        public SoundShiftRuleId(Guid value)
        {
            Value = value;
        }

        public static SoundShiftRuleId New() => new SoundShiftRuleId(Guid.NewGuid());
        public static explicit operator Guid(SoundShiftRuleId id) => id.Value;

        public bool Equals(SoundShiftRuleId other) => Value.Equals(other.Value);
        public int CompareTo(SoundShiftRuleId other) => Value.CompareTo(other.Value);
        public override bool Equals(object obj) => obj is SoundShiftRuleId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(SoundShiftRuleId a, SoundShiftRuleId b) => a.Equals(b);
        public static bool operator !=(SoundShiftRuleId a, SoundShiftRuleId b) => !a.Equals(b);
        public override string ToString() => Value.ToString();

        public class SoundShiftRuleIdValueConverter : ValueConverter<SoundShiftRuleId, Guid>
        {
            public SoundShiftRuleIdValueConverter()
                : base(id => id.Value, value => new SoundShiftRuleId(value)) { }
        }
    }
}
