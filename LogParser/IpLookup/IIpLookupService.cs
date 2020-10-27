using System.Threading.Tasks;

namespace LogParser
{
    public interface IIpLookupService
    {
        Task Lookup();
    }
}
