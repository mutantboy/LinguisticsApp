using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(LanguageContactEventIdValueConverter))]
    public readonly struct LanguageContactEventId : IComparable<LanguageContactEventId>, IEquatable<LanguageContactEventId>
    {
        public Guid Value { get; }

        public LanguageContactEventId(Guid value)
        {
            Value = value;
        }

        public static LanguageContactEventId New() => new LanguageContactEventId(Guid.NewGuid());
        public static explicit operator Guid(LanguageContactEventId id) => id.Value;

        public bool Equals(LanguageContactEventId other) => Value.Equals(other.Value);
        public int CompareTo(LanguageContactEventId other) => Value.CompareTo(other.Value);
        public override bool Equals(object obj) => obj is LanguageContactEventId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(LanguageContactEventId a, LanguageContactEventId b) => a.Equals(b);
        public static bool operator !=(LanguageContactEventId a, LanguageContactEventId b) => !a.Equals(b);
        public override string ToString() => Value.ToString();

        // EF Core Value Converter
        public class LanguageContactEventIdValueConverter : ValueConverter<LanguageContactEventId, Guid>
        {
            public LanguageContactEventIdValueConverter()
                : base(id => id.Value, value => new LanguageContactEventId(value)) { }
        }
    }
}
