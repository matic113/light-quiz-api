using light_quiz_api.Dtos.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/group")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ShortCodeGeneratorService _codeGenerator;

        public GroupsController(ApplicationDbContext context, ShortCodeGeneratorService codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupRequest request)
        {
            var shortCode = await _codeGenerator.GenerateQuizShortCodeAsync();
            var userId = GetCurrentUserId();

            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = request.GroupName,
                ShortCode = shortCode,
                CreatedBy = userId,
            };

            await _context.AddAsync(group);
            await _context.SaveChangesAsync();

            var memberToAdd = new List<GroupMember>();

            // Add the creator of the group as a member
            var creatorMember = new GroupMember
            {
                MemberId = userId,
                GroupId = group.Id,
            };

            memberToAdd.Add(creatorMember);

            // Add other members to the group
            foreach (var memberId in request.MemberIds)
            {
                if (memberId == userId)
                {
                    continue; // Skip adding the creator again
                }

                var groupMember = new GroupMember
                {
                    MemberId = memberId,
                    GroupId = group.Id,
                };
                memberToAdd.Add(groupMember);
            }

            await _context.AddRangeAsync(memberToAdd);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroup), new { shortCode }, null);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddMembersToGroup(AddMembersToGroupRequest request)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .FirstOrDefaultAsync(g => g.ShortCode == request.QuizShortCode);

            if (group is null)
            {
                return NotFound("Group not found");
            }

            var userId = GetCurrentUserId();

            if (group.CreatedBy != userId)
            {
                return Unauthorized("You are not allowed to add members to this group");
            }

            var memberToAdd = new List<GroupMember>();

            // Add other members to the group
            foreach (var memberId in request.MemberIds)
            {
                var isAlreadyMember = group.GroupMembers.Any(gm => gm.MemberId == memberId);

                if (isAlreadyMember)
                {
                    continue; // Skip adding if already a member
                }

                var groupMember = new GroupMember
                {
                    MemberId = memberId,
                    GroupId = group.Id,
                };
                memberToAdd.Add(groupMember);
            }

            await _context.AddRangeAsync(memberToAdd);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveMembersFromGroup(RemoveMembersFromGroupRequest request)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .FirstOrDefaultAsync(g => g.ShortCode == request.QuizShortCode);

            if (group is null)
            {
                return NotFound("Group not found");
            }

            var userId = GetCurrentUserId();

            if (group.CreatedBy != userId)
            {
                return Unauthorized("You are not allowed to add members to this group");
            }

            var membersToRemove = new List<GroupMember>();

            // Add other members to the group
            foreach (var memberId in request.MemberIds)
            {
                var isAlreadyMember = group.GroupMembers.Any(gm => gm.MemberId == memberId);

                if (!isAlreadyMember) 
                {
                    continue; // Skip adding if already not a member
                }

                var groupMember = new GroupMember
                {
                    MemberId = memberId,
                    GroupId = group.Id,
                };
                membersToRemove.Add(groupMember);
            }

            _context.RemoveRange(membersToRemove);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("join/{shortCode}")]
        public async Task<IActionResult> JoinGroup(string shortCode)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .FirstOrDefaultAsync(g => g.ShortCode == shortCode);

            if (group is null)
            {
                return NotFound("Group not found");
            }

            var userId = GetCurrentUserId();
            var isAlreadyMember = group.GroupMembers.Any(gm => gm.MemberId == userId);

            if (isAlreadyMember)
            {
                return BadRequest("You are already a member of this group");
            }

            var groupMember = new GroupMember
            {
                MemberId = userId,
                GroupId = group.Id,
            };

            await _context.AddAsync(groupMember);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("leave/{shortCode}")]
        public async Task<IActionResult> LeaveGroup(string shortCode)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .FirstOrDefaultAsync(g => g.ShortCode == shortCode);

            if (group is null)
            {
                return NotFound("Group not found");
            }

            var userId = GetCurrentUserId();

            var groupMember = group.GroupMembers.FirstOrDefault(gm => gm.MemberId == userId);

            if (groupMember is null)
            {
                return BadRequest("You are not a member of this group");
            }
            else
            {
                _context.Remove(groupMember);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("created")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetGroupResponse>))]
        public async Task<ActionResult<IEnumerable<GetGroupResponse>>> GetCreatedGroups()
        {
            var userId = GetCurrentUserId();

            var groups = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .Where(g => g.CreatedBy == userId)
                .ToListAsync();

            var response = new List<GetGroupResponse>();

            foreach (var group in groups)
            {
                var teacher = group.GroupMembers
                    .FirstOrDefault(gm => gm.MemberId == group.CreatedBy);

                var teacherProfile = new TeacherProfile
                {
                    Id = teacher.Member.Id,
                    Name = teacher.Member.FullName,
                    Email = teacher.Member.Email ?? string.Empty,
                    AvatarUrl = teacher.Member.AvatarUrl ?? string.Empty,
                };

                response.Add(new GetGroupResponse
                {
                    GroupId = group.Id,
                    ShortCode = group.ShortCode,
                    Name = group.Name,
                    Teacher = teacherProfile,
                    Members = group.GroupMembers
                        .Where(m => m.MemberId != group.CreatedBy)
                        .Select(m => new GroupMemberResponse
                        {
                            MemberId = m.MemberId,
                            MemberName = m.Member.FullName,
                            MemberEmail = m.Member.Email ?? string.Empty,
                            MemberAvatarUrl = m.Member.AvatarUrl ?? string.Empty,
                        }).ToList()
                });
            }

            return Ok(response);
        }

        [HttpGet("memberof")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetGroupResponse>))]
        public async Task<ActionResult<IEnumerable<GetGroupResponse>>> GetGroupsMemberships()
        {
            var userId = GetCurrentUserId();

            var groups = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .Where(g => g.GroupMembers.Any(gm => gm.MemberId == userId))
                .ToListAsync();

            var response = new List<GetGroupResponse>();

            foreach (var group in groups) {

                var teacher = group.GroupMembers
                    .FirstOrDefault(gm => gm.MemberId == group.CreatedBy);

                var teacherProfile = new TeacherProfile
                {
                    Id = teacher.Member.Id,
                    Name = teacher.Member.FullName,
                    Email = teacher.Member.Email ?? string.Empty,
                    AvatarUrl = teacher.Member.AvatarUrl ?? string.Empty,
                };

                response.Add(new GetGroupResponse
                {
                    GroupId = group.Id,
                    ShortCode = group.ShortCode,
                    Name = group.Name,
                    Teacher = teacherProfile,
                    Members = group.GroupMembers
                        .Where(m => m.MemberId != group.CreatedBy)
                        .Select(m => new GroupMemberResponse
                        {
                            MemberId = m.MemberId,
                            MemberName = m.Member.FullName,
                            MemberEmail = m.Member.Email ?? string.Empty,
                            MemberAvatarUrl = m.Member.AvatarUrl ?? string.Empty,
                        }).ToList()
                });
            }

            return Ok(response);
        }

        [HttpGet("{shortCode}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetGroupResponse))]
        public async Task<ActionResult<GetGroupResponse>> GetGroup(string shortCode)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .FirstOrDefaultAsync(g => g.ShortCode == shortCode);

            if (group == null)
            {
                return NotFound("Group not found");
            }

            var teacher = group.GroupMembers
                .FirstOrDefault(gm => gm.MemberId == group.CreatedBy);

            var teacherProfile = new TeacherProfile
            {
                Id = teacher.Member.Id,
                Name = teacher.Member.FullName,
                Email = teacher.Member.Email ?? string.Empty,
                AvatarUrl = teacher.Member.AvatarUrl ?? string.Empty,
            };

            var response = new GetGroupResponse
            {
                GroupId = group.Id,
                ShortCode = group.ShortCode,
                Name = group.Name,
                Teacher = teacherProfile,
                Members = group.GroupMembers
                    .Where(m => m.MemberId != group.CreatedBy)
                    .Select(m => new GroupMemberResponse
                    {
                        MemberId = m.MemberId,
                        MemberName = m.Member.FullName,
                        MemberEmail = m.Member.Email ?? string.Empty,
                        MemberAvatarUrl = m.Member.AvatarUrl ?? string.Empty,
                    }).ToList()
            };
            return Ok(response);
        }
        [HttpDelete("{groupId:guid}")]
        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("{shortCode}")]
        public async Task<IActionResult> DeleteGroup(string shortCode)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.ShortCode == shortCode);
            if (group == null)
            {
                return NotFound();
            }
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return Ok();
        }
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst("userId");

            if (userIdClaim != null)
            {
                return Guid.Parse(userIdClaim.Value);
            }
            
            return Guid.Empty;
        }
    }
}
