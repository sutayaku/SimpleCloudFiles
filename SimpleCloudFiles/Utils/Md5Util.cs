using System;
using System.IO;
using System.Security.Cryptography;

namespace SimpleCloudFiles.Utils
{
    public class Md5Util
    {
        /// <summary>
        /// 计算MD5的数据长度
        /// </summary>
        private const int Md5Length = 1024;
        /// <summary>
        /// 最小认定长度
        /// </summary>
        private const int MinLength = 5120;//5KB

        public static string GetMD5(string str)
        {
            var md5 = MD5.Create();
            var fromDta = System.Text.Encoding.UTF8.GetBytes(str);
            var targetData = md5.ComputeHash(fromDta);
            return ByteArrayToHexString(targetData);
            //var byte2String = "";
            //for (int i = 0; i < targetData.Length; i++)
            //{
            //    byte2String += targetData[i].ToString("x2");
            //}
            //return byte2String;
        }

        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="fileName">要计算 MD5 值的文件名和路径</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static string Md5File(string fileName)
        {
            if (!File.Exists(fileName))
                return string.Empty;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > MinLength)
                {
                    var offsets = new int[3];
                    offsets[0] = 0;
                    offsets[1] = (int)Math.Floor((decimal)fs.Length / 2) - 512;
                    offsets[2] = (int)fs.Length - Md5Length - 1;
                    var buffer = new byte[3072];

                    for (var i = 0; i < 3; i++)
                    {
                        fs.Position = offsets[i];

                        fs.Read(buffer, i * Md5Length, Md5Length);
                    }
                    fs.Close();
                    return ByteArrayToHexString(HashData(buffer)).ToLower();
                }
                else
                {
                    var bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, (int)fs.Length);
                    var hashBytes = HashData(bytes);
                    return ByteArrayToHexString(hashBytes).ToLower();
                }
            }
        }

        /// <summary>
        /// 计算哈希值
        /// </summary>
        /// <param name="bytes">要计算哈希值的 Stream</param>
        /// <returns>哈希值字节数组</returns>
        private static byte[] HashData(byte[] bytes)
        {
            HashAlgorithm algorithm = MD5.Create();
            return algorithm.ComputeHash(bytes);
        }

        /// <summary>
        /// 字节数组转换为16进制表示的字符串
        /// </summary>
        private static string ByteArrayToHexString(byte[] buf)
        {
            return BitConverter.ToString(buf).Replace("-", "");
        }
    }
}
