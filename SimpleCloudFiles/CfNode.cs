using System;
using System.Collections.Generic;

namespace SimpleCloudFiles
{
    public class CfNode
    {
        public CfNode()
        {
            Children = new List<CfNode>();
        }
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 节点名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 类型dir, file
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public List<CfNode> Children { get; set; }
    }
}
