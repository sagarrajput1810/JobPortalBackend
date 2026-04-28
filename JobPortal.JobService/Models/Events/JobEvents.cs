namespace JobPortal.Shared.Events
{
    public record JobAppliedEvent(int ApplicationId, int JobId, string CandidateEmail, string CandidateName, string ResumeUrl);
    public record ApplicationStatusUpdatedEvent(int ApplicationId, string CandidateEmail, string JobTitle, string NewStatus);
    public record JobCreatedEvent(int Id, string Title, string Description, string CompanyName, string Location, decimal Salary, string RecruiterId);
    public record JobUpdatedEvent(int Id, string Title, string Description, string CompanyName, string Location, decimal Salary, bool IsActive);
    public record JobDeletedEvent(int Id);
}
