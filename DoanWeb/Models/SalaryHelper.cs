namespace DoanWeb.Models
{
    public static class SalaryHelper
    {
        public static (int? Min, int? Max) ParseSalary(string salaryStr)
        {
            if (string.IsNullOrWhiteSpace(salaryStr))
                return (null, null);

            salaryStr = salaryStr.ToLower().Trim();

            // Trường hợp "liên hệ"
            if (salaryStr.Contains("liên hệ") || salaryStr.Contains("thoả thuận") || salaryStr.Contains("thỏa thuận"))
                return (null, null);

            // Chuẩn hóa ký tự
            salaryStr = salaryStr
                .Replace("đ", "")
                .Replace("vnd", "")
                .Replace("₫", "")
                .Replace("vnđ", "")
                .Replace("/tháng", "")
                .Replace("/ tháng", "")
                .Replace(" ", "");

            // Xử lý đơn vị triệu hoặc tr
            bool isTrieu = salaryStr.Contains("triệu") || salaryStr.Contains("tr");

            // Xóa chữ đơn vị để dễ tách số
            salaryStr = salaryStr.Replace("triệu", "").Replace("tr", "");

            // Trường hợp khoảng lương
            if (salaryStr.Contains("-"))
            {
                var parts = salaryStr.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out double min) &&
                    double.TryParse(parts[1], out double max))
                {
                    // Nếu là triệu thì nhân 1_000_000
                    if (isTrieu)
                    {
                        min *= 1_000_000;
                        max *= 1_000_000;
                    }
                    return ((int)min, (int)max);
                }
            }

            // Trường hợp chỉ có 1 giá trị
            if (double.TryParse(salaryStr, out double value))
            {
                if (isTrieu)
                    value *= 1_000_000;
                return ((int)value, (int)value);
            }

            // Nếu không parse được
            return (null, null);
        }
    }
}
