using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetryWithPolly
{
    class Program
    {

        static void Main(string[] args)
        {
            var logger = new Logger();
            var policy = Policy
                            .Handle<TimeoutException>()
                            .WaitAndRetry(Enumerable.Repeat(TimeSpan.FromSeconds(5), 3),
                            (exception, timespan) =>
                            {
                                logger.Warn($"{exception} trying again in {timespan}");
                            });

            PolicyResult result = policy.ExecuteAndCapture(() => TransientOperation());

        }

        static int attempt = 0;
        private static void TransientOperation()
        {
            Console.WriteLine();
            Console.WriteLine("Executing the TransientOperation");
            if (++attempt==3)
            {
                Console.WriteLine("SUCCESS");

                return;
            }
            throw new TimeoutException("Server didnt repond");
        }
    }

    class Logger
    {
        public void Warn(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
