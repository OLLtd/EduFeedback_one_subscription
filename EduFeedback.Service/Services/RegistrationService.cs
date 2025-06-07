using EduFeedback.Common;
using EduFeedback.Core.DatabaseContext;
using EduFeedback.Service.Interfaces;
using EduFeedback.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Services
{
    public class RegistrationService : IRegistrationService
    {
        //public bool FirstTimeRegisterUser()
        //{
        //    //string confirmationToken = WebSecurity.CreateUserAndAccount(model.UserName, model.Password, null);

        //    return true;
        //}
        public RegistrationModel GetUserByUserName(string UserName)
        {
            using (var context = new EduFeedEntities())
            {
                var response = (from s in context.Registrations
                                where s.UserName.ToLower() == UserName.ToLower()
                                select new RegistrationModel()
                                {
                                    User_ID = s.User_ID,
                                    FirstName = s.FirstName,
                                    LastName = !string.IsNullOrEmpty(s.LastName) ? s.LastName : string.Empty,
                                    UserName = s.UserName,
                                    Email_ID = s.Email_ID,
                                    Type = s.Type,
                                    OrganisationId = (int)s.OrgnKey,
                                }).FirstOrDefault();

                return response;
            }
        }

        public async Task<int> RegisterAdminUser(RegistrationModel model)
        {
            var getRandomPassword = RandomPassword.Generate(8, 10);
            var entity = new Registration();
            using (var context = new EduFeedEntities())
            {
                try
                {
                    var dataUser = (from o in context.Registrations
                                    where o.UserName.ToLower() == model.UserName.ToLower()
                                    select o).FirstOrDefault();

                    if (dataUser == null)
                    {
                        entity.FirstName = model.FirstName;
                        entity.LastName = string.Empty;
                        entity.Email_ID = model.Email_ID;
                        entity.UserName = model.UserName;
                        entity.PhoneNo = "0";
                        entity.Year_ID = 2;
                        entity.IsActive = true;
                        entity.Type = model.Type;
                        entity.OrgnKey = 2;
                        entity.ClientUser_ID = model.Email_ID;
                        context.Registrations.Add(entity);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        return await Task.Run(() => dataUser.User_ID);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return await Task.Run(() => entity.User_ID);
        }

        public async Task<int> CreateParent(RegistrationModel model, string DeviceID = "WebApp")
        {
            //int User_ID = 0;
            using (var context = new EduFeedEntities())
            {
                if (context.Database.Connection.State == System.Data.ConnectionState.Closed)
                    context.Database.Connection.Open();
                var entity = new Registration();
                try
                {
                    entity.FirstName = model.FirstName;
                    entity.LastName = model.LastName;
                    entity.Email_ID = model.Email_ID;
                    entity.PhoneNo = model.PhoneNo;
                    entity.Year_ID = model.Year_ID;
                    entity.IsActive = true;
                    entity.UserName = model.Email_ID;
                    entity.Type = model.Type;
                    entity.RegisteredFrom = DeviceID;
                    entity.OrgnKey = 1;
                    entity.ClientUser_ID = model.FirstName;
                    context.Registrations.Add(entity);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    String strLogMessage = "Registration Partial > CreateParent. ";
                    strLogMessage += " In Method (CreateParent) , with message : " + ex.Message;
                    //   myLogger.Error(strLogMessage, ex);
                }
                return entity.User_ID;
            }
        }

        public int AssignRole(int User_ID, String Type)
        {
            using (var context = new EduFeedEntities())
            {
                var entity = new ROLE_USER_MAP();
                try
                {
                    entity.User_ID = User_ID;
                    entity.RoleID = GetRole(Type).RoleID;
                    context.ROLE_USER_MAP.Add(entity);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    String strLogMessage = "Registration Partial > AssignRole. ";
                    strLogMessage += " In Method (AssignRole) , with message : " + ex.Message;
                    // myLogger.Error(strLogMessage);
                }
                return entity.User_ID;
            }
        }

        public Role GetRole(string Role)
        {
            using (var context = new EduFeedEntities())
            {
                return (from s in context.Roles
                        where s.RoleName.ToLower() == Role.ToLower()
                        select s).SingleOrDefault();
            }
        }

        public void GetOrganisationList()
        {
            using (var context = new EduFeedEntities())
            {
                var data = (from o in context.Organisations
                            select o).ToList();

            }
            //  return await Task.FromResult(void);
        }

        public OrganisationModel GetOrganisationDetail(int organisationID = 0)
        {
            using (var context = new EduFeedEntities())
            {
                OrganisationModel om = new OrganisationModel();
                try
                {
                    var organisation = (from o in context.Organisations
                                        where o.Organisation_ID == organisationID
                                        select o).FirstOrDefault();

                    if (organisation != null)
                    {
                        om = new OrganisationModel
                        {
                            Organisation_ID = organisation.Organisation_ID,
                            OrganisationName = organisation.OrganisationName,
                            Org_ID = organisation.Org_ID,
                            IsActive = organisation.IsActive,
                            StudentNameMapping = organisation.StudentNameMapping, // Compute here
                            EmailCode = organisation.EmailCode ?? ""
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (avoid empty catch blocks)
                    // e.g., Console.WriteLine(ex.Message);
                }
                return om;
            }
        }

        public List<SchoolYearModel> GetSchoolYearlist()
        {
            using (var context = new EduFeedEntities())
            {
                var List = (from s in context.SchoolYears
                            where s.IsActive == true
                            select s).ToList();
                var SchoolYearList = new List<SchoolYearModel>();
                List.ForEach(x =>
                {
                    SchoolYearList.Add(new SchoolYearModel()
                    {
                        Year_ID = x.Year_ID,
                        YearName = "School " + x.YearName
                    });

                });
                return SchoolYearList;
            }
        }

        public List<SubjectModel> SubjectList()
        {
            using (var context = new EduFeedEntities())
            {
                var List = (from s in context.Subjects
                            where s.IsActive == true
                            select s).ToList();

                var SubjectList = new List<SubjectModel>();

                List.ForEach(x =>
                {
                    SubjectList.Add(new SubjectModel()
                    {
                        ID = x.Subject_ID,
                        SubjectName = x.Subject_name
                    });

                });
                return SubjectList;
            }
        }

        public SubjectModel SubjectDetail(int subjectId)
        {
            using (var context = new EduFeedEntities())
            {
                var subjectDetail = (from s in context.Subjects
                                     where s.Subject_ID == subjectId
                                     select s).FirstOrDefault();

                var ModelData = new SubjectModel
                {
                    ID = subjectDetail.Subject_ID,
                    SubjectName = subjectDetail.Subject_name
                };
                return ModelData;
            }
        }

    }
}
