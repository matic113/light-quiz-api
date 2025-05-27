namespace light_quiz_api.Dtos.Group
{
    public class AddMembersToGroupRequest
    {
        public string QuizShortCode { get; set; }
        public List<EmailEntry> MemberEmails { get; set; }
    }
}
