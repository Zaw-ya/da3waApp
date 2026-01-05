namespace Da3wa.Application.Interfaces
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Generates a QR code invitation image for a guest
        /// </summary>
        /// <param name="guestId">The guest ID to encode in QR</param>
        /// <param name="eventName">The event name (used for folder organization)</param>
        /// <param name="eventImagePath">Optional event background image path</param>
        /// <param name="guestNumber">The guest number for file naming</param>
        /// <param name="createdDate">Creation date for metadata</param>
        /// <returns>The relative path to the generated invitation image</returns>
        Task<string> GenerateGuestInvitationAsync(int guestId, string eventName, string? eventImagePath, string? phoneNumber, int guestNumber, DateTime createdDate);

        /// <summary>
        /// Deletes a guest invitation image
        /// </summary>
        /// <param name="imagePath">The path to the image to delete</param>
        Task DeleteInvitationAsync(string imagePath);
    }
}
