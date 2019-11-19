using Company.Common.Models.Deals;
using Company.Deals.Dto.v2;

namespace Company.Deals.Interfaces.v2
{
    public interface I1cTransportCommunicator
    {
        void PostDeal(Deal deal);
        ContractorDto GetPartner(string tin, string rrc, string seasonName);
    }
}
