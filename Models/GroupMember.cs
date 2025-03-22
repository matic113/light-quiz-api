namespace light_quiz_api.Models
{
    public class GroupMember
    {
        public Guid GroupId { get; set; }
        public Guid MemberId { get; set; }

        // Navigational Properties
        public Group Group { get; set; }
        public AppUser Member { get; set; }
    }
}
