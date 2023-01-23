using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class UniverseRepository : RepositoryBase<RouteEntity>, IUniverseRepository
    {
        public UniverseRepository(UniverseDbContext dbContext) : base(dbContext)
        {
        }
    }
}
