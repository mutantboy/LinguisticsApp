using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(CognateSetIdValueConverter))]
    public readonly struct CognateSetId : IComparable<CognateSetId>, IEquatable<CognateSetId>
    {
        public Guid Value { get; }

        public CognateSetId(Guid value)
        {
            Value = value;
        }

        public static CognateSetId New() => new CognateSetId(Guid.NewGuid());
        public static explicit operator Guid(CognateSetId id) => id.Value;

        public bool Equals(CognateSetId other) => Value.Equals(other.Value);
        public int CompareTo(CognateSetId other) => Value.CompareTo(other.Value);
        public override bool Equals(object obj) => obj is CognateSetId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(CognateSetId a, CognateSetId b) => a.Equals(b);
        public static bool operator !=(CognateSetId a, CognateSetId b) => !a.Equals(b);
        public override string ToString() => Value.ToString();

        public class CognateSetIdValueConverter : ValueConverter<CognateSetId, Guid>
        {
            public CognateSetIdValueConverter()
                : base(id => id.Value, value => new CognateSetId(value)) { }
        }
    }
}
