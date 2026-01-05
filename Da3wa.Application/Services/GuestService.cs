using Da3wa.Application.Interfaces;
using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.Application.Services
{
    public class GuestService : IGuestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQrCodeService _qrCodeService;

        public GuestService(IUnitOfWork unitOfWork, IQrCodeService qrCodeService)
        {
            _unitOfWork = unitOfWork;
            _qrCodeService = qrCodeService;
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

            // Generate QR invitation if guest has an event
            if (addedGuest.EventId.HasValue)
            {
                var guestEvent = await _unitOfWork.Events.GetById(addedGuest.EventId.Value);
                if (guestEvent != null)
                {
                    // Calculate guest number (count of guests in this event)
                    var guestCount = await _unitOfWork.Guests.GetQueryable()
                        .CountAsync(g => g.EventId == addedGuest.EventId && !g.IsDeleted);

                    // Generate QR invitation
                    var invitationPath = await _qrCodeService.GenerateGuestInvitationAsync(
                        addedGuest.Id,
                        guestEvent.Name ?? "Event",
                        guestEvent.ImagePath,
                        addedGuest.Tel?.FirstOrDefault(),
                        guestCount,
                        addedGuest.CreatedOn
                    );

                    // Update guest with invitation path
                    addedGuest.InvitationImagePath = invitationPath;
                    _unitOfWork.Guests.Update(addedGuest);
                    _unitOfWork.Complete();
                }
            }

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
            if (guests == null || guests.Count == 0)
                return 0;

            // Add all guests first
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

            // Group guests by event and generate QR invitations
            var guestsByEvent = guests.Where(g => g.EventId.HasValue && g.EventId.Value > 0).GroupBy(g => g.EventId!.Value);

            foreach (var eventGroup in guestsByEvent)
            {
                var guestEvent = await _unitOfWork.Events.GetById(eventGroup.Key);
                if (guestEvent != null)
                {
                    // Get starting guest number for this event
                    var existingCount = await _unitOfWork.Guests.GetQueryable()
                        .CountAsync(g => g.EventId == eventGroup.Key && !g.IsDeleted);

                    var guestNumber = existingCount - eventGroup.Count() + 1;

                    // Generate QR invitation for each guest
                    foreach (var guest in eventGroup)
                    {
                        var invitationPath = await _qrCodeService.GenerateGuestInvitationAsync(
                            guest.Id,
                            guestEvent.Name ?? "Event",
                            guestEvent.ImagePath,
                            guest.Tel?.FirstOrDefault(),
                            guestNumber,
                            guest.CreatedOn
                        );

                        guest.InvitationImagePath = invitationPath;
                        _unitOfWork.Guests.Update(guest);
                        guestNumber++;
                    }
                }
            }

            _unitOfWork.Complete();
            return guests.Count;
        }
    }
}
