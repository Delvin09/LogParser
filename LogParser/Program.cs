using System;
using System.Linq;
using System.Threading.Tasks;

namespace LogParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Application.Create(args)
                .Run();
        }
    }
}
