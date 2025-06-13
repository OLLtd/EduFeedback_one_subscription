using EduFeedback.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.DatabaseContext
{
    public partial class Registration
    {
        public static Registration GetUserByUserName(string UserName)
        {
            using (var context = new EduFeedEntities())
            {
                return (from s in context.Registrations
                        where s.UserName.ToLower() == UserName.ToLower()
                        select s).FirstOrDefault();
            }
        }

        public static async Task<int> CreateParent(RegistrationModel model, string DeviceID = "WebApp")
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
                   // entity.LastName = model.LastName;
                    entity.Email_ID = model.Email_ID;
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
                   // myLogger.Error(strLogMessage, ex);
                }
                return entity.User_ID;
            }
        }

        public static int AssignRole(int User_ID, String Type)
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
                    //myLogger.Error(strLogMessage);
                }
                return entity.User_ID;
            }
        }

        public static Role GetRole(string Role)
        {
            using (var context = new EduFeedEntities())
            {
                return (from s in context.Roles
                        where s.RoleName.ToLower() == Role.ToLower()
                        select s).FirstOrDefault();
            }
        }
    }
}
