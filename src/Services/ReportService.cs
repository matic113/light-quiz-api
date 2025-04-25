using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using light_quiz_api.Services;

namespace light_quiz_api.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly FileBlobService _fileService;
        public ReportService(ApplicationDbContext context, FileBlobService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<string> GenerateQuizReport(string quizShortCode)
        {
            var results = await _context.UserResults
                .Include(x => x.User)
                .Include(x => x.Quiz)
                .Where(x => x.QuizShortCode == quizShortCode)
                .Select( ur => new ReportRow
                {
                    Email = ur.User.Email,
                    FullName = ur.User.FullName,
                    Score = ur.Grade,
                    CorrectAnswers = ur.CorrectQuestions ?? 0
                }).ToListAsync();

            var configuration = new OpenXmlConfiguration
            {
                EnableAutoWidth = true,
                FastMode = true,
                TableStyles = TableStyles.Default,
            };

            byte[] excelBytes;
            using (var memoryStream = new MemoryStream())
            {
                await MiniExcel.SaveAsAsync(
                    memoryStream,
                    results,
                    sheetName: "Quiz Results",
                    printHeader: true,
                    configuration: configuration
                );
                excelBytes = memoryStream.ToArray();
            }

            // 3. Upload to blob storage
            string reportName = $"QuizResults_{quizShortCode}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
            string downloadUrl = await _fileService.UploadReportAsync(reportName, excelBytes);

            return downloadUrl;
        }
    }
    public class ReportRow
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
