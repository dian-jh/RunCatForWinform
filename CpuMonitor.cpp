#include <windows.h>

// 宏定义，方便导出函数
#define EXPORT extern "C" __declspec(dllexport)

// 定义一个简单的结构体来传输数据到 C#
struct CPUData {
    float Total;
    float User;
    float Kernel;
    float Idle;
};

// 全局变量保存“上一次”的时间 (单例模式)
//FILETIME 结构体定义在 Windows.h 里，64位的时间戳
static FILETIME prevIdleTime = { 0 };
static FILETIME prevKernelTime = { 0 };
static FILETIME prevUserTime = { 0 };

// 辅助函数：将 FILETIME 转为 unsigned long long 进行数学运算
/*
typedef union _ULARGE_INTEGER {
    struct {
        DWORD LowPart;
        DWORD HighPart;
    };
    ULONGLONG QuadPart;
} ULARGE_INTEGER;
联合体类型，共享一片内存地址，也就是共享64位的空间，使用不同的方式访问QuadPart其实就是一次性访问这个64位空间存储的值
*/
unsigned long long FT2ULL(FILETIME ft) {
    ULARGE_INTEGER uli;
    uli.LowPart = ft.dwLowDateTime;
    uli.HighPart = ft.dwHighDateTime;
    return uli.QuadPart;
}

// 初始化函数 (C# 构造函数里调用)
EXPORT void InitCpuMonitor() {
    GetSystemTimes(&prevIdleTime, &prevKernelTime, &prevUserTime);
}

// 获取核心数据的函数 (C# Update 里调用)
// 参数是一个指向结构体的指针，C# 会传一个引用过来让我们填值
EXPORT bool GetCpuUsage(CPUData* data) {
    FILETIME idle, kernel, user;
    if (!GetSystemTimes(&idle, &kernel, &user)) {
        return false;
    }

    // 1. 转为整数
    unsigned long long curIdle = FT2ULL(idle);
    unsigned long long curKernel = FT2ULL(kernel);
    unsigned long long curUser = FT2ULL(user);

    unsigned long long preIdle = FT2ULL(prevIdleTime);
    unsigned long long preKernel = FT2ULL(prevKernelTime);
    unsigned long long preUser = FT2ULL(prevUserTime);

    // 2. 计算差值 (Delta)
    unsigned long long deltaIdle = curIdle - preIdle;
    unsigned long long deltaKernel = curKernel - preKernel;
    unsigned long long deltaUser = curUser - preUser;

    // 3. 核心公式
    // Windows 的 KernelTime 包含了 IdleTime，所以总时间是 Kernel + User
    unsigned long long totalSystem = deltaKernel + deltaUser;
    
    // 真正的内核忙碌时间 = Kernel - Idle
    unsigned long long kernelBusy = deltaKernel - deltaIdle;

    // 防止除以0
    if (totalSystem == 0) totalSystem = 1; 

    // 4. 填入数据 (0-100)
    data->Total = (float)(totalSystem - deltaIdle) * 100.0f / totalSystem;
    data->User = (float)deltaUser * 100.0f / totalSystem;
    data->Kernel = (float)kernelBusy * 100.0f / totalSystem;
    data->Idle = (float)deltaIdle * 100.0f / totalSystem;

    // 5. 更新状态
    prevIdleTime = idle;
    prevKernelTime = kernel;
    prevUserTime = user;

    return true;
}