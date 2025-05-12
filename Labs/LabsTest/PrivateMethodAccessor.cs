using System.Reflection;

namespace LabsTest;

public static class PrivateMethodAccessor
{
    public static object InvokePrivateMethod<T>(
        T instance,
        string methodName,
        params object[] parameters
    )
    {
        var methodInfo = typeof(T).GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        if (methodInfo == null)
        {
            throw new ArgumentException($"Method {methodName} not found");
        }

        return methodInfo.Invoke(instance, parameters);
    }

    public static TResult InvokePrivateMethod<T, TResult>(
        T instance,
        string methodName,
        params object[] parameters
    )
    {
        return (TResult)InvokePrivateMethod(instance, methodName, parameters);
    }
}
