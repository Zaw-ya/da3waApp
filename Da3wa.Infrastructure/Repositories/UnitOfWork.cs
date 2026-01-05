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
        public IBaseRepository<City> Cities => new BaseRepository<City>(_context);
        public IBaseRepository<Country> Countries => new BaseRepository<Country>(_context);
        public IBaseRepository<Event> Events => new BaseRepository<Event>(_context);
        public IBaseRepository<Category> Categories => new BaseRepository<Category>(_context);
        public IBaseRepository<Guest> Guests => new BaseRepository<Guest>(_context);

        public int Complete() => _context.SaveChanges();

        public void Dispose() => _context.Dispose();
    }
}
