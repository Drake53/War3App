using System;

namespace War3App.MapAdapter.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetTypeAndMessage(this Exception e)
        {
            return $"{e.GetType().FullName}: {e.Message}";
        }
    }
}