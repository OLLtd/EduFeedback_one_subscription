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
    public class FeatureService : IFeatureService
    {
        public List<FeatureModel> FeatureList(int UserID)
        {
            var list = new List<FeatureModel>();
            using (var context = new EduFeedEntities())
            {
                var retVal = (from s in context.ROLE_USER_MAP
                              where s.User_ID == UserID
                              select s).ToList();

                foreach (var r in retVal)
                {
                    var vm = new RoleModel();
                    vm.Id = r.RoleID;
                    vm.FeatureList = new List<FeatureModel>();
                    foreach (var f in r.Role.Feature_ROLE_MAP.ToList())
                    {
                        if (f.Feature.IsAvailable == true)
                        {
                            var vm1 = new FeatureModel();
                            vm1.Name = f.Feature.FeatureName;
                            vm1.ControllerName = f.Feature.Controller;
                            vm1.ActionName = f.Feature.Action;
                            vm1.IsVisible = f.Feature.IsVisible;
                            vm.FeatureList.Add(vm1);
                            list.Add(vm1);
                        }
                    }
                }
                return list;
            }
        }

        public List<RoleModel> DefaultFeature(int UserID)
        {
            var list = new List<RoleModel>();
            using (var context = new EduFeedEntities())
            {
                var retVal = (from s in context.ROLE_USER_MAP
                              where s.User_ID == UserID
                              select s).ToList();

                retVal.ForEach(r =>
                {
                    var vm = new RoleModel();
                    vm.Id = r.RoleID;
                    vm.RoleName = r.Role.RoleName;
                    vm.DefaultController = r.Role.Feature.Controller;
                    vm.DefaultAction = r.Role.Feature.Action;
                    list.Add(vm);
                });
                return list;
            }
        }
    }
}
