namespace light_quiz_api.Dtos.Group
{
    public class RemoveMembersFromGroupRequest
    {
        public string QuizShortCode { get; set; }
        public List<Guid> MemberIds { get; set; }
    }
}
