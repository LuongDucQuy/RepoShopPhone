using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;

namespace projectShopLaptop.Mail
{
    public static class SendMail
    {
        public static bool SendEmail(string to, string subject, string body, string attachFile)
        {
            try
            {
                MailMessage message = new MailMessage(ConstantHelper.emailSender, to, subject, body);
                message.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient(ConstantHelper.host, ConstantHelper.port))
                {
                    smtpClient.EnableSsl = true;
                    if (!string.IsNullOrEmpty(attachFile))
                    {
                        Attachment attachment = new Attachment(attachFile);
                        message.Attachments.Add(attachment);
                    }

                    NetworkCredential credentials = new NetworkCredential(ConstantHelper.emailSender, ConstantHelper.passwordSender);
                    smtpClient.Credentials = credentials;
                    smtpClient.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
                return false;
            }
            return true;
        }
    }
}