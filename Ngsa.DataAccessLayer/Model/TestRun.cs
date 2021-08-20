using Ngsa.LodeRunner.DataAccessLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ngsa.DataAccessLayer.Model
{
    public class TestRun : IBaseLoadEntityModel
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public EntityType EntityType { get; set; }
        public string Name { get; set; }
        public LoadTestConfig LoadTestConfig { get; set; }
        public List<LoadClient> LoadClients { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompletedTime { get; set; }
        public List<LoadResult> ClientResults { get; set; }
    }
}
