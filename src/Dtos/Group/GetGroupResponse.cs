namespace light_quiz_api.Dtos.Group
{
    public class GetGroupResponse
    {
        public Guid GroupId { get; set; }
        public string ShortCode { get; set; }
        public string Name { get; set; }
        public TeacherProfile Teacher { get; set; }
        public List<GroupMemberResponse> Members { get; set; }
    }
}
