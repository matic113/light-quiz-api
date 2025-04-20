
using System.Text.Json;
using light_quiz_api.Dtos;
using light_quiz_api.Dtos.Question;

namespace light_quiz_api.Services
{
    public class GradingService : IGradingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<GradingService> _logger;
        public GradingService(ApplicationDbContext context, IGeminiService geminiService, ILogger<GradingService> logger)
        {
            _context = context;
            _geminiService = geminiService;
            _logger = logger;
        }

        public async Task GradeQuizAsync(Guid studentId, Guid quizId)
        {
            _logger.LogInformation($"Grading quiz {quizId} for student {studentId}");

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz is null)
            {
                _logger.LogWarning($"Quiz {quizId} not found");
                return;
            }

            var studentAnswers = await _context.StudentAnswers
                .Where(sa => sa.UserId == studentId && sa.Question.QuizId == quizId)
                .ToListAsync();

            if (studentAnswers.Count == 0)
            {
                _logger.LogWarning($"No answers found for student {studentId} in quiz {quizId}");
                return;
            }

            var questionsToGrade = new List<GradeQuestionRequest>();

            foreach (var question in quiz.Questions)
            {
                var studentAnswer = studentAnswers.FirstOrDefault(sa => sa.QuestionId == question.Id);

                if (studentAnswer is null)
                {
                    _logger.LogWarning($"No student answer found for question {question.Id} in quiz {quizId}");
                    continue;
                }

                // check if the question is a multiple choice question
                if (question.QuestionOptions.Count > 0)
                {
                    var correctOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                    if (correctOption != null)
                    {
                        questionsToGrade.Add(new GradeQuestionRequest
                        {
                            QuestionId = question.Id,
                            QuestionText = question.QuestionText,
                            CorrectAnswer = correctOption.OptionLetter.ToString(),
                            StudentAnswer = studentAnswer?.AnswerOptionLetter?.ToString()
                        });
                    }
                    continue;
                }

                // if the question is not multiple choice.
                questionsToGrade.Add(new GradeQuestionRequest
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    CorrectAnswer = question.CorrectAnswer,
                    StudentAnswer = studentAnswer.AnswerText ?? string.Empty
                });
            }

            if (questionsToGrade.Count == 0)
            {
                _logger.LogWarning($"No questions to grade for quiz {quizId}");
                return;
            }

            var prompt = """
                You are a grading assistant. You will get a list of questions in json format for each question you will get student answer and correct answer your job is to grade each answer against the correct answer you have to respond in json only with this format dont respond with {
                    "results" : [
                        {
                            "questionId": "guid",
                            "rating": out of 10,
                            "confidence": out of 10,
                            "feedback": "string"
                        }
                    ]
                }
                INPUT:
                """;

            var input = JsonSerializer.Serialize(questionsToGrade);
            var fullPrompt = prompt + input;

            _logger.LogInformation($"Gemini prompt: {fullPrompt}");

            var response = await _geminiService.GetGeminiResponseAsync(fullPrompt);

            _logger.LogInformation($"Gemini response: {response}");

            var cleanedResponse = ExtractJsonFromMarkdown(response);

            _logger.LogInformation($"Cleaned response: {cleanedResponse}");

            var gradingResponse = JsonSerializer.Deserialize<GradingResponse>(cleanedResponse);

            if (gradingResponse is null)
            {
                _logger.LogWarning($"Failed to deserialize grading response for quiz {quizId}");
                return;
            }

            _logger.LogInformation($"Saving student grading results");

            var totalPoints = 0;

            foreach (var result in gradingResponse.Results)
            {
                var studentAnswer = studentAnswers.FirstOrDefault(sa => sa.QuestionId == result.QuestionId);

                studentAnswer.GradingRating = result.Rating;
                studentAnswer.GradingConfidence = result.Confidence;
                studentAnswer.GradingFeedback = result.Feedback;

                if (result.Rating > 5)
                {
                    totalPoints += quiz.Questions.FirstOrDefault(q => q.Id == result.QuestionId)?.Points ?? 0;
                }
            }

            var possiblePoints = quiz.Questions.Sum(q => q.Points);

            var finalResult = new UserResult
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                QuizTitle = quiz.Title,
                UserId = studentId,
                Grade =totalPoints,
                PossiblePoints = possiblePoints,
                CorrectQuestions = gradingResponse.Results.Count(r => r.Rating > 5),
                TotalQuestion = quiz.Questions.Count,
                CreatedAt = DateTime.UtcNow
            };
            await _context.UserResults.AddAsync(finalResult);

            await _context.SaveChangesAsync();
        }
        public static string ExtractJsonFromMarkdown(string markdown)
        {
            // Handles both ```json and ``` style code blocks
            var startTag = "```json";
            var endTag = "```";

            int startIndex = markdown.IndexOf(startTag);
            if (startIndex == -1) startIndex = markdown.IndexOf("```");
            if (startIndex == -1) return markdown; // No code block found

            startIndex = markdown.IndexOf('\n', startIndex) + 1;
            int endIndex = markdown.LastIndexOf(endTag);

            if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
                return markdown; // Invalid formatting

            return markdown.Substring(startIndex, endIndex - startIndex).Trim();
        }

    } 
}
