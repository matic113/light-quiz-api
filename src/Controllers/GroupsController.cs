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
        private readonly UserManager<AppUser> _userManager;

        public GroupsController(ApplicationDbContext context, ShortCodeGeneratorService codeGenerator, UserManager<AppUser> userManager)
        {
            _context = context;
            _codeGenerator = codeGenerator;
            _userManager = userManager;
        }        /// <summary>
        /// Creates a new group with specified members.
        /// </summary>
        /// <remarks>
        /// Creates a new group and adds the specified members to it.
        /// The authenticated user becomes the group creator and is automatically added as a member.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        }        /// <summary>
        /// Adds new members to an existing group.
        /// </summary>
        /// <remarks>
        /// Adds new members to a group by their email addresses.
        /// Only the group creator can add new members.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            foreach (var memberEmail in request.MemberEmails)
            {
                var email = memberEmail.NormalizedEmail();
                var member = group.GroupMembers.FirstOrDefault(gm => gm.Member.NormalizedEmail == email);

                if (member is not null)
                {
                    continue; // Skip adding if already a member
                }

                var user = await _userManager.FindByEmailAsync(email);

                if (user is null)
                {
                    return NotFound($"User with email {memberEmail} not found");
                }

                var groupMember = new GroupMember
                {
                    MemberId = user.Id,
                    GroupId = group.Id,
                };
                memberToAdd.Add(groupMember);
            }

            await _context.AddRangeAsync(memberToAdd);
            await _context.SaveChangesAsync();

            return Ok();
        }        /// <summary>
        /// Removes specified members from a group.
        /// </summary>
        /// <remarks>
        /// Removes members from a group by their email addresses.
        /// Only the group creator can remove members.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

                var groupMember = group.GroupMembers.FirstOrDefault(gm => gm.MemberId == memberId);

                membersToRemove.Add(groupMember);
            }

            _context.RemoveRange(membersToRemove);
            await _context.SaveChangesAsync();

            return Ok();
        }        /// <summary>
        /// Allows a user to join a group using its short code.
        /// </summary>
        /// <remarks>
        /// Enables authenticated users to join a group by providing the group's short code.
        /// Users cannot join groups they are already members of.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        }        /// <summary>
        /// Allows a user to leave a group.
        /// </summary>
        /// <remarks>
        /// Enables authenticated users to leave a group they are currently a member of.
        /// Users cannot leave groups they are not members of.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        }        /// <summary>
        /// Retrieves all groups created by the authenticated user.
        /// </summary>
        /// <remarks>
        /// Returns a list of all groups where the authenticated user is the creator,
        /// including member information and teacher profile details.
        /// </remarks>
        [ProducesResponseType(typeof(IEnumerable<GetGroupResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("created")]
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
        }        /// <summary>
        /// Retrieves all groups where the authenticated user is a member.
        /// </summary>
        /// <remarks>
        /// Returns a list of all groups where the authenticated user is a member,
        /// including member information and teacher profile details.
        /// </remarks>
        [ProducesResponseType(typeof(IEnumerable<GetGroupResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("memberof")]
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
        }        /// <summary>
        /// Retrieves detailed information about a specific group by its short code.
        /// </summary>
        /// <remarks>
        /// Returns comprehensive group information including member details and teacher profile
        /// for a group identified by its short code.
        /// </remarks>
        [ProducesResponseType(typeof(GetGroupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("{shortCode}")]
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
        }        /// <summary>
        /// Deletes a group by its unique identifier.
        /// </summary>
        /// <remarks>
        /// Permanently removes a group from the system using its GUID.
        /// Only authorized users can delete groups.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        }        /// <summary>
        /// Deletes a group by its short code.
        /// </summary>
        /// <remarks>
        /// Permanently removes a group from the system using its short code.
        /// Only authorized users can delete groups.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
