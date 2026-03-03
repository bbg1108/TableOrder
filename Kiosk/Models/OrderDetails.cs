using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.Models
{
    [MessageType(MsgIDEnum.OrderDetails)]
    public class OrderDetails
    {
        //public int OrderNo { get; set; }              // 주문번호 (주방 서버에서 생성)
        public int TableNo { get; set; }
        public int ItemsCount { get; set; }             // Items 개수
        public OrderItem[] Items { get; set; }
        public int TotalPrice { get; set; }
        public DateTimeOffset OrderTime { get; set; }
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
    }
}
