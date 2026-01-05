using Da3wa.Application.Interfaces;
using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.Application.Services
{
    public class GuestService : IGuestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GuestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Guest>> GetAllAsync()
        {
            return await _unitOfWork.Guests.GetQueryable().Include(g => g.Event).ToListAsync();
        }

        public async Task<Guest?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Guests.GetById(id);
        }

        public async Task<Guest> CreateAsync(Guest guest)
        {
            guest.CreatedOn = DateTime.Now;
            guest.IsDeleted = false;
            var addedGuest = await _unitOfWork.Guests.Add(guest);
            _unitOfWork.Complete();
            return addedGuest;
        }

        public async Task UpdateAsync(Guest guest)
        {
            guest.LastUpdatedOn = DateTime.Now;
            _unitOfWork.Guests.Update(guest);
            _unitOfWork.Complete();
        }

        public async Task DeleteAsync(int id)
        {
            var guest = await _unitOfWork.Guests.GetById(id);
            if (guest != null && !guest.IsDeleted)
            {
                guest.IsDeleted = true;
                guest.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Guests.Update(guest);
                _unitOfWork.Complete();
            }
        }

        public async Task ToggleDeleteAsync(int id)
        {
            var guest = await _unitOfWork.Guests.GetById(id);
            if (guest != null)
            {
                guest.IsDeleted = !guest.IsDeleted;
                guest.LastUpdatedOn = DateTime.Now;
                _unitOfWork.Guests.Update(guest);
                _unitOfWork.Complete();
            }
        }

        public async Task<List<Guest>> ImportFromVcfAsync(Stream vcfStream, int eventId)
        {
            var guests = new List<Guest>();
            using var reader = new StreamReader(vcfStream);
            string? line;
            Guest? currentGuest = null;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("BEGIN:VCARD"))
                {
                    currentGuest = new Guest { EventId = eventId };
                }
                else if (line.StartsWith("FN:") && currentGuest != null)
                {
                    currentGuest.FullName = line.Substring(3);
                }
                else if (line.StartsWith("TEL:") && currentGuest != null)
                {
                    var tel = line.Substring(4);
                    currentGuest.Tel.Add(tel);
                }
                else if (line.StartsWith("END:VCARD") && currentGuest != null)
                {
                    if (!string.IsNullOrEmpty(currentGuest.FullName))
                    {
                        guests.Add(currentGuest);
                    }
                    currentGuest = null;
                }
            }

            // Save guests to database
            foreach (var guest in guests)
            {
                await CreateAsync(guest);
            }

            return guests;
        }
    }
}
