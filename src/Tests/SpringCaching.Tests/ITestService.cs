using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCaching.Tests
{
    public interface ITestService
    {
        string ServiceId { get; set; }
        Task<List<string>> GetAllNames();
        Task<List<string>> GetNames(int id);
        Task<List<string>> GetNames(string id);
        Task<List<string>> GetNames(TestServiceParam param);

        Task UpdateNames();

    }
}
