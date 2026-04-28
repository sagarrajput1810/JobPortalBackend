using JobPortal.SearchService.Models;
using JobPortal.SearchService.Services;
using JobPortal.Shared.Events;
using MassTransit;

namespace JobPortal.SearchService.Consumers
{
    public class JobEventsConsumer : 
        IConsumer<JobCreatedEvent>,
        IConsumer<JobUpdatedEvent>,
        IConsumer<JobDeletedEvent>
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<JobEventsConsumer> _logger;

        public JobEventsConsumer(ISearchService searchService, ILogger<JobEventsConsumer> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<JobCreatedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation($"Search Service: Indexing new job: {msg.Title}");

            var jobDoc = new JobDocument
            {
                Id = msg.Id,
                Title = msg.Title,
                CompanyName = msg.CompanyName,
                Location = msg.Location,
                Description = msg.Description
            };

            await _searchService.IndexJobAsync(jobDoc);
        }

        public async Task Consume(ConsumeContext<JobUpdatedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation($"Search Service: Updating job index: {msg.Title}");

            if (!msg.IsActive)
            {
                await _searchService.DeleteJobAsync(msg.Id);
                return;
            }

            var jobDoc = new JobDocument
            {
                Id = msg.Id,
                Title = msg.Title,
                CompanyName = msg.CompanyName,
                Location = msg.Location,
                Description = msg.Description
            };

            await _searchService.UpdateJobAsync(jobDoc);
        }

        public async Task Consume(ConsumeContext<JobDeletedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation($"Search Service: Deleting job from index: {msg.Id}");
            await _searchService.DeleteJobAsync(msg.Id);
        }
    }
}
