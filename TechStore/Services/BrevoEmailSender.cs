using SendWithBrevo;

namespace TechStore.Services {
    public class BrevoEmailSender : IEmailSender {
        private readonly IConfiguration configuration;

        public BrevoEmailSender(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage) {
            var apiKey = configuration["Brevo:ApiKey"];
            var senderEmail = configuration["Brevo:SenderEmail"];
            var senderName = configuration["Brevo:SenderName"];

            var client = new BrevoClient(apiKey);

            await client.SendAsync(
                new Sender(senderName, senderEmail),
                new List<Recipient> { new Recipient(toEmail, toEmail) },
                subject,
                htmlMessage,
                true
            );
        }
    }
}
