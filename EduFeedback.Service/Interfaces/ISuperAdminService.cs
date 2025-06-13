using EduFeedback.Service.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Interfaces
{
    public interface ISuperAdminService
    {
        public List<OrganisationModel> GetOrganisationsList();

        public bool CheckOrganisationExist(String orgName);

        public  bool CreateOrganisation(OrganisationModel organisationModel);
    }
}
