using Microsoft.AspNetCore.Http;
using Company.Common.Models.Seasons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Pim.Client.v2
{
    public interface ISeasonsMsCommunicator
    {
        Task<DiscountPolicy> GetDiscountPolicyAsync(int id, HttpContext context);
    }
}
