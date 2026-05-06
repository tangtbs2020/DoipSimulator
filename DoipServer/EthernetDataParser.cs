using System.Text.RegularExpressions;

namespace DOIPUtils
{
    public class EthernetDataParser
    {
        public static List<DataGroup> ParseFile(string filePath)
        {
            return ParseFile(filePath, hasLengthHeader: true);
        }

        public static List<DataGroup> ParseFile(string filePath, bool hasLengthHeader)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                content = Regex.Replace(content, @"(Ans:\s+)1N\s+", "$1", RegexOptions.Multiline);
                Regex regex = new Regex(@"(Req|Ans):\s+(([0-9A-Fa-f]{2}\s*)+)", RegexOptions.Multiline);
                MatchCollection matches = regex.Matches(content);

                List<DataGroup> groups = new List<DataGroup>();
                DataGroup? currentGroup = null;

                foreach (Match match in matches)
                {
                    string type = match.Groups[1].Value;
                    string hexData = match.Groups[2].Value.Trim();

                    var binaryData = HexStringToBinary(hexData, " ");
                    if (!hasLengthHeader)
                        binaryData = PrependDoipHeader(binaryData);

                    if (type == "Req")
                    {
                        if (currentGroup != null && (currentGroup.RequestData != null || currentGroup.ResponseData.Count > 0))
                            groups.Add(currentGroup);
                        currentGroup = new DataGroup();
                        currentGroup.RequestData = binaryData;
                    }

                    if (currentGroup != null && type == "Ans")
                    {
                        currentGroup.ResponseData.Add(binaryData);
                    }
                }

                if (currentGroup != null && (currentGroup.RequestData != null || currentGroup.ResponseData.Count > 0))
                    groups.Add(currentGroup);
                return groups;
            }
            catch (Exception ex)
            {
                throw new Exception($"处理文件{filePath}发生错误：{ex.Message}");
            }
        }

        private static byte[] PrependDoipHeader(byte[] payload)
        {
            int len = payload.Length;
            byte[] result = new byte[len + 4];
            int iDataLen = len - 2; // 减去2字节的协议标识
            result[0] = (byte)(iDataLen >> 24);
            result[1] = (byte)(iDataLen >> 16);
            result[2] = (byte)(iDataLen >> 8);
            result[3] = (byte)(iDataLen);
            Array.Copy(payload, 0, result, 4, len);
            return result;
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