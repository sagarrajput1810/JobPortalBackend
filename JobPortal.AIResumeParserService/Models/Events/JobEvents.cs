namespace JobPortal.Shared.Events
{
    public record JobAppliedEvent(int ApplicationId, int JobId, string CandidateEmail, string CandidateName, string ResumeUrl);
}
