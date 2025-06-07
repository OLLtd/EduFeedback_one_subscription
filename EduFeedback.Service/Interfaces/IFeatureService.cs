using EduFeedback.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Interfaces
{
    public interface IFeatureService
    {
        public List<FeatureModel> FeatureList(int UserID);

        public List<RoleModel> DefaultFeature(int UserID);
    }
}
