using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(PhonemeInventoryIdValueConverter))]
    public readonly struct PhonemeInventoryId : IComparable<PhonemeInventoryId>, IEquatable<PhonemeInventoryId>
    {
        public Guid Value { get; }

        public PhonemeInventoryId() { }
        public PhonemeInventoryId(Guid value)
        {
            Value = value;
        }

        public static PhonemeInventoryId New() => new PhonemeInventoryId(Guid.NewGuid());
        public static explicit operator Guid(PhonemeInventoryId id) => id.Value;

        public bool Equals(PhonemeInventoryId other) => Value.Equals(other.Value);
        public int CompareTo(PhonemeInventoryId other) => Value.CompareTo(other.Value);
        public override bool Equals(object obj) => obj is PhonemeInventoryId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(PhonemeInventoryId a, PhonemeInventoryId b) => a.Equals(b);
        public static bool operator !=(PhonemeInventoryId a, PhonemeInventoryId b) => !a.Equals(b);
        public override string ToString() => Value.ToString();

        public class PhonemeInventoryIdValueConverter : ValueConverter<PhonemeInventoryId, Guid>
        {
            public PhonemeInventoryIdValueConverter()
                : base(id => id.Value, value => new PhonemeInventoryId(value)) { }
        }
    }

}
