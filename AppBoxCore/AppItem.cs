using System.ComponentModel;
using System.IO;
using YamlDotNet.Serialization;

namespace AppBox
{

    public enum AppItemType { 应用, 快链, EXE, 应用包 };
    /// <summary>
    /// App 项
    /// 应用在文件夹内具有多个文件，其他为单文件
    /// 应用路径关联版本等属性
    /// </summary>
    public class AppItem : INotifyPropertyChanged
    {
        /// <summary>
        /// APP名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 全局APP名
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 启动文件
        /// </summary>
        public string InitFile { get; set; }
        /// <summary>
        /// 描述说明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 图标信息
        /// </summary>
        public string IconDept { get; set; }
        [YamlIgnore]
        public string InfoWithDescription { get=> this.DescriptionString() + Description;  set { } }
        [YamlIgnore]
        public bool IsKeySelected { get; set; } = true;
        [YamlIgnore]
        public bool IsEnable { get => AppItemType == AppItemType.应用; }
        [YamlIgnore]
        public AppItemType AppItemType { get => (AppItemType)AppItemTypeCode; set => AppItemTypeCode = (int)value; }
        public int AppItemTypeCode { get; set; } = 0;
        public string Size { get; set; }
        /// <summary>
        /// 分类名
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 不存在文件用本地图标
        /// </summary>
        /// <returns></returns>
        [YamlIgnore]
        public object Icon
        {
            get
            {
                return this.GetIconByIconDept();
            }
            set => _= value;
        }
        [YamlIgnore]
        public static object WaitInstallIcon
        {
            get; set;
        }

        public string IconFile { get; set; }
        [YamlIgnore]
        public string Status { get; set; }

        public AppItem()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return $"{Name}-{Version}";
        }
        public string DescriptionString()
        {
            return $@"应用名称:
    {Name}
Uri唯一名称：
    {Uri}
版本：
    {Version}
大小：
    {Size}
包类型：
    {AppItemType}
分类名：
    {Category}

    ";
        }

    }
    public class AppItemInstallConfig
    {
        [Category("配置")]
        [DisplayName("应用启动文件")]
        [Description("启动文件路径")]
        [ReadOnly(true)]
        public string InitFile { get; set; }
        [Category("配置")]
        [DisplayName("APP全局名称")]
        [Description("全局唯一名称，不支持特殊字符")]
        public string Uri { get; set; }
        [Category("配置")]
        [DisplayName("名称")]
        [Description("列表中显示的名称，支持特殊字符")]
        public string Name { get; set; }
        [Category("配置")]
        [DisplayName("版本号")]
        [Description("APP全局名称重复将用版本号区分")]
        public string Version { get; set; }
        [Category("配置")]
        [DisplayName("分类名")]
        [Description("列表中分类名称")]
        public string Category { get; set; } = "";
        [Category("配置")]
        [DisplayName("移动文件")]
        [Description("移动方式导入将删除源文件/文件夹")]
        public bool IsMove { get; set; } = true;
        public string Description { get; set; }
    }
    public class AppItemConfig
    {
        [Category("配置")]
        [DisplayName("名称")]
        [Description("列表中显示的名称，支持特殊字符")]
        public string Name { get; set; }
        [Category("配置")]
        [DisplayName("分类名")]
        [Description("列表中分类名称")]
        public string Category { get; set; } = "";
        [Category("配置")]
        [DisplayName("描述说明")]
        [Description("描述App的特点、场景、已知问题")]
        public string Description { get; set; } = "";
    }
}