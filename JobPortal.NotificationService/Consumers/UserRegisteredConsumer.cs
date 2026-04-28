using JobPortal.Shared.Events;
using MassTransit;

namespace JobPortal.NotificationService.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var userEvent = context.Message;
            _logger.LogInformation($"Notification Sent: Welcome {userEvent.Email}! You registered as {userEvent.Role}.");
            Console.WriteLine(userEvent);
            
            // yha email bhejne k liye logic likha agar sir ne kha to
            await Task.CompletedTask;
        }
    }
}