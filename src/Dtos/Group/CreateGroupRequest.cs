using System.ComponentModel.DataAnnotations;

namespace light_quiz_api.Dtos.Group
{
    public class CreateGroupRequest
    {
        [Required]
        public string GroupName { get; set; }
        public List<Guid> MemberIds { get; set; } = [];
    }
}
