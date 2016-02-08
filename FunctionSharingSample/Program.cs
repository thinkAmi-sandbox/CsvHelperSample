using System;

namespace FunctionSharingSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var runDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var datePath = System.IO.Path.Combine(runDir, @"date.csv");
            var contentPath = System.IO.Path.Combine(runDir, @"content.csv");

            //--- Type.GetType()版
            // date.csvを読み込んだ時の独自処理
            Action<object> dateActionByObject = obj =>
            {
                var record = (DateCsv)obj;
                Console.WriteLine($"date.csvの中身：{record.Name} - {record.Date}");
            };

            // content.csvを読み込んだ時の独自処理
            Action<object> contentActionByObject = obj =>
            {
                var record = (ContentCsv)obj;
                Console.WriteLine($"content.csvの中身：{record.Name} - {record.Content}");
            };

            Console.WriteLine("--- Type.GetType()版 ---");
            ImportUsingGetType(dateActionByObject, datePath, "FunctionSharingSample.DateCsv", "FunctionSharingSample.DateMapper");
            ImportUsingGetType(contentActionByObject, contentPath, "FunctionSharingSample.ContentCsv", "FunctionSharingSample.ContentMapper");


            //--- typeof演算子版
            Action<DateCsv> dateActionByGeneric = obj =>
            {
                Console.WriteLine($"date.csvの中身：{obj.Name} - {obj.Date}");
            };

            Action<ContentCsv> contentActionByGeneric = obj =>
            {
                Console.WriteLine($"content.csvの中身：{obj.Name} - {obj.Content}");
            };

            Console.WriteLine("--- typeof演算子版 ---");
            ImportUsingTypeof<DateCsv, DateMapper>(dateActionByGeneric, datePath);
            ImportUsingTypeof<ContentCsv, ContentMapper>(contentActionByGeneric, contentPath);


            //--- 型引数(型パラメータ)版
            Console.WriteLine("--- 型引数版 ---");
            ImportUsingTypeParameter<DateCsv, DateMapper>(dateActionByGeneric, datePath);
            ImportUsingTypeParameter<ContentCsv, ContentMapper>(contentActionByGeneric, contentPath);

            Console.ReadLine();
        }

        /// <summary>
        /// 【Type.GetType版】ファイルインポート時の共通処理
        /// </summary>
        /// <param name="action">ファイルフォーマットごとの独自処理</param>
        /// <param name="path">ファイルパス</param>
        /// <param name="csvClassName">マッピングする先のクラス名(名前空間付)</param>
        /// <param name="mapperClassName">マッピングクラス名(名前空間付)</param>
        public static void ImportUsingGetType(Action<object> action, string path, string csvClassName, string mapperClassName)
        {
            using (var sr = new System.IO.StreamReader(path))
            using (var csv = new CsvHelper.CsvReader(sr))
            {
                csv.Configuration.RegisterClassMap(Type.GetType(mapperClassName));
                var records = csv.GetRecords(Type.GetType(csvClassName));

                foreach (var record in records)
                {
                    // Action<object>で渡された、ファイルフォーマットごとの独自処理
                    // GetRecords()の戻り値はIEnumerable<object>になるため、
                    // 独自処理の型はAction<object>
                    action(record);
                }
            }
        }


        /// <summary>
        /// 【typeof演算子版】ファイルインポート時の共通処理
        /// </summary>
        /// <typeparam name="TCsv">マッピングする先のクラス</typeparam>
        /// <typeparam name="TMapper">マッピングクラス</typeparam>
        /// <param name="action">ファイルフォーマットごとの独自処理</param>
        /// <param name="path">ファイルパス</param>
        public static void ImportUsingTypeof<TCsv, TMapper>(Action<TCsv> action, string path)
        {
            using (var sr = new System.IO.StreamReader(path))
            using (var csv = new CsvHelper.CsvReader(sr))
            {
                // typeof演算子を使って、Typeを取得する
                csv.Configuration.RegisterClassMap(typeof(TMapper));
                var records = csv.GetRecords(typeof(TCsv));

                foreach (TCsv record in records)
                {
                    // 読込レコードはTCsvになるため、
                    // 独自処理の型はAction<csvClassType>
                    action(record);
                }
            }
        }


        /// <summary>
        /// 【型引数版】ファイルインポート時の共通処理
        /// </summary>
        /// <typeparam name="TCsv">マッピングする先のクラス</typeparam>
        /// <typeparam name="TMapper">マッピングクラス</typeparam>
        /// <param name="action">ファイルフォーマットごとの独自処理</param>
        /// <param name="path">ファイルパス</param>
        public static void ImportUsingTypeParameter<TCsv, TMapper>(Action<TCsv> action, string path)
            // where無しだと、コンパイルエラー"CS0314"が発生する
            where TMapper : CsvHelper.Configuration.CsvClassMap
        {
            using (var sr = new System.IO.StreamReader(path))
            using (var csv = new CsvHelper.CsvReader(sr))
            {
                csv.Configuration.RegisterClassMap<TMapper>();
                var records = csv.GetRecords<TCsv>();

                foreach (var record in records)
                {
                    // GetRecords()の戻り値はIEnumerable<TCsv>になるため、
                    // 独自処理の型はAction<TCsv>
                    action(record);
                }
            }
        }
    }


    //--- マッピングする先のクラス
    public class DateCsv
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
    public class ContentCsv
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }


    //--- マッピングクラス
    public class DateMapper : CsvHelper.Configuration.CsvClassMap<DateCsv>
    {
        public DateMapper()
        {
            Map(m => m.Name).Index(0);
            Map(m => m.Date).Index(1).TypeConverterOption("yyyyMMdd");
        }
    }
    public class ContentMapper : CsvHelper.Configuration.CsvClassMap<ContentCsv>
    {
        public ContentMapper()
        {
            Map(m => m.Name).Index(0);
            Map(m => m.Content).Index(1);
        }
    }
}