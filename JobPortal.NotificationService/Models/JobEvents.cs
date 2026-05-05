namespace JobPortal.Shared.Events
{
    public record JobAppliedEvent(int ApplicationId, int JobId, string CandidateEmail, string CandidateName, string ResumeUrl, string JobTitle, string CompanyName);
    public record ApplicationStatusUpdatedEvent(int ApplicationId, string CandidateEmail, string JobTitle, string NewStatus);
}
