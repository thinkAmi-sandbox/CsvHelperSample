using System;

namespace DateConverterSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var runDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = System.IO.Path.Combine(runDir, @"test.csv");

            using (var sr = new System.IO.StreamReader(path))
            using (var csv = new CsvHelper.CsvReader(sr))
            {
                // 手動でCSVをマッピング
                csv.Configuration.RegisterClassMap<Mapper>();
                var records = csv.GetRecords<CsvFile>();

                foreach (var r in records)
                {
                    Console.WriteLine($"date1: {r.Date1}\ndate2: {r.Date3}\ndate3: {r.Date3}");
                }
                Console.ReadKey();
            }
        }
    }


    /// <summary>
    /// CSVファイルをマッピングする先のクラス
    /// </summary>
    public class CsvFile
    {
        public string Name { get; set; }
        public DateTime Date1 { get; set; }
        public DateTime Date2 { get; set; }
        public DateTime Date3 { get; set; }
    }


    /// <summary>
    /// CSVファイルと上記の`CsvFile`クラスをマッピングするためのクラス
    /// </summary>
    public class Mapper : CsvHelper.Configuration.CsvClassMap<CsvFile>
    {
        public Mapper()
        {
            Map(m => m.Name).Index(0);

            // Index()だけだと、「文字列は有効な DateTime ではありませんでした。」エラー
            //Map(m => m.Date1).Index(1);
            Map(m => m.Date1).Index(1).TypeConverter<CsvDateConverter>();

            Map(m => m.Date2).ConvertUsing(row => DateTime.ParseExact(row.GetField<string>(2), "yyyyMMdd", null));
            
            Map(m => m.Date3).Index(3).TypeConverterOption("yyyyMMdd");
        }
    }


    /// <summary>
    /// 日付に関する独自のコンバータークラス
    /// </summary>
    public class CsvDateConverter : CsvHelper.TypeConversion.DateTimeConverter
    {
        public override object ConvertFromString(CsvHelper.TypeConversion.TypeConverterOptions options, string text)
        {
            if (text == null)
            {
                return base.ConvertFromString(options, null);
            }

            if (text.Trim().Length == 0)
            {
                return DateTime.MinValue;
            }
            return DateTime.ParseExact(text, "yyyyMMdd", null);
        }
    }
}
