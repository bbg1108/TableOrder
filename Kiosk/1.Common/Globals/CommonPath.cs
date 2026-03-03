using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk
{
    public static class CommonPath
    {
        // 실행 exe파일 위치 경로
        public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 이미지 경로
        public static readonly string ImageDir = Path.Combine(BaseDir, "../../1.Common/Resources/Images/");

        // 음성인식 키워드 목록 json 경로
        public static readonly string SpeechKeywordListJsonPath = Path.Combine(BaseDir, "../../1.Common/Utils/Speech/STT/speechCommands.json");
    }
}
