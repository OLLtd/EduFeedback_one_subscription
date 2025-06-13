using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.ServiceModels
{
    public class FeatureModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool? IsVisible { get; set; }
        public bool IsAvailable { get; set; }
    }
    public class RoleModel
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string DefaultController { get; set; }
        public string DefaultAction { get; set; }
        public List<FeatureModel> FeatureList { get; set; }
    }
}
