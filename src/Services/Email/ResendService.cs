using System.Diagnostics;
using GenerativeAI.Types;
using Resend;

namespace light_quiz_api.Services.Email
{
    public class ResendService
    {
        private readonly IResend _resend;
        private readonly ILogger<ResendService> _logger;
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;
        public ResendService(IResend resend, IRazorViewToStringRenderer razorViewToStringRenderer, ILogger<ResendService> logger)
        {
            _resend = resend;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _logger = logger;
        }
        public async Task SendEmailAsync()
        {
            var message = new EmailMessage();
            message.From = "support@no-reply.theknight.tech";
            message.To.Add("faresma7moud1@gmail.com");
            message.Subject = "Welcome To Light-Quiz";
            message.HtmlBody = "<strong>Your Registration for Light-Quiz is successfull you can now login and take your quiz!</strong>";

            await _resend.EmailSendAsync(message);
        }
        public async Task SendEmailWithTemplateAsync<TModel>(string receipient, string subject, string viewName, TModel viewModel)
        {
            var renderStopwatch = Stopwatch.StartNew();
            string htmlBody = await _razorViewToStringRenderer.RenderViewToStringAsync(viewName, viewModel);
            renderStopwatch.Stop();
            _logger.LogInformation("Razor view rendering took {ElapsedMilliseconds} ms", renderStopwatch.ElapsedMilliseconds);

            var resendStopwatch = Stopwatch.StartNew();
            var message = new EmailMessage();
            message.From = "support@no-reply.theknight.tech";
            message.To.Add(receipient);
            message.Subject = subject;
            message.HtmlBody = htmlBody;

            await _resend.EmailSendAsync(message);
            resendStopwatch.Stop();
            _logger.LogInformation("Resend email sending took {ElapsedMilliseconds} ms", resendStopwatch.ElapsedMilliseconds);
        }
    }
}
