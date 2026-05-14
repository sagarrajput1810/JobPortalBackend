using JobPortal.ProfileService.Data;
using JobPortal.ProfileService.Models;
using JobPortal.Shared.Events;
using MassTransit;

namespace JobPortal.ProfileService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly ProfileDbContext _context;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(ProfileDbContext context, ILogger<UserRegisteredConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation($"Profile Service: Creating skeleton profile for {msg.Email} ({msg.Role})");

            if (msg.Role == "Candidate")
            {
                var profile = new CandidateProfile
                {
                    UserId = msg.UserId,
                    FullName = msg.FullName,
                    PhoneNumber = "0000000000", // Placeholder
                    CreatedAt = DateTime.UtcNow
                };
                _context.CandidateProfiles.Add(profile);
            }
            else if (msg.Role == "Recruiter")
            {
                var profile = new RecruiterProfile
                {
                    UserId = msg.UserId,
                    CompanyName = "New Company" // Placeholder
                };
                _context.RecruiterProfiles.Add(profile);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Profile Service: Profile created for {msg.Email}");
        }
    }
}
