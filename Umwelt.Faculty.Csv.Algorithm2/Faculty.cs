using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Umwelt.Faculty.Csv.Algorithm2.Models;

namespace Umwelt.Faculty.Csv.Algorithm2
{
    class Faculty
    {
        private readonly string _inputPath;
        private readonly string _outputPath;
        private readonly string[] _targetGroupHeaders;
        private readonly string[] _targetCalcHeaders;
        private readonly string[] _outputIndexes;

        private class GroupData
        {
            public string[]? ParamList { get; set; }
        }


        public Faculty(IConfiguration configuration)
        {
            (_inputPath, _outputPath) = Initializer.InFileOutFile(configuration);
            var incar = configuration.GetSection("INCAR");
            // ここで設定を読み取ります。

            _targetGroupHeaders = incar["GroupHeaders"].Split(',').Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            _targetCalcHeaders = incar["CalcHeaders"].Split(',').Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            _outputIndexes = incar["OutputIndexes"].Split(',').Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        }

        public dynamic calc()
        {
            return new { total = 600, cate1 = 100, cate2 = 200, cate3 = 300 };
        }

        private static string[] extractArray(string[] target, int[] cols)
        {
            string[] ret = new string[cols.Count()];
            int i = 0;
            foreach (var col in cols)
            {
                ret[i] = target[col];
                i++;
            }
            return ret;
        }

        public async Task ExecuteAsync()
        {
            // ここにアルゴリズムの処理を書きます。

            //TODO
            //1.指定カラムでグループ化する
            //2.別の指定カラムで指定の指標（合計、平均、標準偏差等）を出力(付加)する

            using var reader = Csv.OpenRead(_inputPath);
            using var writer = Csv.Create(_outputPath);

            var records = new List<string[]>();

            reader.Read();//1行バッファよみ
            reader.ReadHeader();//ヘッダー化

            while (reader.Read())//1行バッファよみ
            {
                var record = reader.GetFields().ToArray();
                if (record == null) continue;

                records.Add(record);
            }

 

            //GroupByお試しコード
            //int[] cols = new int[2] {0, 1};
            //var list = new List<string[]>()
            //{
            //    new[] {"a", "z", "2" },
            //    new[] {"a", "z", "2" },
            //    new[] {"a", "z", "1" },
            //};
            //var groupList2 = list.GroupBy(x => convertArray(x, cols), StringArrayEqualityComparer.Default);
            //foreach (var group in groupList2)
            //{
            //    System.Console.WriteLine("group:{0}", group.Key);
            //    foreach (var value in group)
            //    {
            //        System.Console.WriteLine("\t{0}", value);
            //    }
            //}

            List<string[]> sortedRecords = new List<string[]>(records);
            int[] indexes = new int[_targetGroupHeaders.Count()];
            int i = 0;
            foreach (var header in _targetGroupHeaders)
            {
                if (header == null) break;

                int index = 0;
                int j = 0;
                foreach (var a in reader.HeaderRecord)
                {
                    if (a == header)
                    {
                        index = j;
                        break;
                    }
                    j++;
                }

                indexes[i] = index;
                i++;
            }


            var groupList = sortedRecords.GroupBy(x => Faculty.extractArray(x, indexes), StringArrayEqualityComparer.Default);
            //foreach (var group in groupList)
            //{
            //    System.Console.WriteLine("group:{0}", group.Key);
            //    foreach (var value in group)
            //    {
            //        System.Console.WriteLine("\t{0}", value);
            //    }
            //}

            var results = sortedRecords.GroupBy(x => Faculty.extractArray(x, indexes), StringArrayEqualityComparer.Default)
                                       .Select(x => {
                                            return new
                                            {
                                                Key = x.Key,
                                                Data = x,
                                                Sum = x.Select(y => Convert.ToDecimal(y[2])).Sum(),
                                                Ave = x.Select(y => Convert.ToDecimal(y[2])).Average(),
                                            };
                                        }).ToList();

            foreach (var result in results)
            {
                //System.Console.WriteLine("group:{0}", group.Key);
                //foreach (var value in group)
                //{
                //    System.Console.WriteLine("\t{0}", value);
                //}
            }



            writer.WriteFields(reader.HeaderRecord);
            writer.NextRecord();

            foreach (var record in sortedRecords)
            {
                writer.WriteFields(record);
                writer.NextRecord();
            }

        }
    }
}
