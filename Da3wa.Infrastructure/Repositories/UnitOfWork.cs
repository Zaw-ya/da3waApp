using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain.Entities;
using Da3wa.Infrastructure.Persistence;

namespace Da3wa.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBaseRepository<ApplicationUser> ApplicationUsers => new BaseRepository<ApplicationUser>(_context);

        public int Complete() => _context.SaveChanges();

        public void Dispose() => _context.Dispose();
    }
}
