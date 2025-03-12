using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(UserIdValueConverter))]
    public readonly struct UserId : IComparable<UserId>, IEquatable<UserId>
    {
        public Guid Value { get; }

        public UserId() : this(Guid.Empty) { }

        public UserId(Guid value)
        {
            Value = value;
        }

        public static UserId New() => new UserId(Guid.NewGuid());
        public static explicit operator Guid(UserId id) => id.Value;

        public bool Equals(UserId other) => Value.Equals(other.Value);
        public int CompareTo(UserId other) => Value.CompareTo(other.Value);
        public override bool Equals(object obj) => obj is UserId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(UserId a, UserId b) => a.Equals(b);
        public static bool operator !=(UserId a, UserId b) => !a.Equals(b);
        public override string ToString() => Value.ToString();

        public class UserIdValueConverter : ValueConverter<UserId, Guid>
        {
            public UserIdValueConverter()
                : base(id => id.Value, value => new UserId(value)) { }
        }
    }
}
