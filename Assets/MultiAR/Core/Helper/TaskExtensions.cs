using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiAR.Core.Helper
{
    public static class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, TimeSpan duration)
        {
            try
            {
                if (task == await Task.WhenAny(task, Task.Delay(duration)))
                {
                    Debug.Log("Task completed before timeout!");
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured: {e.Message}");
                throw;
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan duration)
        {
            try
            {
                if (task == await Task.WhenAny(task, Task.Delay(duration)))
                {
                    return task.Result;
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured: {e}");
                throw;
            }
        }
    }
}
