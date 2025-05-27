namespace light_quiz_api.Models
{
    public class Group
    {
        public Guid Id { get; set; }
        public string ShortCode { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }

        // Navigational Properties

        public ICollection<GroupMember> GroupMembers { get; set; }
        public ICollection<Quiz> Quizzes { get; set; }
        public ICollection<Invitation> Invitations { get; set; }
    }
}
