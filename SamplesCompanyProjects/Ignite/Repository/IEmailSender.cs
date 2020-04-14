using System.Threading.Tasks;

namespace Ignite.Areas.Office.Repository
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
