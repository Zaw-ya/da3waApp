using Da3wa.Domain.Entities;

namespace Da3wa.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<ApplicationUser> ApplicationUsers { get; }
        IBaseRepository<City> Cities { get; }
        IBaseRepository<Country> Countries { get; }
        IBaseRepository<Event> Events { get; }
        IBaseRepository<Category> Categories { get; }

        int Complete();
    }
}
