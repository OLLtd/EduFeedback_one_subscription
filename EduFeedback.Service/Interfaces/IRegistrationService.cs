using EduFeedback.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Interfaces
{
    public interface IRegistrationService
    {
        public abstract RegistrationModel GetUserByUserName(string UserName);

        public Task<int> RegisterAdminUser(RegistrationModel model);

        public int AssignRole(int User_ID, String Type);

        public void GetOrganisationList();

        public List<SchoolYearModel> GetSchoolYearlist();

        public OrganisationModel GetOrganisationDetail(int organisationID = 0);

        public SubjectModel SubjectDetail(int subjectId);
    }
}
