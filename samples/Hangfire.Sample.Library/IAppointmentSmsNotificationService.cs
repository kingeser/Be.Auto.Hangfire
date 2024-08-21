using System;
using System.Threading.Tasks;

namespace Hangfire.Sample.Library
{
    public interface IAppointmentSmsNotificationService
    {
        Task SendSms(string delayTime);
        Task SendSms(string delayTime, bool start, int count);
        Task SendSms(TestClass test, string delayTime, DateTime next);
    }
}