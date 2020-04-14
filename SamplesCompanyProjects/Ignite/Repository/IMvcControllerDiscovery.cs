using Ignite.Areas.Office.Models;
using System.Collections.Generic;

namespace Ignite.Areas.Office.Repository
{
    public interface IMvcControllerDiscovery
    {
        IEnumerable<MvcControllerInfo> GetControllers();
    }
}
