using SimpleCloudFiles.Dtos;
using SimpleCloudFiles.Exceptions;
using SimpleCloudFiles.Models;
using SimpleCloudFiles.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SimpleCloudFiles.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : BaseController
    {
        private readonly CfDbContext _db;
        private readonly ILogger<FileController> _iLogger;
        public FileController(ILogger<FileController> iLogger, CfDbContext db)
        {
            _iLogger = iLogger;
            _db = db;
        }

        [HttpOptions("Upload")]
        public void OptionsUpload() { }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost("Upload")]
        [ProducesResponseType(500, Type = typeof(ApiResult))]
        [ProducesResponseType(200, Type = typeof(ApiResult))]
        public async Task<IActionResult> PostUpload()
        {
            var result = new ApiResult();
            try
            {
                var form = Request.Form;
                var files = form.Files;
                var input = new UploadInput
                {
                    DirId = form["dirId"],
                    Md5 = form["identifier"],
                    Chunk = Convert.ToUInt32(form["chunkNumber"]) - 1,
                    Chunks = Convert.ToUInt32(form["totalChunks"]),
                    Name = form["filename"],
                    Size = Convert.ToInt64(form["totalSize"])
                };

                var index = input.Name.LastIndexOf(".");
                input.Ext = input.Name.Substring(index + 1);

                var sf = new SourceFile
                {
                    Name = input.Name,
                    Ext = input.Ext,
                    Md5 = input.Md5,
                    Size = input.Size,
                    Chunk = input.Chunk,
                    Chunks = input.Chunks
                };

                #region 写入文件块到源文件目录，所有块完毕后自动合并

                if (files.Count == 0)
                {
                    result.Code = 2;
                    return StatusCode(500, result);
                }

                foreach (var item in files)
                {
                    if (item.FileName != null)
                    {
                        var stream = item.OpenReadStream();
                        using (var br = new BinaryReader(stream))
                        {
                            sf.ChunkData = br.ReadBytes((int)stream.Length);
                            try
                            {
                                FileOperation.SaveChunk(sf);
                            }
                            catch (CfException e)
                            {
                                if (e.Code == CfExceptionCodeEnum.CreateFileFail)
                                {
                                    result.Code = 4;//写入文件失败
                                }
                                else if (e.Code == CfExceptionCodeEnum.CreateDirectoryFail)
                                {
                                    result.Code = 3;//创建目录失败
                                }
                                else
                                {
                                    result.Code = 99;
                                }
                                result.Msg = Guid.NewGuid().GetHashCode().ToString();
                                _iLogger.LogError("error code [" + result.Msg + "]");
                                _iLogger.LogError(e, e.Message);
                                return StatusCode(500, result);
                            }
                            finally
                            {
                                br.Close();
                                br.Dispose();
                                stream.Close();
                                stream.Dispose();
                            }
                        }
                    }
                }

                #endregion

                //已经上传完了，计算md5，存入数据
                if (FileOperation.ExistsSourceFile(sf.Md5, sf.Ext, sf.Size))
                {
                    #region 转存到正式目录

                    try
                    {
                        FileOperation.SaveToRoot(sf.Md5, sf.Ext, sf.Size);
                    }
                    catch (CfException e)
                    {
                        if (e.Code == CfExceptionCodeEnum.CreateFileFail)
                        {
                            result.Code = 4; //写入文件失败
                        }
                        else if (e.Code == CfExceptionCodeEnum.CreateDirectoryFail)
                        {
                            result.Code = 3; //创建目录失败
                        }
                        else
                        {
                            result.Code = 99;
                        }
                        result.Msg = Guid.NewGuid().GetHashCode().ToString();
                        _iLogger.LogError("error code [" + result.Msg + "]");
                        _iLogger.LogError(e, e.Message);
                        return StatusCode(500, result);
                    }

                    #endregion

                    #region 记录源文件信息到数据库

                    var filePath = FileOperation.GetSourceFilePath(sf.Md5, sf.Ext, sf.Size);
                    var md5 = Md5Util.Md5File(filePath);
                    var file = new CfFile()
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        DirId = input.DirId,
                        AccountId = AccountId,
                        SourceMd5 = input.Md5,
                        FileName = input.Name,
                        Md5 = md5,
                        Size = input.Size,
                        Ext = input.Ext,
                        CreateTime = DateTime.Now
                    };

                    if (!await _db.Dirs.AnyAsync(a=>a.Id == input.DirId && a.AccountId == AccountId))
					{
                        var dir = _db.Dirs.FirstOrDefault(a => string.IsNullOrEmpty(a.Name) && a.AccountId == AccountId);
                        file.DirId = dir.Id;
					}

                    await _db.CfFiles.AddAsync(file);
                    await _db.SaveChangesAsync();
                    #endregion

                    //上传结束后给客户端
                    result.Data = new
                    {
                        file.Id,
                        file.Md5,
                        file.Size
                    };
                }
            }
            catch (Exception e)
            {
                result.Code = 99;
                result.Msg = Guid.NewGuid().GetHashCode().ToString();
                _iLogger.LogError("error code [" + result.Msg + "]");
                _iLogger.LogError(e, e.Message);
                return StatusCode(500, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(FileStreamResult))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFile(string id)
        {
            var file = await _db.CfFiles.FirstOrDefaultAsync<CfFile>(a => a.Id == id);
            if (file == null)
            {
                return NotFound();
            }
            var path = FileOperation.GetFileSavePath(file.Md5, file.Ext, file.Size);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }
            var provide = new FileExtensionContentTypeProvider();
            var success = provide.Mappings.TryGetValue(file.Ext, out string mime);
            if (!success)
            {
                mime = "application/octet-stream";
            }

            var hasCode = id.GetHashCode();
            if (hasCode < 0)
            {
                hasCode = -hasCode;
            }
            var fs = System.IO.File.OpenRead(path);
            var result = new FileStreamResult(fs, mime)
            {
                FileDownloadName = file.FileName
            };
            return result;
        }

		/// <summary>
		/// 检测文件是否已经上传
		/// </summary>
		/// <param name="md5"></param>
		/// <param name="totalSize"></param>
		/// <returns></returns>
		[HttpGet("Check/{md5}/{totalSize}")]
        [ProducesResponseType(500, Type = typeof(ApiResult))]
        [ProducesResponseType(204)]
        [ProducesResponseType(200, Type = typeof(ApiResult))]
        public async Task<IActionResult> GetCheckFile(string md5, long totalSize)
        {
            var result = new ApiResult();
            try
            {
                var fileId = await _db.CfFiles.Where(a => a.Md5 == md5 && a.Size == totalSize).Select(a => a.Id).FirstOrDefaultAsync();
                var exists = !string.IsNullOrWhiteSpace(fileId);

                if (exists)
                {
                    result.Data = fileId;
                    return Ok(result);
                }
                return NoContent();
            }
            catch (Exception e)
            {
                result.Code = 99;
                result.Msg = Guid.NewGuid().GetHashCode().ToString();
                _iLogger.LogError($"error code [{result.Msg}]");
                _iLogger.LogError(e, e.Message);
                return StatusCode(500, result);
            }
        }

        /// <summary>
        /// 检测文件块是否已经上传
        /// </summary>
        /// <returns></returns>
        [HttpGet("Upload")]
        public Task<ApiResult> GetCheckChunk()
        {
            var result = new ApiResult();
            try
            {
                var query = Request.Query;
                var md5 = query["identifier"];
                var totalSize = Convert.ToInt64(query["totalSize"]);
                var chunkNumber = Convert.ToUInt32(query["chunkNumber"]);

                var exists = FileOperation.ExistsChunk(md5, totalSize, chunkNumber - 1);
                result.Data = new { Exists = exists };
                result.Code = 1;
                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                result.Code = 99;
                result.Msg = Guid.NewGuid().GetHashCode().ToString();
                _iLogger.LogError($"error code [{result.Msg}]");
                _iLogger.LogError(e, e.Message);
                return Task.FromResult(result);
            }
        }
    }
}
