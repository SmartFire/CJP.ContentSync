using System.Threading.Tasks;
using CJP.ContentSync.Models;
using Orchard;

namespace CJP.ContentSync.Services
{
    public interface IContentExportService : IDependency {
        string GetContentExportText();
        Task<ApiResult> GetContentExportFromUrlAsync(string url, string username, string password);
        ApiResult GetContentExportFromUrl(string url, string username, string password);
    }
}