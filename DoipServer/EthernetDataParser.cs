using System.Text.RegularExpressions;

namespace DOIPUtils
{
    public class EthernetDataParser
    {
        public static List<DataGroup> ParseFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                // 使用正则表达式匹配 Req 和 Ans 及其后的数据
                // 匹配模式：Req: 或 Ans: 后跟至少一个空格，然后是十六进制字符组（如 00 00 00...）
                Regex regex = new Regex(@"(Req|Ans):\s+(([0-9A-Fa-f]{2}\s*)+)", RegexOptions.Multiline);
                MatchCollection matches = regex.Matches(content);

                List<DataGroup> groups = new List<DataGroup>();
                DataGroup? currentGroup = null;

                foreach (Match match in matches)
                {
                    string type = match.Groups[1].Value; // "Req" or "Ans"
                    string hexData = match.Groups[2].Value.Trim();

                    var binaryData = HexStringToBinary(hexData, " "); // 将十六进制字符串转换为二进制数据

                    if (type == "Req")
                    {
                        // 遇到新的 Req，保存上一组（如果有数据），并创建新组
                        if (currentGroup != null && (currentGroup.RequestData != null || currentGroup.ResponseData.Count > 0))
                        {
                            groups.Add(currentGroup);
                        }
                        currentGroup = new DataGroup();
                        currentGroup.RequestData = binaryData;
                    }

                    if (currentGroup != null && type == "Ans")
                    {
                        // 将 Ans 数据添加到当前组的响应列表中
                        currentGroup.ResponseData.Add(binaryData);
                    }
                }

                // 添加最后一组
                if (currentGroup != null && (currentGroup.RequestData != null || currentGroup.ResponseData.Count > 0))
                {
                    groups.Add(currentGroup);
                }
                return groups;
            }
            catch (Exception ex)
            {
                throw new Exception($"处理文件{filePath}发生错误：{ex.Message}");
            }

        }

        // 辅助方法：将连续的16进制字符串格式化为带空格的形式 (例如: "0001" -> "00 01")
        public static string FormatHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return hex;
            // 确保长度为偶数
            if (hex.Length % 2 != 0) hex = "0" + hex;

            List<string> chunks = new List<string>();
            for (int i = 0; i < hex.Length; i += 2)
            {
                chunks.Add(hex.Substring(i, 2));
            }
            return string.Join(" ", chunks).ToUpper();
        }

        public static byte[] HexStringToBinary(string hex,string split)
        {

            var array = hex.Trim().Split(split, StringSplitOptions.RemoveEmptyEntries);
            var data = new byte[array.Length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Convert.ToByte(array[i], 16);
            }
            return data;
        }
    }

    // 定义一个类来存储一组日志
    public class DataGroup
    {
        public byte[] RequestData { get; set; } = Array.Empty<byte>();
        public List<byte[]> ResponseData { get; set; } = new List<byte[]>();

    }
}