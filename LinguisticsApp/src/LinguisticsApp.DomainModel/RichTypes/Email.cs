using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.RichTypes
{
    public class Email
    {
        public string Value { get; private set; }

        protected Email()
        {
            Value = string.Empty;
        }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty", nameof(value));

            if (!value.Contains('@') || !value.Contains('.'))
                throw new ArgumentException("Invalid email format", nameof(value));

            Value = value.ToLowerInvariant();
        }

        public static implicit operator Email(string value) => new Email(value);
        public static implicit operator string(Email email) => email.Value;
    }
}