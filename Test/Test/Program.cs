using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Test
{
    class Program
    {
        public static string getDownloadPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CppPlugin", "download");
        }
        static void Main(string[] args)
        {
            string qid = Console.ReadLine();
            string path1_2 = Path.Combine(getDownloadPath(), "36");
            string str2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Output", qid, "unzipFile");
            string str3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Output", qid, "zipFile");
            try
            {
                string filePath = Path.Combine(path1_2, qid);
                string str4 = Path.Combine(str3, qid + ".zip");
                byte[] content = EncryptUtil.decrypt(FileUtil.readFileInBytes(filePath));
                FileUtil.saveFileWithByte(str4, content);
                Console.WriteLine(str4);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sabi");
            }
        }
    }
    class EncryptUtil
    {
        private static string aeskey = "hYOTz5Il8IzWQSVk";

        private static byte[] generateIV()
        {
            char[] charArray = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder();
            for (int index1 = 0; index1 < 16; ++index1)
            {
                int index2 = random.Next(charArray.Length);
                stringBuilder.Append(charArray[index2]);
            }
            return Encoding.Default.GetBytes(stringBuilder.ToString());
        }

        public static byte[] decrypt(byte[] input)
        {
            byte[] numArray1 = Convert.FromBase64String(Encoding.UTF8.GetString(input));
            AesManaged aesManaged = new AesManaged();
            aesManaged.Mode = CipherMode.CBC;
            aesManaged.Padding = PaddingMode.PKCS7;
            aesManaged.Key = Encoding.UTF8.GetBytes(EncryptUtil.aeskey);
            byte[] numArray2 = new byte[16];
            Array.Copy((Array)numArray1, 0, (Array)numArray2, 0, 16);
            aesManaged.IV = numArray2;
            byte[] buffer1 = new byte[numArray1.Length - 16];
            Array.Copy((Array)numArray1, 16, (Array)buffer1, 0, numArray1.Length - 16);
            CryptoStream cryptoStream = new CryptoStream((Stream)new MemoryStream(buffer1), aesManaged.CreateDecryptor(), CryptoStreamMode.Read);
            byte[] buffer2 = new byte[buffer1.Length];
            cryptoStream.Read(buffer2, 0, buffer2.Length);
            cryptoStream.Close();
            return buffer2;
        }

        public static byte[] encrypt(byte[] input)
        {
            AesManaged aesManaged = new AesManaged();
            aesManaged.Mode = CipherMode.CBC;
            aesManaged.Padding = PaddingMode.PKCS7;
            aesManaged.Key = Encoding.UTF8.GetBytes(EncryptUtil.aeskey);
            byte[] iv = EncryptUtil.generateIV();
            aesManaged.IV = iv;
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, aesManaged.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(input, 0, input.Length);
            cryptoStream.FlushFinalBlock();
            byte[] array = memoryStream.ToArray();
            Console.WriteLine(string.Join<byte>(",", (IEnumerable<byte>)array));
            cryptoStream.Close();
            byte[] inArray = new byte[16 + array.Length];
            Array.Copy((Array)iv, 0, (Array)inArray, 0, 16);
            Array.Copy((Array)array, 0, (Array)inArray, 16, array.Length);
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(inArray));
        }
    }
    public class FileUtil
    {
        private const int BUFFER_SIZE = 1024;

        public static byte[] readFileInBytes(string filePath)
        {
            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                return new byte[0];
            }
        }

        public static string readFileInString(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static byte[] readInputStreamIntoBytes(Stream inputStream)
        {
            long num1 = 0;
            if (inputStream.CanSeek)
            {
                num1 = inputStream.Position;
                inputStream.Position = 0L;
            }
            try
            {
                IList<byte[]> numArrayList = (IList<byte[]>)new List<byte[]>();
                int count = 0;
                byte[] buffer = new byte[1024];
                int num2;
                while ((num2 = inputStream.Read(buffer, 0, 1024)) > 0)
                {
                    count += num2;
                    numArrayList.Add(buffer);
                }
                byte[] numArray = new byte[count];
                int index;
                for (index = 0; index < numArrayList.Count - 1; ++index)
                    Buffer.BlockCopy((Array)numArrayList[index], 0, (Array)numArray, index * 1024, numArrayList[index].Length);
                Buffer.BlockCopy((Array)numArrayList[index], 0, (Array)numArray, index * 1024, count);
                return numArray;
            }
            finally
            {
                if (inputStream.CanSeek)
                    inputStream.Position = num1;
            }
        }

        public static bool saveFileWithByte(string saveFilePath, byte[] content)
        {
            try
            {
                FileUtil.CreateDirectoryWithFilePath(saveFilePath);
                File.WriteAllBytes(saveFilePath, content);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool saveFileWithString(string saveFilePath, string content)
        {
            try
            {
                FileUtil.CreateDirectoryWithFilePath(saveFilePath);
                File.WriteAllText(saveFilePath, content);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool saveFile(string saveFilePath, Stream inputStream)
        {
            long num = 0;
            if (inputStream.CanSeek)
            {
                num = inputStream.Position;
                inputStream.Position = 0L;
            }
            try
            {
                FileUtil.CreateDirectoryWithFilePath(saveFilePath);
                FileStream fileStream = new FileStream(saveFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                byte[] buffer = new byte[1024];
                int count;
                while ((count = inputStream.Read(buffer, 0, 1024)) > 0)
                    fileStream.Write(buffer, 0, count);
                fileStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (inputStream.CanSeek)
                    inputStream.Position = num;
            }
        }

        public static bool delAllFile(string path)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    directoryInfo.Delete(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public static bool delAllFileWithEX(string path, List<string> exceptions)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                        directory.Delete(true);
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        if (!exceptions.Contains(file.Name))
                            file.Delete();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public static bool copy(string src, string dst)
        {
            try
            {
                FileUtil.CreateDirectoryWithFilePath(dst);
                File.Copy(src, dst);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool copyDir(string src, string dst)
        {
            if (!Directory.Exists(dst))
                Directory.CreateDirectory(dst);
            if (!Directory.Exists(src))
                return false;
            DirectoryInfo directoryInfo = new DirectoryInfo(src);
            foreach (FileInfo file in directoryInfo.GetFiles())
                FileUtil.copy(file.FullName, Path.Combine(dst, file.Name));
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                FileUtil.copyDir(directory.FullName, Path.Combine(dst, directory.Name));
            return true;
        }

        private static void CreateDirectoryWithFilePath(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Directory.Exists)
                return;
            fileInfo.Directory.Create();
        }
    }
}
