using KonserBiletim.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace KonserBiletim.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly IUrlHelper _urlHelper;
        private readonly HttpContext _httpContext;

        public EmailService(IOptions<SmtpSettings> smtpSettings, IUrlHelper urlHelper, IHttpContextAccessor httpContextAccessor)
        {
            _smtpSettings = smtpSettings.Value;
            _urlHelper = urlHelper;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public void SendConfirmationEmail(string email, string confirmationCode)
        {
            var confirmationLink = _urlHelper.Action("ConfirmEmail", "Register", new { code = confirmationCode }, _httpContext.Request.Scheme);
            var smtpClient = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.Email, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Email),
                Subject = "Konser Biletim Kayıt E-postası Onay Kodu",
                Body = $"Lütfen hesabınızı onaylamak için <a href='{confirmationLink}'>buraya tıklayın</a>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            smtpClient.Send(mailMessage);
        }
    }
}
