using DataLibrary.Classes;
using DataLibrary.Contexts;
using DataLibrary.Interfaces;
using System.Data.SqlTypes;

namespace DataLibrary.Helpers
{
    public class ApiVersionContextHelper(AppDbContext appDbContext) : IApiVersionContextHelper
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public DTOApiVersion GetLatestVersion()
        {
            var version = _appDbContext.ApiVersions
                .OrderByDescending(v => v.BuildDate)
                .FirstOrDefault() ?? throw new SqlNullValueException("No version found in the database");
            return version.GetDTO();
        }
    }
}
