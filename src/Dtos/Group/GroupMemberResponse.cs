﻿namespace light_quiz_api.Dtos.Group
{
    public class GroupMemberResponse
    {
        public Guid MemberId { get; set; }
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
        public string MemberAvatarUrl { get; set; }
    }
}