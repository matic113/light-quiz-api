namespace light_quiz_api.Models
{
    public class Invitation
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid InviterId { get; set; }
        public string InviteeEmail { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigational properties
        public Group Group { get; set; }
        public AppUser Inviter { get; set; }
        public AppUser Invitee { get; set; }
    }
}
