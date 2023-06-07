using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(ILogger<NotificationController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> SendSMS([FromForm] string phones, [FromForm] string mes) {

            HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://smsc.ru/sys/send.php");

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                ["phones"] = phones,
                ["mes"] = mes,
                ["login"] = "tech@olima.ru",
                ["psw"] = "testadmin#01020305"
            };
            // создаем объект HttpContent
            HttpContent contentForm = new FormUrlEncodedContent(data);            
            request.Content = contentForm;

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            string requestStr = response.RequestMessage.ToString();
            //var strResponse = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK) return Ok();
            else return BadRequest();
        }
        [HttpPost]
        public async Task<ActionResult> SendEmail([FromForm] string ToEmail, [FromForm] string Caption, 
            [FromForm] string Message, [FromForm] string FromName)
        {
            string FromEmail = "techolima@yandex.ru";
            string FromEmailPassword = "obsyyftzpyachica";

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(FromName, FromEmail));
            emailMessage.To.Add(new MailboxAddress("", ToEmail));
            emailMessage.Subject = Caption;
            //первый вариант добавления содержания с приложениями
            var bodyBuilder = new BodyBuilder();            
            bodyBuilder.HtmlBody = Message;
            emailMessage.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.yandex.ru", 25, false);
                    await client.AuthenticateAsync(FromEmail, FromEmailPassword);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}