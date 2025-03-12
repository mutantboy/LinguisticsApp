using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(LanguageNameValueConverter))]
    public class LanguageName : IEquatable<LanguageName>
    {
        public string Value { get; }

        protected LanguageName() { }

        public LanguageName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Language name cannot be empty", nameof(value));

            Value = value;
        }

        public static implicit operator string(LanguageName name) => name.Value;
        public static implicit operator LanguageName(string value) => new LanguageName(value);

        public bool Equals(LanguageName other) =>
            other != null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            obj is LanguageName other && Equals(other);

        public override int GetHashCode() =>
            Value.ToLowerInvariant().GetHashCode();

        public override string ToString() => Value;

        public class LanguageNameValueConverter : ValueConverter<LanguageName, string>
        {
            public LanguageNameValueConverter()
                : base(name => name.Value, value => new LanguageName(value)) { }
        }
    }
}
