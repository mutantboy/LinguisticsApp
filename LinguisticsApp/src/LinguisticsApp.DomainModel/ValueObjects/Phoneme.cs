using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.ValueObjects
{
    public class Phoneme : ValueObject
    {
        public string Value { get; }
        public bool IsVowel { get; }

        protected Phoneme() { }

        public Phoneme(string value, bool isVowel)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phoneme cannot be empty", nameof(value));

            Value = value;
            IsVowel = isVowel;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return IsVowel;
        }

        public override string ToString() => Value;
    }
}
