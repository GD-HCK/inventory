using DataLibrary.Classes;
using DataLibrary.Contexts;
using DataLibrary.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace DataLibrary.Helpers
{
    public class ServerContextHelper(AppDbContext appDbContext) : IServerContextHelper
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<DTOServer> PostServerAsync(Server server)
        {
            _appDbContext.Servers.Add(server);

            await _appDbContext.SaveChangesAsync();

            return server.GetDTO();
        }

        public PaginatedList<DTOServer> GetServers(Dictionary<string, string> filters)
        {
            var page = filters.TryGetValue("page", out string? pageValue) && int.TryParse(pageValue, out int parsedPage) ? parsedPage : 1;
            var limit = filters.TryGetValue("limit", out string? limitValue) && int.TryParse(limitValue, out int parsedLimit) ? parsedLimit : 10;
            limit = (limit <= 0 || limit > 100) ? 10 : limit;

            var query = _appDbContext.Servers.AsQueryable();

            foreach (var filter in filters)
            {
                if (string.IsNullOrEmpty(filter.Value))
                    throw new InvalidDataException($"value for property {filter.Key} was null or an empty string");

                if (filter.Key.Equals("page", StringComparison.CurrentCultureIgnoreCase) || filter.Key.Equals("limit", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                var property = typeof(Server).GetProperty(filter.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    var parameter = Expression.Parameter(typeof(Server), "s");
                    var propertyAccess = Expression.Property(parameter, property);

                    if (property.PropertyType == typeof(IList<ServerScope>) || property.PropertyType == typeof(List<ServerScope>))
                    {

                        List<ServerScopeType> scopesEnumList = [..
                            filter.Value.Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(
                                v => Enum.TryParse<ServerScopeType>(v, true, out var scope)
                                ? scope : throw new ArgumentException($"Invalid ServerScope value: {v}")
                            )
                        ];

                        query = query.Where(s => s.Scopes.Any(scope => scopesEnumList.Contains(scope.ScopeType)));

                    }
                    else if (property.PropertyType == typeof(ServerOS) || property.PropertyType == typeof(ServerOS))
                    {

                        List<ServerOSType> osEnumList = [..
                            filter.Value.Split(",", StringSplitOptions.RemoveEmptyEntries)
                           .Select(v => Enum.TryParse<ServerOSType>(v, true, out var scope)
                                ? scope : throw new ArgumentException($"Invalid ServerOS value: {v}")
                           )
                        ];

                        query = query.Where(s => osEnumList.Contains(s.OS.ServerOSType));

                    }
                    else
                    {
                        if (filter.Key.ToLower().Equals("name"))
                        {
                            var nproperty = Expression.Property(parameter, nameof(Server.Name));
                            var notNull = Expression.NotEqual(nproperty, Expression.Constant(null, typeof(string)));
                            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);
                            var containsCall = Expression.Call(nproperty, containsMethod!, Expression.Constant(filter.Value));
                            var andExp = Expression.AndAlso(notNull, containsCall);
                            var lambda = Expression.Lambda<Func<Server, bool>>(andExp, parameter);

                            query = _appDbContext.Servers.Where(lambda);
                        }
                        else
                        {
                            object value;

                            value = Convert.ChangeType(filter.Value, property.PropertyType);

                            var constant = Expression.Constant(value);
                            var exp = Expression.Equal(propertyAccess, constant);

                            var lambda = Expression.Lambda<Func<Server, bool>>(exp, parameter);
                            query = query.Where(lambda);
                        }

                    }
                }
                else
                {
                    throw new InvalidFilterCriteriaException($"filter {filter.Key} is invalid");
                }
            }

            var DTOServers = query
                .Include(s => s.Scopes)
                .Include(s => s.OS)
                .Select(s => s.GetDTO());

            return new PaginatedList<DTOServer>(DTOServers.AsQueryable(), page, limit);
        }

        public async Task<DTOServer> GetServerByIdAsync(int Id)
        {
            var result = await _appDbContext.Servers.Where(x => x.Id == Id).Include(x => x.Scopes).Include(x => x.OS).FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No servers found matching Id {Id}.");

            return result.GetDTO();

        }

        public async Task<DTOServer> PatchServerAsync(ServerPatch serverPatch)
        {
            var existingServer = await _appDbContext.Servers
                .Include(s => s.Scopes)
                .Include(s => s.OS)
                .FirstOrDefaultAsync(s => s.Id == serverPatch.Id) ?? throw new KeyNotFoundException($"Server with ID {serverPatch.Id} not found.");

            // Update scalar properties
            if (!string.IsNullOrEmpty(serverPatch.Name))
                existingServer.Name = serverPatch.Name;

            if (!string.IsNullOrEmpty(serverPatch.IPAddress))
                existingServer.IPAddress = serverPatch.IPAddress;

            if (serverPatch.CreatedAt != default) // Fixed condition
                existingServer.CreatedAt = serverPatch.CreatedAt;

            // Update Scopes
            if (null != serverPatch.Scopes)
            {
                _appDbContext.ServerScopes.RemoveRange(existingServer.Scopes);
                existingServer.Scopes.Clear();
                foreach (var scope in serverPatch.Scopes)
                {
                    existingServer.Scopes.Add(new ServerScope(scope.ScopeType) { ServerId = existingServer.Id });
                }
            }

            // Update OS (one-to-one)
            if (null != serverPatch.OS)
            {
                _appDbContext.ServersOS.Remove(existingServer.OS);
                existingServer.OS = new ServerOS(serverPatch.OS.ServerOSType) { ServerId = existingServer.Id };
            }

            _appDbContext.Servers.Update(existingServer);
            await _appDbContext.SaveChangesAsync();

            // Return the updated entity with includes
            return await GetServerByIdAsync(existingServer.Id);
        }

        public async Task<DTOServer> PutServerAsync(Server server)
        {
            var existingServer = await _appDbContext.Servers
                .Include(s => s.Scopes)
                .Include(s => s.OS)
                .FirstOrDefaultAsync(s => s.Id == server.Id) ?? throw new KeyNotFoundException($"Server with ID {server.Id} not found.");

            // Update scalar properties
            existingServer.Name = server.Name ?? throw new ArgumentNullException(nameof(server), "Server's Name is null");
            existingServer.IPAddress = server.IPAddress ?? throw new ArgumentNullException(nameof(server), "Server's IPAddress is null");
            existingServer.CreatedAt = server.CreatedAt;

            // Update Scopes
            if (null == server.Scopes)
                throw new KeyNotFoundException($"Server is missing the scopes property.");

            _appDbContext.ServerScopes.RemoveRange(existingServer.Scopes);
            existingServer.Scopes.Clear();
            foreach (var scope in server.Scopes)
            {
                existingServer.Scopes.Add(new ServerScope(scope.ScopeType) { ServerId = existingServer.Id });
            }

            // Update OS (one-to-one)
            if (null == server.OS)
                throw new KeyNotFoundException($"Server is missing the OS property.");

            _appDbContext.ServersOS.Remove(existingServer.OS);
            existingServer.OS = new ServerOS(server.OS.ServerOSType) { ServerId = existingServer.Id };

            _appDbContext.Servers.Update(existingServer);
            await _appDbContext.SaveChangesAsync();

            // Return the updated entity with includes
            return await GetServerByIdAsync(existingServer.Id);
        }

        public async Task<int> DeleteServerAsync(int Id)
        {
            var existingServer = _appDbContext.Servers.Find(Id) ?? throw new KeyNotFoundException($"Server with ID {Id} not found.");
            _appDbContext.Servers.Remove(existingServer);
            await _appDbContext.SaveChangesAsync();
            return existingServer.Id;
        }
    }
}
