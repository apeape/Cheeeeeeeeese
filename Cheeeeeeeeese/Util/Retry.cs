using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cheeeeeeeeese.Util
{
    public static class Retry
    {
        public static TResult RetryAction<TResult>(Func<TResult> action, int numRetries, int retryTimeout)
        {
            return RetryAction<TResult>(action, null, numRetries, retryTimeout);
        }
        /// <summary>
        ///  RetryAction( () => SomeFunctionThatCanFail(), ... );
        /// </summary>
        public static TResult RetryAction<TResult>(Func<TResult> action, string actionName, int numRetries, int retryTimeout)
        {
            if (action == null)
                throw new ArgumentNullException("action"); // slightly safer...

            do
            {
                try { return action(); }
                catch (Exception e)
                {
                    if (numRetries <= 0) throw;  // improved to avoid silent failure
                    else
                    {
                        if (actionName != null)
                            Console.WriteLine(actionName + "failed: " + e.Message + ", retrying...");
                        else
                            Console.WriteLine("failed: " + e.Message + ", retrying...");

                        Thread.Sleep(retryTimeout);
                    }
                }
            } while (numRetries-- > 0);

            return default(TResult);
        }
    }
}
