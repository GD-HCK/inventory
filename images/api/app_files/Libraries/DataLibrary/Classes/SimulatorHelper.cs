using DataLibrary.Interfaces;
using System.Diagnostics;

namespace DataLibrary.Classes
{
    public class SimulatorHelper : ISimulatorHelper
    {
        public Dictionary<string, string> SimulateRandomHttpResponse()
        {
            Random random = new();
            // Generate a random HTTP status code between 100 and 599
            int code = random.Next(200, 600);

            // Ensure the code is not 204 (No Content) or 304 (Not Modified) as they do not return a body
            while (code == 204 || code == 304)
            {
                code = random.Next(100, 600);
            }

            if (code >= 500)
            {
                // In your controller or service method:
                try
                {
                    throw new Exception($"Simulated server error with status code {code}");
                }
                catch (Exception ex)
                {
                    var activity = Activity.Current;
                    if (activity != null)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                        activity.SetTag("exception.type", ex.GetType().ToString());
                        activity.SetTag("exception.message", ex.Message);
                        activity.SetTag("exception.stacktrace", ex.StackTrace);
                    }
                    throw; // rethrow to preserve error status and HTTP 500
                }
            }

            // Simulate a response based on the provided code
            return new Dictionary<string, string>
            {
                { "code", code.ToString() },
                { "response", $"Simulated response for status code {code}" }
            };
        }
    }
}
