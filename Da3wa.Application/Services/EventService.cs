using Da3wa.Application.Interfaces;
using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _unitOfWork.Events.GetQueryable()
                .Include(e => e.City)
                .Include(e => e.Category)
                .ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Events.GetQueryable()
                .Include(e => e.City)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Event> CreateAsync(Event @event)
        {
            @event.CreatedOn = DateTime.Now;
            @event.IsDeleted = false;
            var addedEvent = await _unitOfWork.Events.Add(@event);
            _unitOfWork.Complete();
            return addedEvent;
        }

        public async Task UpdateAsync(Event @event)
        {
            @event.LastUpdatedOn = DateTime.Now;
            _unitOfWork.Events.Update(@event);
            _unitOfWork.Complete();
        }

        public async Task DeleteAsync(int id)
        {
            var @event = await _unitOfWork.Events.GetById(id);
            if (@event != null && !@event.IsDeleted)
            {
                @event.IsDeleted = true;
                @event.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Events.Update(@event);
                _unitOfWork.Complete();
            }
        }

        public async Task ToggleDeleteAsync(int id)
        {
            var @event = await _unitOfWork.Events.GetById(id);
            if (@event != null)
            {
                @event.IsDeleted = !@event.IsDeleted;
                @event.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Events.Update(@event);
                _unitOfWork.Complete();
            }
        }
    }
}
