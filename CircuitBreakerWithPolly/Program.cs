using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitBreakerWithPolly
{
    class Program
    {
        static void Main(string[] args)
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<TimeoutException>()
                .CircuitBreaker(2, TimeSpan.FromMinutes(1),
                (exception, timespan, context) => { Console.WriteLine("OnBreak"); },
                context => { Console.WriteLine("OnReset"); });

            Console.WriteLine("---- Try 1 ----");
            TryExecutingRiskyOperation(breaker);
            Console.WriteLine("---- Try 2 ----");
            TryExecutingRiskyOperation(breaker);
            Console.WriteLine("---- Try 3 ----");
            TryExecutingRiskyOperation(breaker);
            Console.WriteLine("---- Try 4 ----");
            TryExecutingRiskyOperation(breaker);

        }
        static void TryExecutingRiskyOperation(CircuitBreakerPolicy breaker)
        {
            try
            {
                breaker.Execute(() => { CloudOperation(); });
            }
            catch (Exception)
            {
                Console.WriteLine("TryExecutingRiskyOperation - got exception");
            }
        }
        private static void CloudOperation()
        {
            Console.WriteLine("Executing CloudOperation");
            throw new TimeoutException("Server didnt respond");
        }
    }
}
