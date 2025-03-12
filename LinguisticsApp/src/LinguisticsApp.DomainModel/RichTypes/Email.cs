using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(EmailValueConverter))]
    public class Email : IEquatable<Email>
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty", nameof(value));

            if (!value.Contains('@') || !value.Contains('.'))
                throw new ArgumentException("Invalid email format", nameof(value));

            Value = value.ToLowerInvariant();
        }

        public static implicit operator string(Email email) => email.Value;
        public static implicit operator Email(string value) => new Email(value);

        public bool Equals(Email other) =>
            other != null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            obj is Email other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;

        public class EmailValueConverter : ValueConverter<Email, string>
        {
            public EmailValueConverter()
                : base(email => email.Value, value => new Email(value)) { }
        }
    }
}
