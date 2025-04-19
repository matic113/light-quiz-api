namespace light_quiz_api.Services
{
    public class StudentSubmissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentSubmissionService> _logger;
        private readonly IGradingService _gradingService;
        public StudentSubmissionService(ApplicationDbContext context, ILogger<StudentSubmissionService> logger, IGradingService gradingService)
        {
            _context = context;
            _logger = logger;
            _gradingService = gradingService;
        }
        public async Task AutoSubmitQuizAsync(Guid attemptId)
        {
            var studentAttempt = await _context.QuizAttempts.SingleOrDefaultAsync(x => x.Id == attemptId);

            if (studentAttempt == null)
            {
                _logger.LogError("studentAttempt not found for Auto Submission {AttempId}", attemptId);
                return;
            }

            studentAttempt.State = AttemptState.Submitted;
            await _context.SaveChangesAsync();

            // run the autograding
            await _gradingService.GradeQuizAsync(studentAttempt.StudentId, studentAttempt.QuizId);
        }
    }
}
