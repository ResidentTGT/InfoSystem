using System.Collections.Generic;
using Company.Common.Enums;

namespace Company.Common.Models.Crm
{
    public class Partner
    {
        public Partner()
        {
            TeamRolePeoples = new List<TeamRolePeople>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public PartnerType Type { get; set; }

        public virtual ICollection<TeamRolePeople> TeamRolePeoples { get; set; }
    }
}