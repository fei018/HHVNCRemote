using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VncHelperLib
{
    public static class TaskAsync
    {
        public static Task StartNewAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            Task task2 = new Task(() =>{
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            task2.Start();
            return tcs.Task;
        }
    }
}
