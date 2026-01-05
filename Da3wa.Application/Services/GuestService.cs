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

        public async Task<List<Guest>> ParseVcfAsync(Stream vcfStream, int eventId)
        {
            var guests = new List<Guest>();
            using var reader = new StreamReader(vcfStream);
            string? line;
            Guest? currentGuest = null;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("BEGIN:VCARD"))
                {
                    currentGuest = new Guest
                    {
                        EventId = eventId,
                        ExpireAt = DateTime.Now.AddDays(30),
                        IsAttending = false,
                        FamilyNumber = 1
                    };
                }
                else if (line.StartsWith("FN:") && currentGuest != null)
                {
                    currentGuest.FullName = line.Substring(3);
                }
                else if (line.StartsWith("TEL") && currentGuest != null)
                {
                    // Extract phone number from various TEL formats
                    // Examples:
                    // TEL:+123456789
                    // TEL;type=Mobile:+123456789
                    // TEL;type=Mobile;waid=201004987132:+20 10 04987132

                    var telLine = line.Substring(3).Trim(); // Remove "TEL"

                    // Check if there's a waid parameter
                    string? phoneNumber = null;

                    if (telLine.Contains("waid="))
                    {
                        // Extract waid number
                        var waidStart = telLine.IndexOf("waid=") + 5;
                        var waidEnd = telLine.IndexOf(":", waidStart);
                        if (waidEnd == -1) waidEnd = telLine.Length;

                        phoneNumber = telLine.Substring(waidStart, waidEnd - waidStart).Trim();

                        // Add + prefix if not present
                        if (!phoneNumber.StartsWith("+"))
                        {
                            phoneNumber = "+" + phoneNumber;
                        }
                    }
                    else if (telLine.Contains(":"))
                    {
                        // Extract number after colon
                        var colonIndex = telLine.IndexOf(":");
                        phoneNumber = telLine.Substring(colonIndex + 1).Trim();
                    }
                    else
                    {
                        // Simple format TEL:number
                        phoneNumber = telLine.Trim();
                    }

                    // Clean phone number (remove spaces)
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        phoneNumber = phoneNumber.Replace(" ", "");
                        currentGuest.Tel.Add(phoneNumber);
                    }
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

            return guests;
        }

        public async Task<List<Guest>> ImportFromVcfAsync(Stream vcfStream, int eventId)
        {
            var guests = await ParseVcfAsync(vcfStream, eventId);

            // Save guests to database
            foreach (var guest in guests)
            {
                await CreateAsync(guest);
            }

            return guests;
        }

        public async Task<int> CreateManyAsync(List<Guest> guests)
        {
            foreach (var guest in guests)
            {
                guest.CreatedOn = DateTime.Now;
                guest.IsDeleted = false;
                if (guest.ExpireAt == default)
                {
                    guest.ExpireAt = DateTime.Now.AddDays(30);
                }
                await _unitOfWork.Guests.Add(guest);
            }
            _unitOfWork.Complete();
            return guests.Count;
        }
    }
}
