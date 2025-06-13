using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EduFeedback.Service.ServiceModels
{
    public class OrganisationModel
    {
        public int Organisation_ID { get; set; }
        public string OrganisationName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string EmailContact { get; set; }
        public string EmailCode { get; set; }
        public string WebsiteUrl { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<System.Guid> Org_ID { get; set; }
        public Nullable<bool> IsExternalClient { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool StudentNameMapping { get; set; }

        public OrganisationTypeEnum OrganisationType { get; set; } // Updated to use enum

        public enum OrganisationTypeEnum
        {
            School,
            Tution
        }
        public static OrganisationTypeEnum GetOrganisationTypeEnum(string organisationType)
        {
            return organisationType switch
            {
                "School" => OrganisationTypeEnum.School,
                "Tution" => OrganisationTypeEnum.Tution,
                _ => throw new ArgumentException("Invalid organisation type", nameof(organisationType))
            };
        }

        public static string GetOrganisationTypeString(OrganisationTypeEnum organisationTypeEnum)
        {
            return organisationTypeEnum switch
            {
                OrganisationTypeEnum.School => "School",
                OrganisationTypeEnum.Tution => "Tution",
                _ => throw new ArgumentException("Invalid organisation type enum", nameof(organisationTypeEnum))
            };
        }
    }


    public class OrganisationFilterModel
    {
        public string Organisation { get; set; }

        public string SearchOrganisation { get; set; }

        public IEnumerable<SelectListItem> OrganisationList { get; set; }
    }

}
