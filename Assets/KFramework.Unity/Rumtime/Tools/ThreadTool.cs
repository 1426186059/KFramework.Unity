using System.Diagnostics;
using System.Threading;

public static class ThreadTool
{
    // 1. 声明一个静态变量保存主线程ID
    private static int nMainThreadId = int.MaxValue;

    // 2. 提供一个初始化方法，必须在主线程中调用（例如全局单例的 Awake 中）
    public static void SetMainThread(Thread mThread)
    {
        nMainThreadId = mThread.ManagedThreadId;
    }

    // 3. 提供判断方法
    public static bool IsMainThread()
    {
        Debug.Assert(nMainThreadId != int.MaxValue, "Please Call SetMainThread");
        return Thread.CurrentThread.ManagedThreadId == nMainThreadId;
    }

    public static void AssertMainThread(bool bIsMainThread = true)
    {
        UnityEngine.Debug.Assert(IsMainThread() == bIsMainThread);
    }
}
