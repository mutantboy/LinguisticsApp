using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.DomainModel
{
    public abstract class User : Entity<UserId>
    {
        public Email Username { get; protected set; }
        public string Password { get; protected set; }
        public string FirstName { get; protected set; }
        public string LastName { get; protected set; }

        protected User() { }

        protected User(UserId id, Email username, string password, string firstName, string lastName)
            : base(id)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        }

        public void UpdatePassword(string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("Password cannot be empty", nameof(newPassword));

            Password = newPassword;
        }

        public void UpdateName(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));

            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));

            FirstName = firstName;
            LastName = lastName;
        }
    }
}
