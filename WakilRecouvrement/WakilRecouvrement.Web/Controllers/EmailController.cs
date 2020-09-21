using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using WakilRecouvrement.Web.Models;

namespace WakilRecouvrement.Web.Controllers
{
    public class EmailController : ApiController
    {

        public IHttpActionResult sendmail(Email e)
        {

            string subject = e.Subject;
            string body = e.Body;
            string to = e.To;
            Debug.WriteLine("aaaaa");
            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("alwakilrecouvrementmailtest@gmail.com");
            mm.To.Add(to);
            mm.Subject = subject;
            mm.Body = body;
            mm.IsBodyHtml = false;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.UseDefaultCredentials = true;
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("alwakilrecouvrementmailtest@gmail.com", "wakil123456");
            smtp.Send(mm);

            return Ok();

        }
    }
}
