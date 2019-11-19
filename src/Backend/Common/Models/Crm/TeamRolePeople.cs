using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Common.Models.Crm
{
    public class TeamRolePeople
    {
        public int PersonId { get; set; }
        public int PartnerId { get; set; }

        public virtual Person Person { get; set; }
        public virtual Partner Partner { get; set; }
    }
}