using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Umwelt.Faculty.Csv.Algorithm2
{
    class Faculty
    {
        private readonly string _inputPath;
        private readonly string _outputPath;

        public Faculty(IConfiguration configuration)
        {
            (_inputPath, _outputPath) = Initializer.InFileOutFile(configuration);
            var incar = configuration.GetSection("INCAR");
            // ここで設定を読み取ります。
        }

        public async Task ExecuteAsync()
        {
            // ここにアルゴリズムの処理を書きます。
        }
    }
}
