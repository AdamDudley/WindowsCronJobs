using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace WindowsServerCronTask
{
 
    class Program
    {
        private static void OutputUsage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine(string.Format("\t windowscron url [{0}] [{1}]", ArgSettings.CountArgument, ArgSettings.IntervalArgument));
            Console.WriteLine("where");
            Console.WriteLine("\t url is the url you would like to GET from");
            Console.WriteLine(string.Format("\t {0} is the count of times to perform the request (default {0}1)", ArgSettings.CountArgument));
            Console.WriteLine(string.Format("\t {0} is the interval in miliseconds betweeen requests (default {0}500)", ArgSettings.IntervalArgument));
            Console.WriteLine();
            Console.WriteLine("example:");
            Console.WriteLine(string.Format("\t windowcron https://www.bing.com {0}5 {1}2000", ArgSettings.CountArgument, ArgSettings.IntervalArgument));
            Console.WriteLine("\t will visit www.bing.com 5 times with a 2000ms (2 second) gap between requests");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            //Check we have some args, or user may be asking for help
            if (args.Length == 0 || args[0].Trim().StartsWith("/?"))
            {
                OutputUsage();
                return;
            }

            //Create our app settings
            ArgSettings settings;
            try
            {
                settings = new ArgSettings(args);
            } 
            catch (ArgumentException ae)
            {
                //Inform user of error
                Console.WriteLine("Error: ");
                Console.WriteLine("\t" + ae.Message);
                
                //Inform user of usage
                OutputUsage();

                //Exit
                return;
            }

            //It would be nice to cancel this operation when a key is pressed
            CancellationTokenSource cts = new CancellationTokenSource();

            //Start the requests
            Task t = MakeRequests(settings.Url, settings.Count, settings.Interval, 60000, cts.Token);
            
            //Start a task to read a key
            Task tReader = Task.Factory.StartNew(() => Console.ReadKey());

            //Wait for either of these tasks to finish prior to exiting the app
            Task[] waitForThese = { t, tReader };
            Task.WaitAny(waitForThese);
            cts.Cancel();
        }


        /// <summary>
        /// Makes the requests.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="count">The count.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="timeOut">The time out.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        async static Task MakeRequests(string url, int count, int interval, int timeOut, CancellationToken ct)
        {

            for (int i = 1; i <= count; i++)
            {
                //have we been cancelled?
                if (ct.IsCancellationRequested) return;

                //A separate cancellation source for our requests
                CancellationTokenSource requestCts = new CancellationTokenSource();

                //If our cancellation token can be cancelled, then we should cancel our requests if it is cancelled
                if (ct.CanBeCanceled)
                {
                    ct.Register(new Action(() => { requestCts.Cancel(); }));
                }

                //Store our content length from each request
                long contentLength = 0;

                //Show a message that we are waiting for the interval
                if (i != 1 && interval > 0)
                {
                    Console.WriteLine(string.Format("Waiting for {0} ms", interval));
                    await Task.Delay(interval, requestCts.Token);
                }

                //Show a message we are proceeding with the request
                Console.WriteLine(string.Format("Request {0} of {1} to {2}", i, count, url));
                
                try
                {
                    //Start our request and wait for it to finish
                    Task<long> urlRequestTask = RequestUrlAsync(url, requestCts.Token);

                    //Wait until timeout, otherwise cancel the task
                    if (await Task.WhenAny(Task.Delay(timeOut, requestCts.Token), urlRequestTask) == urlRequestTask)
                    {
                        //Get the value of the task (or any exceptions)
                        contentLength = await urlRequestTask;
                        Console.WriteLine(string.Format("Response was {0} bytes long", contentLength));
                        Console.WriteLine();
                    }
                    else
                    {
                        //We didn't get our response in the given time. cancel the operation
                        requestCts.Cancel();
                        Console.WriteLine(string.Format("Failed to receive a response within the given timout of {0}", timeOut));
                    }
                }
                catch (AggregateException ae)
                {
                    foreach (Exception e in ae.InnerExceptions)
                    {
                        Console.WriteLine("EXCEPTION:");
                        Console.WriteLine(string.Format("\t {0}", e.Message));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("EXCEPTION:");
                    Console.WriteLine(string.Format("\t {0}", e.Message));
                }
            }
        }

        /// <summary>
        /// Task to request a url, and return the length of the content
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        async static Task<long> RequestUrlAsync(string url, CancellationToken cts)
        {
            //Create our http client and get the bytes from the request
            HttpClient client = new HttpClient();

            //await Task.Delay(10000, cts);
            Task<HttpResponseMessage> getBytesTask = client.GetAsync(url, cts);
            
            //Inform user we are awaiting a response
            Console.WriteLine("Awaiting response");


            //Await the response
            Byte[] bytes = await getBytesTask.Result.Content.ReadAsByteArrayAsync();
            
            //Return the length
            return bytes.LongLength;
        }
    }
}
