using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    [EfCoreValueConverter(typeof(ProtoFormValueConverter))]
    public class ProtoForm : IEquatable<ProtoForm>
    {
        public string Value { get; }

        public ProtoForm(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Proto-form cannot be empty", nameof(value));

            Value = value;
        }

        public static implicit operator string(ProtoForm form) => form.Value;
        public static implicit operator ProtoForm(string value) => new ProtoForm(value);

        public bool Equals(ProtoForm other) =>
            other != null && Value.Equals(other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is ProtoForm other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;

        public class ProtoFormValueConverter : ValueConverter<ProtoForm, string>
        {
            public ProtoFormValueConverter()
                : base(form => form.Value, value => new ProtoForm(value)) { }
        }
    }
}
