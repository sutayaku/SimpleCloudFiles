namespace SimpleCloudFiles.Dtos
{
    /// <summary>
    /// WebApi通用返回数据格式
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 返回码 1 成功，0 失败
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 相关信息
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 返回数据对象
        /// </summary>
        public object Data { get; set; }
    }
}
