using System;
using System.ServiceProcess;
using System.Windows.Forms; // Đảm bảo đã thêm Reference tới System.Windows.Forms.dll

namespace SdvnAutoEmail
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Kiểm tra xem ứng dụng đang chạy ở chế độ tương tác (console) hay là Service
            if (Environment.UserInteractive)
            {
                // Chế độ Console: Chạy Service như một ứng dụng Console để dễ debug
                using (var service = new Service1())
                {
                    Console.WriteLine("SdvnAutoEmail Service running in interactive mode.");
                    Console.WriteLine("Press 'Enter' to stop the service.");

                    // Gọi phương thức debug_OnStart() public mới
                    service.Debug_OnStart(null);

                    // Chờ người dùng nhấn Enter để dừng
                    Console.ReadLine();

                    // Gọi phương thức debug_OnStop() public mới
                    service.Debug_OnStop();

                    Console.WriteLine("SdvnAutoEmail Service stopped.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                }
            }
            else
            {
                // Chế độ Service: Chạy như một Windows Service thông thường
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service1()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}