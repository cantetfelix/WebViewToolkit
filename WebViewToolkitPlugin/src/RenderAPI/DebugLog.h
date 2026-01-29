#pragma once

#include <windows.h>
#include <stdio.h>
#include <stdarg.h>

namespace WebViewToolkit
{
    class DebugLog
    {
    public:
        static void Log(const char* format, ...)
        {
            char tempPath[MAX_PATH];
            GetTempPathA(MAX_PATH, tempPath);

            char logPath[MAX_PATH];
            sprintf_s(logPath, "%sWebViewToolkit_D3D12_Debug.log", tempPath);

            FILE* file = nullptr;
            fopen_s(&file, logPath, "a");
            if (file)
            {
                // Get timestamp
                SYSTEMTIME st;
                GetLocalTime(&st);
                fprintf(file, "[%02d:%02d:%02d.%03d] ", st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);

                // Write formatted message
                va_list args;
                va_start(args, format);
                vfprintf(file, format, args);
                va_end(args);

                fprintf(file, "\n");
                fclose(file);
            }
        }
    };
}
