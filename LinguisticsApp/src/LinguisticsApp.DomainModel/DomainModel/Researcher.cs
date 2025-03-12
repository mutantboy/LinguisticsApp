using LinguisticsApp.DomainModel.Enumerations;
using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.DomainModel
{
    public class Researcher : User
    {
        public string Institution { get; private set; }
        public ResearchField Field { get; private set; }

        private Researcher() { }

        public Researcher(UserId id, Email username, string password, string firstName, string lastName,
                        string institution, ResearchField field)
            : base(id, username, password, firstName, lastName)
        {
            Institution = institution ?? throw new ArgumentNullException(nameof(institution));
            Field = field;
        }

        public void UpdateInstitution(string newInstitution)
        {
            if (string.IsNullOrEmpty(newInstitution))
                throw new ArgumentException("Institution cannot be empty", nameof(newInstitution));

            Institution = newInstitution;
        }

        public void UpdateField(ResearchField newField)
        {
            Field = newField;
        }
    }
}
