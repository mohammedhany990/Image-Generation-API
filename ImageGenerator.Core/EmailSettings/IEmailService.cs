using ImageGenerator.API.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGenerator.Core.EmailSettings
{
    public interface IEmailService
    {
        public void SendEmail(Email email);
    }
}
