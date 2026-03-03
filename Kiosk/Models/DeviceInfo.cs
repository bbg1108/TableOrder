using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.Models
{
    [MessageType(MsgIDEnum.DeviceInfo)]
    public struct DeviceInfo
    {
        public int TableNo { get; }     // 테이블 번호

        public DeviceInfo(int num)
        {
            TableNo = num;
        }
    }
}
