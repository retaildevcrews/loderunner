using Ngsa.LodeRunner.DataAccessLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer.Model
{
    interface IBaseLoadEntityModel
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public EntityType EntityType { get; set; }
        public string Name { get; set; }
    }
}
