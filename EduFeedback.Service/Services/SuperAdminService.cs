using EduFeedback.Core.DatabaseContext;
using EduFeedback.Service.Interfaces;
using EduFeedback.Service.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        public List<OrganisationModel> GetOrganisationsList()
        {
            using (var context = new EduFeedEntities())
            {
                var organisations = (from s in context.Organisations
                                     where s.IsActive == true
                                     select new
                                     {
                                         s.Organisation_ID,
                                         s.OrganisationName,
                                         s.CreatedDate,
                                         s.IsActive,
                                         s.EmailContact,
                                         s.EmailCode,
                                         s.WebsiteUrl,
                                         s.ModifiedDate,
                                         s.Org_ID,
                                         s.IsExternalClient,
                                         s.Address,
                                         s.Phone,
                                         s.OrganisationType
                                     }).ToList();

                return organisations.Select(s => new OrganisationModel
                {
                    Organisation_ID = s.Organisation_ID,
                    OrganisationName = s.OrganisationName,
                    CreatedDate = s.CreatedDate,
                    IsActive = s.IsActive,
                    EmailContact = s.EmailContact,
                    EmailCode = s.EmailCode,
                    WebsiteUrl = s.WebsiteUrl,
                    ModifiedDate = s.ModifiedDate,
                    Org_ID = s.Org_ID,
                    IsExternalClient = s.IsExternalClient,
                    Address = s.Address,
                    Phone = s.Phone,
                    //OrganisationType = OrganisationModel.GetOrganisationTypeEnum(s.OrganisationType)
                }).ToList();
            }
        }

        public bool CheckOrganisationExist(string orgName)
        {
            using (var context = new EduFeedEntities())
            {
                return context.Organisations
                              .Any(s => s.OrganisationName.ToLower().Contains(orgName.ToLower()));
            }
        }

        public bool CreateOrganisation(OrganisationModel organisationModel)
        {
            using (var context = new EduFeedEntities())
            {
                var organisation = new Organisation
                {
                    OrganisationName = organisationModel.OrganisationName,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    EmailContact = organisationModel.EmailContact,
                    //EmailCode = organisationModel.EmailCode,
                    //WebsiteUrl = organisationModel.WebsiteUrl,
                    ModifiedDate = DateTime.Now,
                    Org_ID = Guid.NewGuid(),
                    IsExternalClient = true,
                    Address = organisationModel.Address,
                    Phone = organisationModel.Phone,
                    OrganisationType = OrganisationModel.GetOrganisationTypeString(organisationModel.OrganisationType)
                };

                context.Organisations.Add(organisation);
                return context.SaveChanges() > 0;
            }
        }
    }

}

