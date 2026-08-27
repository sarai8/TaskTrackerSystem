using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTrackerSystem.Application.Services
{
    internal interface EmailService
    {      
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
