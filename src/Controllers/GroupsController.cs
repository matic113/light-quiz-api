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
        private readonly ILogger<GroupsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ShortCodeGeneratorService _codeGenerator;

        public GroupsController(ILogger<GroupsController> logger, ApplicationDbContext context, ShortCodeGeneratorService codeGenerator)
        {
            _logger = logger;
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
            foreach (var memberId in request.StudentsId)
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

            var response = new GetGroupResponse
            {
                GroupId = group.Id,
                Name = group.Name,
                Members = group.GroupMembers.Select(m => new GroupMemberResponse
                {
                    MemberName = m.Member.FullName,
                    MemberEmail = m.Member.Email ?? string.Empty,
                }).ToList(),
            };
            return Ok(response);
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
