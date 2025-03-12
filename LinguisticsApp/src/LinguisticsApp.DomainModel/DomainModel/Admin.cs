using LinguisticsApp.DomainModel.RichTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.DomainModel.DomainModel
{
    public class Admin : User
    {
        public bool CanModifyRules { get; private set; }

        private Admin() { }

        public Admin(UserId id, Email username, string password, string firstName, string lastName, bool canModifyRules)
            : base(id, username, password, firstName, lastName)
        {
            CanModifyRules = canModifyRules;
        }

        public void SetModifyRulesPermission(bool canModify)
        {
            CanModifyRules = canModify;
        }
    }

}
