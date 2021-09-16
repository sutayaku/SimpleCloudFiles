using SimpleCloudFiles.Dtos;
using SimpleCloudFiles.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleCloudFiles.Utils
{
    public class FileOperation
    {
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="sFile">源文件对象</param>
        public static void SaveChunk(SourceFile sFile)
        {
            if (!ExistsChunk(sFile.Md5, sFile.Size, sFile.Chunk))
            {
                var dir1 = sFile.Md5.Substring(28, 2);
                var dir2 = sFile.Md5.Substring(30, 2);
                var dir3 = $"{sFile.Md5}_{sFile.Size}";
                var filePath = Path.Combine(CfCfg.SourceFileRoot, dir1, dir2);
                CreateFileDir(filePath, sFile.Md5, sFile.Size);
                filePath = Path.Combine(filePath, dir3, dir3);
                CreateDirectory(filePath);

                var fileName = sFile.Md5 + "_" + sFile.Chunk;
                var tempFileName = "temp_" + sFile.Chunk;
                try
                {
                    var tempFilePath = Path.Combine(filePath, tempFileName);
                    var saveFilePath = Path.Combine(filePath, fileName);
                    File.WriteAllBytes(tempFilePath, sFile.ChunkData);
                    var fileInfo = new FileInfo(tempFilePath);//重命名操作是为了防止创建文件但还没写完内容时就进行了合并操作
                    fileInfo.MoveTo(saveFilePath);
                }
                catch (Exception e)
                {
                    var exception = new CfException(e.Message, e) { Code = CfExceptionCodeEnum.CreateFileFail };
                    throw exception;
                }
            }

            if (CheckUploadFinish(sFile.Md5, sFile.Size, sFile.Chunks))
            {
                try
                {
                    CombineChunk(sFile.Md5, sFile.Ext, sFile.Size, sFile.Chunks);
                }
                catch (Exception e)
                {
                    var exception = new CfException(e.Message, e) { Code = CfExceptionCodeEnum.CreateFileFail };
                    throw exception;
                }
            }
        }

        /// <summary>
        /// 分块是否已存在
        /// </summary>
        /// <param name="md5">客户端传入的源文件md5</param>
        /// <param name="size">客户端传入的源文件大小</param>
        /// <param name="chunk">客户端传入的源文件分块序列</param>
        /// <returns></returns>
        public static bool ExistsChunk(string md5, long size, uint chunk)
        {
            var fileName = md5 + "_" + chunk;
            var dir1 = md5.Substring(28, 2);
            var dir2 = md5.Substring(30, 2);
            var dir3 = $"{md5}_{size}";
            var filePath = Path.Combine(CfCfg.SourceFileRoot, dir1, dir2, dir3, dir3, fileName);
            var tempPath = Path.Combine(CfCfg.SourceFileRoot, dir1, dir2, dir3, dir3, "temp_" + chunk);
            return File.Exists(filePath) || File.Exists(tempPath);
        }

        /// <summary>
        /// 是否存在源文件
        /// </summary>
        /// <param name="md5"></param>
        /// <param name="ext"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool ExistsSourceFile(string md5, string ext, long size)
        {
            var filePath = GetSourceFilePath(md5, ext, size);
            return File.Exists(filePath);
        }

        /// <summary>
        /// 获取源文件的路径
        /// </summary>
        /// <param name="md5">客户端传入的源文件md5</param>
        /// <param name="ext">文件扩展名</param>
        /// <param name="size">客户端传入的源文件大小</param>
        /// <returns></returns>
        public static string GetSourceFilePath(string md5, string ext, long size)
        {
            var fileName = md5 + "_" + size + "." + ext;
            var dir1 = md5.Substring(28, 2);
            var dir2 = md5.Substring(30, 2);
            var savePath = Path.Combine(CfCfg.SourceFileRoot, dir1, dir2, $"{md5}_{size}", fileName);
            return savePath;
        }

        /// <summary>
        /// 获取文件存储路径
        /// </summary>
        /// <param name="md5">文件md5</param>
        /// <param name="ext"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetFileSavePath(string md5, string ext, long size)
        {
            var fileName = md5 + "_" + size + "." + ext;
            var dir1 = md5.Substring(28, 2);
            var dir2 = md5.Substring(30, 2);
            var savePath = Path.Combine(CfCfg.SaveFileRoot, dir1, dir2, $"{md5}_{size}", fileName);
            return savePath;
        }

        /// <summary>
        /// 源文件转存至正式目录
        /// </summary>
        /// <param name="md5">客户端传入的源文件md5</param>
        /// <param name="ext">文件扩展名</param>
        /// <param name="size">客户端传入的源文件大小</param>
        public static void SaveToRoot(string md5, string ext, long size)
        {
            var sourceFilePath = GetSourceFilePath(md5, ext, size);

            var fileMd5 = Md5Util.Md5File(sourceFilePath);

            if (fileMd5.Length < 32)
            {
                throw new Exception($"fileMd5 生成错误，SourceFilePath:{sourceFilePath}, md5:{md5}, ext:{ext}, size:{size}");
            }

            var dir1 = fileMd5.Substring(28, 2);
            var dir2 = fileMd5.Substring(30, 2);
            var filePath = Path.Combine(CfCfg.SaveFileRoot, dir1, dir2);
            CreateFileDir(filePath, fileMd5, size);
            var fileName = fileMd5 + "_" + size;
            filePath = Path.Combine(filePath, fileName, $"{fileName}.{ext}");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            try
            {
                File.Copy(sourceFilePath, filePath, true);
            }
            catch (Exception e)
            {
                var exception = new CfException(e.Message, e) { Code = CfExceptionCodeEnum.CreateFileFail };
                throw exception;
            }

            if (!CfCfg.SaveSourceFile)
            {
                File.Delete(sourceFilePath);
            }
        }

        /// <summary>
        /// 解压压缩文件到正式目录
        /// </summary>
        /// <param name="md5">客户端传入的源文件md5</param>
        /// <param name="size">客户端传入的源文件大小</param>
        public static void UnZipToRoot(string md5, long size)
        {
            var sourceFilePath = GetSourceFilePath(md5, "zip", size);

            var fileMd5 = Md5Util.Md5File(sourceFilePath);

            var dir1 = fileMd5.Substring(28, 2);
            var dir2 = fileMd5.Substring(30, 2);
            var dir3 = $"{md5}_{size}";
            var filePath = Path.Combine(CfCfg.SaveFileRoot, dir1, dir2);
            CreateFileDir(filePath, md5, size);
            filePath = Path.Combine(filePath, dir3, dir3);
            CreateDirectory(filePath);
            try
            {
                ZipFile.ExtractToDirectory(sourceFilePath, filePath);
            }
            catch (Exception e)
            {
                var exception = new CfException(e.Message, e) { Code = CfExceptionCodeEnum.CreateFileFail };
                throw exception;
            }
        }

        /// <summary>
        /// 获取压缩文件内部文件结构
        /// </summary>
        /// <param name="md5">服务端计算出的的文件大小</param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static List<CfNode> GetZipInfo(string md5, long size)
        {
            var list = new List<CfNode>();

            var dir1 = md5.Substring(28, 2);
            var dir2 = md5.Substring(30, 2);
            var dir3 = $"{md5}_{size}";

            var dirPath = Path.Combine(CfCfg.SaveFileRoot, dir1, dir2, dir3, dir3);
            if (!Directory.Exists(dirPath))
            {
                return list;
            }

            Action<DirectoryInfo, CfNode> build = null;
            build = (dirInfo, parentNode) =>
            {
                var dirList = dirInfo.GetDirectories();
                foreach (var dir in dirList)
                {
                    var dirNode = new CfNode { Name = dir.Name, Type = "dir" };
                    build(dir, dirNode);
                    if (parentNode == null)
                    {
                        list.Add(dirNode);
                    }
                    else
                    {
                        parentNode.Children.Add(dirNode);
                    }
                }
                var fileList = dirInfo.GetFiles();
                foreach (var file in fileList)
                {
                    var fileNode = new CfNode { Name = file.Name, Type = "file" };
                    if (parentNode == null)
                    {
                        list.Add(fileNode);
                    }
                    else
                    {
                        parentNode.Children.Add(fileNode);
                    }
                }
            };
            build(new DirectoryInfo(dirPath), null);

            return list;
        }

        /// <summary>
        /// 创建文件所属目录（当前分区不够创建虚拟链接）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="md5"></param>
        /// <param name="size"></param>
        public static void CreateFileDir(string path, string md5, long size)
        {
            var savePath = Path.Combine(path, md5 + "_" + size);
            if (Directory.Exists(savePath))
            {
                return;
            }
            CreateDirectory(savePath);
        }

        /// <summary>
        /// 检测是否已经上传完成
        /// </summary>
        /// <param name="md5">客户端传入的源文件md5</param>
        /// <param name="size">客户端传入的源文件大小</param>
        /// <param name="chunks">chunk总数</param>
        /// <returns></returns>
        private static bool CheckUploadFinish(string md5, long size, uint chunks)
        {
            var dir1 = md5.Substring(28, 2);
            var dir2 = md5.Substring(30, 2);
            var dir3 = $"{md5}_{size}";
            var savePath = Path.Combine(CfCfg.SourceFileRoot, dir1, dir2, dir3, dir3);
            var files = Directory.GetFiles(savePath);
            var count = 0;
            //计算所有chunk的大小和文件大小进行对比来确认
            long chunkSize = 0;
            foreach (var filePath in files)
            {
                var f = new FileInfo(filePath);
                if (f.Name.Contains("temp"))
                {
                    continue;
                }
                count++;
                chunkSize += f.Length;
            }
            return chunkSize >= size && count >= chunks;
        }

        /// <summary>
        /// 检测目录是否存在，不存在则创建
        /// </summary>
        /// <param name="path">目录路径</param>
        private static void CreateDirectory(string path)
        {
            try
            {
                path = path.Replace("\\\\", "/").Replace('\\', '/');
                var dirs = path.Split('/');
                var newPath = dirs[0];

                for (var i = 0; i < dirs.Length; i++)
                {
                    newPath = Path.Combine(newPath, dirs[1]);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            catch (Exception e)
            {
                var exception = new CfException(e.Message, e) { Code = CfExceptionCodeEnum.CreateDirectoryFail };
                throw exception;
            }

        }

        /// <summary>
        /// 合并文件块
        /// </summary>
        /// <param name="md5">文件md5</param>
        /// <param name="ext">文件扩展名</param>
        /// <param name="size">文件大小</param>
        /// <param name="chunks">文件分块总数</param>
        private static void CombineChunk(string md5, string ext, long size, uint chunks)
        {
            if (chunks == 0)
            {
                chunks = 1;
            }
            var fileName = md5 + "_" + size + "." + ext;
            var dir1 = md5.Substring(28, 2);
            var dir2 = md5.Substring(30, 2);
            var dir3 = $"{md5}_{size}";
            var savePath = Path.Combine(CfCfg.SourceFileRoot, dir1, dir2, dir3);
            var chunksPath = Path.Combine(savePath, dir3);
            var sourceFilePath = Path.Combine(savePath, fileName);
            if (File.Exists(sourceFilePath))
            {
                File.Delete(sourceFilePath);
            }
            using (var fs = File.OpenWrite(sourceFilePath))
            {
                for (int i = 0; i < chunks; i++)
                {
                    var chunkName = md5 + "_" + i;
                    var chunkPath = Path.Combine(chunksPath, chunkName);
                    var bytes = File.ReadAllBytes(chunkPath);
                    fs.Write(bytes, 0, bytes.Length);
                }
                fs.Close();
                fs.Dispose();
            }
            //删除文件块缓存目录
            Directory.Delete(chunksPath, true);
        }
    }
}
