namespace JobPortal.Shared.Events
{
    public record JobAppliedEvent(int JobId, string CandidateEmail, string CandidateName, string JobTitle);
    public record ApplicationStatusUpdatedEvent(int ApplicationId, string CandidateEmail, string JobTitle, string NewStatus);
}
