namespace Sample.Test
{
    public class AppointmentSmsNotificationService(string url) : IAppointmentSmsNotificationService
    {
        private readonly string _url = url;

        public async Task SendSms(string delayTime)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(5000, source.Token);
        }

        public async Task SendSms(string delayTime, bool start, int count)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(5000, source.Token);
        }

        public async Task SendSms(TestClass test, string delayTime, DateTime next)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(5000, source.Token);
        }



    }
}
